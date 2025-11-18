using UnityEngine;
using UnityEngine.AI;

public class CrowdPatrol : MonoBehaviour
{
    public Transform[] waypoints;

    private NavMeshAgent agent;
    private int currentIndex;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.radius = Random.Range(0.35f, 0.55f);
        agent.height = 2f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(10, 80);

        agent.speed = Random.Range(1.5f, 3.2f);
        agent.angularSpeed = Random.Range(90f, 150f);
        agent.acceleration = Random.Range(5f, 10f);

        currentIndex = Random.Range(0, waypoints.Length);

        float delay = Random.Range(0f, 1.5f);
        Invoke(nameof(GoNext), delay);
    }

    private void GoNext()
    {
        if (waypoints.Length == 0) return;

        int next;
        do
        {
            next = Random.Range(0, waypoints.Length);
        }
        while (next == currentIndex);

        currentIndex = next;
        agent.SetDestination(waypoints[currentIndex].position);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.4f)
        {
            GoNext();
        }
    }
}
