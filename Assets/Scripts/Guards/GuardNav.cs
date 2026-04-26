using System.Timers;
using UnityEngine;
using UnityEngine.AI;

public class GuardNav : MonoBehaviour
{
    public enum GuardState
    {
        Patrolling,
        Chasing,
        Inspecting,
        Searching,
        Shooting
    }

    public Transform[] patrolPoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;

    public GameObject player;
    public GameObject guard;
    public Inventory inv;
    private Vector3 lastKnownPlayerPosition;
    public float detectRange = 100;
    public float hearingRange = 50;
    public float forgetRange = 150;
    public float stopAndShootRange = 10;
    public float detectAngle = 90;

    private bool isInAngle = false;
    private bool isInDetectRange = false;
    private bool isVisible = false;

    private GameObject targetInteractable = null;
    public float pickupRange = 2.0f;
    public LayerMask InteractableLayerMask;

    public GuardState currentGuardState = GuardState.Patrolling;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(patrolPoints[currentPointIndex].position);

        GlobalEvents.Instance.OnPlayerShoot += OnHearNoise;
    }

    void OnEnable()
    {
        //Debug.Log(GlobalEvents.Instance);
        //GlobalEvents.Instance.OnPlayerShoot += OnHearNoise;
    }

    void OnDisable()
    {
        GlobalEvents.Instance.OnPlayerShoot -= OnHearNoise;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdatePlayerDetection();

        switch (currentGuardState)
        {
            case GuardState.Patrolling:
                HandlePatrolling();
                break;
            case GuardState.Chasing:
                HandleChasing();
                break;
            case GuardState.Searching:
                HandleSearching();
                break;
            case GuardState.Shooting:
                HandleShooting();
                break;
            case GuardState.Inspecting:
                HandleInspecting();
                break;
        }
    }

    private void UpdatePlayerDetection()
    {
        isInAngle = false;
        isInDetectRange = false;
        isVisible = false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= detectRange)
        {
            isInDetectRange = true;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit, Mathf.Infinity))
        {
            if (hit.transform == player.transform)
            {
                isVisible = true;
            }
        }

        float angle = Vector3.Angle(transform.forward, player.transform.position - transform.position);
        if (angle < detectAngle)
        {
            isInAngle = true;
        }
    }

    private void SwapToPatrolling()
    {
        Debug.Log("Swapping to patrolling");
        currentGuardState = GuardState.Patrolling;
        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
        lastKnownPlayerPosition = Vector3.zero;
    }

    private void HandlePatrolling()
    {
        if (isInAngle && isInDetectRange && isVisible)
        {
            SwapToChasing();
            return;
        }

        GameObject interactable = FindInteractableInView();
        if (interactable != null)
        {
            SwapToInspecting(interactable);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 2.0f)
        {
            if (patrolPoints.Length == 0)
            {
                return;
            }
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }

    private void SwapToChasing()
    {
        Debug.Log("Swapping to chasing");
        currentGuardState = GuardState.Chasing;
        inv.GuardEquipByName("Gun");
        lastKnownPlayerPosition = player.transform.position;
        agent.SetDestination(lastKnownPlayerPosition);
    }

    private void HandleChasing()
    {
        if (isVisible)
        {
            // If we can see the player, move forward until we can shoot, then shoot
            lastKnownPlayerPosition = player.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= stopAndShootRange)
            {
                SwapToShooting();
            }
            else
            {
                agent.SetDestination(player.transform.position);
            }
        }
        else
        {
            // If we lost sight of the player, move to the last known position to try to find them again
            SwapToSearching();
        }

        // In any case, if the player gets too far, stop chasing
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > forgetRange)
        {
            SwapToPatrolling();
        }
    }

    private void SwapToSearching()
    {
        Debug.Log("Swapping to searching");
        currentGuardState = GuardState.Searching;
        agent.SetDestination(lastKnownPlayerPosition); // Stop moving to search
    }

    private void HandleSearching()
    {
        // We lost sight of the player, move to the last known position.
        // If we find them, swap to chasing.
        // If we get there and still can't see them, go back to patrolling
        if (isVisible && isInAngle && isInDetectRange)
        {
            SwapToChasing();
            return;
        }

        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 1.0f)
        {
            agent.SetDestination(lastKnownPlayerPosition);
        }
        else
        {
            // One last look around before giving up, this time we let the guard look all around them
            if (isVisible && isInDetectRange)
            {
                SwapToChasing();
            }
            else
            {
                SwapToPatrolling();
            }
        }
    }

    private void SwapToShooting()
    {
        Debug.Log("Swapping to shooting");
        currentGuardState = GuardState.Shooting;
        agent.SetDestination(transform.position); // Stop moving to shoot
    }

    private void HandleShooting()
    {
        // Shooting logic handled in GuardPistol, so just check if we should stop shooting
        if (isVisible)
        {
            transform.LookAt(player.transform.position); // Always face the player when shooting

            // If the guard can see the player, either keep shooting or chase if they get too far
            lastKnownPlayerPosition = player.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > stopAndShootRange)
            {
                SwapToChasing();
            }
            else
            {
                agent.SetDestination(transform.position); // Stay still to shoot
            }
        }
        else
        {
            // If we lost sight of the player, move back to chasing to try to find them again
            SwapToChasing();
        }
    }

    private void OnHearNoise(Vector3 noisePosition)
    {
        if (currentGuardState == GuardState.Patrolling || currentGuardState == GuardState.Searching)
        {
            float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
            if (distanceToNoise <= hearingRange)
            {
                lastKnownPlayerPosition = noisePosition;
                SwapToSearching();
            }
        }
    }

    private void SwapToInspecting(GameObject interactable)
    {
        Debug.Log("Swapping To Inspecting");
        currentGuardState = GuardState.Inspecting;
        targetInteractable = interactable;
        agent.SetDestination(targetInteractable.transform.position);
    }

    private void HandleInspecting()
    {
        if (isInAngle && isInDetectRange && isVisible)
        {
            targetInteractable = null;
            SwapToChasing();
            return;
        }

        if (targetInteractable == null)
        {
            SwapToPatrolling();
            return;
        }

        float distToObj = Vector3.Distance(transform.position, targetInteractable.transform.position);
        if (distToObj <= pickupRange)
        {
            Interactable inter = targetInteractable.GetComponent<Interactable>();
            if (inter != null)
            {
                Debug.Log($"Picking Up Item: {targetInteractable.name}");
            }
            inter?.Interact(guard);
            Destroy(targetInteractable);
            targetInteractable = null;
            SwapToPatrolling();
        } 
        else
        {
            agent.SetDestination(targetInteractable.transform.position);
        }
    }

    private GameObject FindInteractableInView()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange);
        foreach (Collider coll in hits)
        {
            if (!coll.CompareTag("Interactable"))
            {
                continue;
            }

            Vector3 dirToObj = coll.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dirToObj);
            if (angle < detectAngle)
            {
                continue;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dirToObj.normalized, out hit, detectRange))
            {
                if (hit.collider == coll)
                {
                    return coll.gameObject;
                }
            }
        }

        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);

        Vector3 origin = transform.position;
        int rayCount = 30;

        float halfAngle = detectAngle;
        float angleStep = (halfAngle * 2) / rayCount;

        Vector3 previousPoint = origin;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -halfAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Vector3 currentPoint = origin + direction * detectRange;

            Gizmos.DrawLine(origin, currentPoint);

            if (i > 0)
                Gizmos.DrawLine(previousPoint, currentPoint);

            previousPoint = currentPoint;
        }
    }
}
