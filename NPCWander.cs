using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float roamRadius = 20f;        
    public float stepMinDistance = 3.0f; 
    public float idleMin = 0.6f, idleMax = 1.5f;

    private NavMeshAgent agent;
    private Vector3 roamCenter;
    private float nextTime;
    private float stuckTimer;

    private OpenAIConnector connector;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.autoBraking = false;
        roamCenter = transform.position;


        connector = FindObjectOfType<OpenAIConnector>();
    }

    void OnEnable()
    {
        TrySnapToNavMesh();
        PickNewDestination();
    }

    void Update()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;


        if (connector != null && connector.IsDialogueActive)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return; 
        }
        else if (agent.isStopped)
        {
            agent.isStopped = false;
        }


        if (agent.velocity.sqrMagnitude < 0.01f)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        bool stuck = stuckTimer > 2.0f || agent.pathStatus == NavMeshPathStatus.PathInvalid;

        if ((arrived && Time.time >= nextTime) || stuck)
        {
            if (arrived)
                roamCenter = transform.position;

            PickNewDestination();
        }
    }

    void PickNewDestination()
    {
        if (RandomRingPoint(roamCenter, stepMinDistance, roamRadius, out var dest))
            agent.SetDestination(dest);

        nextTime = Time.time + Random.Range(idleMin, idleMax);
        stuckTimer = 0f;
    }

    static bool RandomRingPoint(Vector3 center, float min, float max, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            float r = Random.Range(min, max);
            var dir = Random.insideUnitSphere.normalized;
            var pos = center + dir * r;
            pos.y = center.y;

            if (NavMesh.SamplePosition(pos, out var hit, 3f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

    void TrySnapToNavMesh()
    {
        if (agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(transform.position, out var hit, 3f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            roamCenter = hit.position;
        }
    }
}
