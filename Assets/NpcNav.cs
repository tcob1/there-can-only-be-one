using UnityEngine;
using UnityEngine.AI;

public class NpcNav : MonoBehaviour
{
    public enum NPCState { Wandering, Running }

    [Header("Patrol")]
    public Transform[] wanderPoints;
    private int currentPointIndex = 0;

    [Header("Detection")]
    public float hearingRange = 50f;
    public float detectRange = 100f;
    public float detectAngle = 90f;
    public float calmDownTime = 5f;

    [Header("Performance")]
    [Tooltip("Seconds between each vision/threat scan. Stagger this per NPC.")]
    public float scanInterval = 0.5f;
    [Tooltip("Beyond this distance from player, NPC skips all updates.")]
    public float cullingDistance = 80f;

    public NPCState currentNPCState = NPCState.Wandering;

    private NavMeshAgent agent;
    private float baseSpeed;
    private float baseAcceleration;
    private float baseAngularSpeed;

    private float lastThreatTime = -Mathf.Infinity;
    private float lastScanTime = 0f;
    private Transform trackedThreat = null;
    private bool isCulled = false;

    private static readonly Collider[] scanBuffer = new Collider[32];
    private static int threatLayerMask = -1;
    private static Transform playerTransform;
    private float cullingDistanceSqr;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (wanderPoints.Length > 0)
            agent.SetDestination(wanderPoints[currentPointIndex].position);

        baseSpeed = agent.speed;
        baseAcceleration = agent.acceleration;
        baseAngularSpeed = agent.angularSpeed;

        lastScanTime = Time.time + Random.Range(0f, scanInterval);

        if (threatLayerMask == -1)
            threatLayerMask = LayerMask.GetMask("NPC", "Player");

        cullingDistanceSqr = cullingDistance * cullingDistance;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        GlobalEvents.Instance.OnPlayerShoot += OnHearNoise;
    }

    void OnDestroy()
    {
        if (GlobalEvents.Instance != null)
            GlobalEvents.Instance.OnPlayerShoot -= OnHearNoise;
    }

    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;
            bool shouldCull = sqrDist > cullingDistanceSqr;

            if (shouldCull != isCulled)
            {
                isCulled = shouldCull;
                agent.enabled = !shouldCull;

                if (!isCulled) // just re-enabled, refresh destination
                {
                    if (currentNPCState == NPCState.Wandering && wanderPoints.Length > 0)
                        agent.SetDestination(wanderPoints[currentPointIndex].position);
                    else if (currentNPCState == NPCState.Running && trackedThreat != null)
                        agent.SetDestination(trackedThreat.position);
                }
            }

            if (isCulled) return;
        }

        float simScale = TimeHub.Instance.CurrentSimScale;
        agent.speed = baseSpeed * simScale;
        agent.acceleration = baseAcceleration * Mathf.Min(simScale * 4f, 100f);
        agent.angularSpeed = baseAngularSpeed * Mathf.Min(simScale * 10f, 720f);

        switch (currentNPCState)
        {
            case NPCState.Wandering:
                ThrottledScan();
                HandleWandering();
                break;

            case NPCState.Running:
                ThrottledScan();
                HandleRunning();
                CheckCalmDown();
                break;
        }
    }

    private void ThrottledScan()
    {
        if (Time.time - lastScanTime < scanInterval) return;
        lastScanTime = Time.time;
        ScanForArmedEntities();
    }

    private void ScanForArmedEntities()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectRange, scanBuffer, threatLayerMask);

        float halfAngle = detectAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Collider col = scanBuffer[i];
            if (col.gameObject == gameObject) continue;

            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) > halfAngle) continue;

            Inventory entity_inv = col.GetComponent<Inventory>();
            if (entity_inv == null) continue;

            if (IsGun(entity_inv.currentHeldItem))
            {
                trackedThreat = col.transform;
                lastThreatTime = Time.time;
                if (currentNPCState != NPCState.Running)
                    SwapToRunning();
                return;
            }
        }
    }

    private bool IsGun(GameObject heldItem)
    {
        if (heldItem == null) return false;
        return heldItem.GetComponent<Gun>() != null;
    }

    private void OnHearNoise(Vector3 noisePosition)
    {
        float distSq = (transform.position - noisePosition).sqrMagnitude;
        if (distSq <= hearingRange * hearingRange)
        {
            lastThreatTime = Time.time;
            if (currentNPCState != NPCState.Running)
                SwapToRunning();
        }
    }

    private void SwapToRunning()
    {
        currentNPCState = NPCState.Running;
    }

    private void SwapToWandering()
    {
        currentNPCState = NPCState.Wandering;
        trackedThreat = null;
        if (wanderPoints.Length > 0)
            agent.SetDestination(wanderPoints[currentPointIndex].position);
    }

    private void CheckCalmDown()
    {
        if (Time.time - lastThreatTime >= calmDownTime)
            SwapToWandering();
    }

    private void HandleWandering()
    {
        if (agent.pathPending || wanderPoints.Length == 0) return;
        if (agent.remainingDistance < 2.0f)
        {
            currentPointIndex = (currentPointIndex + 1) % wanderPoints.Length;
            agent.SetDestination(wanderPoints[currentPointIndex].position);
        }
    }

    private Vector3 cachedFleeDestination;
    private Vector3 lastThreatPos;
    private float fleePosUpdateThresholdSqr = 9f;

    private void HandleRunning()
    {
        Vector3 fleeOrigin = trackedThreat != null
            ? trackedThreat.position
            : transform.position;

        float sqrMoved = (fleeOrigin - lastThreatPos).sqrMagnitude;
        if (sqrMoved < fleePosUpdateThresholdSqr && cachedFleeDestination != Vector3.zero)
            return;

        lastThreatPos = fleeOrigin;

        Vector3 bestPos = transform.position;
        float bestDist = 0f;
        const int samples = 6;

        for (int i = 0; i < samples; i++)
        {
            float angle = i * (360f / samples);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 candidate = transform.position + dir * detectRange;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, detectRange, NavMesh.AllAreas))
            {
                float dist = (fleeOrigin - hit.position).sqrMagnitude;
                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestPos = hit.position;
                }
            }
        }

        cachedFleeDestination = bestPos;
        agent.SetDestination(bestPos);
    }
}