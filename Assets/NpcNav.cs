using UnityEngine;
using UnityEngine.AI;

public class NpcNav : MonoBehaviour
{
    public enum NPCState { Wandering, Running }
    public GameObject player;
    public DialogueTrigger dialogueTrigger;

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
    public float scanInterval = 0.2f;

    public NPCState currentNPCState = NPCState.Wandering;

    private NavMeshAgent agent;
    private float baseSpeed;
    private float baseAcceleration;
    private float baseAngularSpeed;

    private float lastThreatTime = -Mathf.Infinity;
    private float lastScanTime = 0f;
    private Transform trackedThreat = null;

    private bool dialogueTriggered = false;

    // Reusable buffer shared across ALL NPC instances to avoid per-scan allocation
    private static readonly Collider[] scanBuffer = new Collider[32];

    // Layer mask set in Start to only scan relevant layers
    private static int threatLayerMask = -1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (wanderPoints.Length > 0)
            agent.SetDestination(wanderPoints[currentPointIndex].position);

        baseSpeed = agent.speed;
        baseAcceleration = agent.acceleration;
        baseAngularSpeed = agent.angularSpeed;

        // Randomise first scan so all NPCs don't all scan on frame 1
        lastScanTime = Time.time + Random.Range(0f, scanInterval);

        // Build layer mask once 
        if (threatLayerMask == -1)
            threatLayerMask = LayerMask.GetMask("Entity", "Player");

        GlobalEvents.Instance.OnPlayerShoot += OnHearNoise;
    }

    void OnDestroy()
    {
        if (GlobalEvents.Instance != null)
            GlobalEvents.Instance.OnPlayerShoot -= OnHearNoise;
    }

    void FixedUpdate()
    {
        float simScale = TimeHub.Instance.CurrentSimScale;
        agent.speed = baseSpeed * simScale;
        agent.acceleration = baseAcceleration * simScale * 4f;
        agent.angularSpeed = baseAngularSpeed * simScale * 10f;

        switch (currentNPCState)
        {
            case NPCState.Wandering:
                ThrottledScan();
                HandleWandering();
                HandleDialogue();
                break;

            case NPCState.Running:
                ThrottledScan();
                HandleRunning();
                CheckCalmDown();
                break;
        }
    }

    private void HandleDialogue()
    {
        if (player)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < 10f && !dialogueTriggered)
            {
                dialogueTriggered = true;
                dialogueTrigger.Interact();
            }
        }
    }

    private void ThrottledScan()
    {
        if (Time.time - lastScanTime < scanInterval) return;
        lastScanTime = Time.time;
        ScanForArmedEntities();
    }

    // Scans all nearby entities within detectRange and detectAngle for anyone holding a gun
    private void ScanForArmedEntities()
    {
        // Grab every collider in range
        Collider[] nearby = Physics.OverlapSphere(transform.position, detectRange);

        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            // Angle check
            Vector3 dirToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle > detectAngle * 0.5f) continue;

            // Gun check via currentHeldItem
            Inventory entity_inv = col.GetComponent<Inventory>();

            if (entity_inv == null) continue;

            if (IsGun(entity_inv.currentHeldItem))
            {
                trackedThreat = col.transform;

                lastThreatTime = Time.time;

                if (currentNPCState != NPCState.Running)
                    SwapToRunning();

                return; // One armed entity is enough to stay alarmed
            }
        }
    }

    private bool IsGun(GameObject heldItem)
    {
        if (heldItem == null) return false;
        return heldItem.name == "Pistol(Clone)";
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

    // Cached flee destination to only recalculate when threat moves significantly
    private Vector3 cachedFleeDestination;
    private Vector3 lastThreatPos;
    private const float fleePosUpdateThreshold = 3f;

    private void HandleRunning()
    {
        Vector3 fleeOrigin = trackedThreat != null
            ? trackedThreat.position
            : transform.position;

        // Only re-sample if the threat has moved enough to matter
        if (Vector3.Distance(fleeOrigin, lastThreatPos) < fleePosUpdateThreshold
            && cachedFleeDestination != Vector3.zero)
        {
            return;
        }

        lastThreatPos = fleeOrigin;

        Vector3 bestPos = transform.position;
        float bestDist = 0f;
        const int samples = 8;

        for (int i = 0; i < samples; i++)
        {
            float angle = i * (360f / samples);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 candidate = transform.position + dir * detectRange;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, detectRange, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(fleeOrigin, hit.position);
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