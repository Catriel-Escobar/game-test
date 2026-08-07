using UnityEngine;

public class AttackState : IMobState
{
    private readonly MobAI _ai;

    private float _attackTimer;

    private const float AttackCooldown = 1f;

    public AttackState(MobAI ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        _attackTimer = 0;
        _ai.TargetSpeed = 0f;
        _ai.Movement.Stop();
        FaceTarget();
    }

    public void Tick()
    {
        if (_ai.Target == null)
        {
            _ai.ChangeState(
                new ReturnToSpawnState(_ai));

            return;
        }

        float distanceToTarget =
            Vector3.Distance(
                _ai.Position,
                _ai.Target.position);

        float distanceToSpawn =
            Vector3.Distance(
                _ai.Position,
                _ai.SpawnPosition);

        if (distanceToTarget > _ai.AttackRange)
        {
            _ai.ChangeState(
                new ChaseState(_ai));

            return;
        }

        if (distanceToSpawn > _ai.LoseTargetRange)
        {
            _ai.Target = null;

            _ai.ChangeState(
                new ReturnToSpawnState(_ai));

            return;
        }

        FaceTarget();

        if (_ai.Owner.Combat.IsAttacking)
            return;

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= AttackCooldown)
        {
            _attackTimer = 0;

            _ai.Owner.Combat.TryBeginAttack(new Attack
            {
                damageMultiplier = 1f,
                damageType = AttackDamageType.Physical
            });
        }
    }

    public void Exit()
    {
        _ai.Owner.Combat.SetAttackActive(false);
    }

    private void FaceTarget()
    {
        if (_ai.Target == null) return;

        Vector3 direction =
            _ai.Target.position - _ai.Position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        _ai.Owner.transform.rotation = Quaternion.RotateTowards(
            _ai.Owner.transform.rotation,
            targetRotation,
            720f * Time.deltaTime);
    }
}
