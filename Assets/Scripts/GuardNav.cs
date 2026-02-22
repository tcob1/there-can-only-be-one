using UnityEngine;
using UnityEngine.AI;

public class GuardNav : MonoBehaviour
{
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;

    bool isInAngle, isInRange, isNotHidden;
    public GameObject Player;
    public float DetectRange = 100;
    public float DetectAngle = 60;

    bool isChasing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    // Update is called once per frame
    void Update()
    {
        isInAngle = false;
        isInRange = false;
        isNotHidden = false;

        if (Vector3.Distance(transform.position, Player.transform.position) < DetectRange)
        {
            isInRange = true;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, (Player.transform.position - transform.position), out hit, Mathf.Infinity)) 
        { 
            if (hit.transform == Player.transform) 
            { 
                isNotHidden = true;
            }
        }

        float angle = Vector3.Angle(transform.forward, Player.transform.position - transform.position);
        if (angle < DetectAngle)
        {
            isInAngle = true;
        }

        if (isInAngle && isInRange && isNotHidden)
        {
            isChasing = true;
        } else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            agent.SetDestination(Player.transform.position);

            // stop chasing if player is VERY far
            if (Vector3.Distance(transform.position, Player.transform.position) > DetectRange * 1.5f)
            {
                isChasing = false;
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 3.0f)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPointIndex].position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);

        Vector3 origin = transform.position;
        int rayCount = 30;

        float halfAngle = DetectAngle;
        float angleStep = (halfAngle * 2) / rayCount;

        Vector3 previousPoint = origin;

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = -halfAngle + angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Vector3 currentPoint = origin + direction * DetectRange;

            Gizmos.DrawLine(origin, currentPoint);

            if (i > 0)
                Gizmos.DrawLine(previousPoint, currentPoint);

            previousPoint = currentPoint;
        }
    }
}
