using UnityEngine;

public class ChaseState : IMobState
{
    private readonly MobAI _ai;

    public ChaseState(MobAI ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        _ai.TargetSpeed = 1f;
    }

    public void Tick()
    {
        if (_ai.Target == null)
        {
            _ai.ChangeState(
                new ReturnToSpawnState(_ai));

            return;
        }

        if (_ai.Target.TryGetComponent<Player>(out var player)
            && !player.IsAlive)
        {
            _ai.Target = null;

            _ai.ChangeState(
                new ReturnToSpawnState(_ai));

            return;
        }

        float distanceToSpawn =
            Vector3.Distance(
                _ai.Position,
                _ai.SpawnPosition);

        if (distanceToSpawn > _ai.LoseTargetRange)
        {
            _ai.Target = null;

            _ai.ChangeState(
                new ReturnToSpawnState(_ai));

            return;
        }

        _ai.Movement.MoveTo(
            _ai.Target.position);

        float distanceToTarget =
            Vector3.Distance(
                _ai.Position,
                _ai.Target.position);

        if (distanceToTarget <= _ai.AttackRange)
        {
            _ai.ChangeState(
                new AttackState(_ai));

            return;
        }
    }

    public void Exit()
    {
    }
}