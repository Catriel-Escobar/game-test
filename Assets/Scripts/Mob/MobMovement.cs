using UnityEngine;
using UnityEngine.AI;

public class MobMovement
{
    private readonly NavMeshAgent _agent;

    public MobMovement(
        NavMeshAgent agent)
    {
        _agent = agent;
    }

    public void MoveTo(Vector3 position)
    {
        _agent.SetDestination(position);
    }

    public void Stop()
    {
        _agent.ResetPath();
    }

    public bool HasReachedDestination()
    {
        return !_agent.pathPending &&
               _agent.remainingDistance <= _agent.stoppingDistance;
    }
}