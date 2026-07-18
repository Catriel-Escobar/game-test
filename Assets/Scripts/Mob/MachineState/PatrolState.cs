using UnityEngine;

public class PatrolState : IMobState
{
    private readonly MobAI _ai;

    private Vector3 _currentPatrolPoint;

    public PatrolState(MobAI ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        _ai.TargetSpeed = 0.5f;
        PickNewPatrolPoint();
    }

    public void Tick()
    {
        if (_ai.Target != null)
        {
            _ai.ChangeState(
                new ChaseState(_ai));

            return;
        }

        if (_ai.Movement.HasReachedDestination())
        {
            PickNewPatrolPoint();
        }
    }

    public void Exit()
    {
    }

    private void PickNewPatrolPoint()
{
    Vector2 random =
        Random.insideUnitCircle * _ai.PatrolRadius;

    Vector3 patrolPoint =
        _ai.SpawnPosition +
        new Vector3(random.x, 0, random.y);

    _ai.Movement.MoveTo(patrolPoint);
}
}