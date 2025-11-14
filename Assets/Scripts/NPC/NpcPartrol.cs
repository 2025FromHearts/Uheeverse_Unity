using UnityEngine;
using UnityEngine.AI;

public class NpcPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentIndex = 0;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NPC 위치 NavMesh 보정
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            transform.position = hit.position;

        GoNext();
    }

    private void Update()
    {
        // NavMesh 위에 없다면 패스 (오류 방지)
        if (!agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance < 0.3f)
            GoNext();
    }

    private void GoNext()
    {
        if (waypoints.Length == 0) return;

        // waypoint 위치도 NavMesh 위로 보정
        NavMeshHit hit;
        if (NavMesh.SamplePosition(waypoints[currentIndex].position, out hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        currentIndex = (currentIndex + 1) % waypoints.Length;
    }
}
