using UnityEngine;

public class MobAI
{
    public Transform Target { get; set; }

    public Vector3 SpawnPosition { get; }

    public Vector3 Position =>
        _owner.transform.position;

    public MobMovement Movement => _movement;

    public Mob Owner => _owner;

    public MobAnimationController Animation =>
        _owner != null
            ? _owner.GetComponent<MobAnimationController>()
            : null;

    private readonly Mob _owner;
    private readonly MobMovement _movement;

    public IMobState _currentState;
    public float PatrolRadius { get; }

    public float AggroRange { get; set; }

    public float LoseTargetRange { get; set; }

    public float AttackRange { get; set; }

    public float TargetSpeed { get; set; }
   public MobAI(
        Mob owner,
        MobMovement movement,
        MobSpawnData spawnData)
    {
        _owner = owner;
        _movement = movement;

        SpawnPosition = spawnData.SpawnPosition;
        PatrolRadius = spawnData.PatrolRadius;

        ChangeState(new PatrolState(this));
    }

    public void Tick()
    {
        if (_owner.IsStunned)
        {
            _movement.Stop();
            TargetSpeed = 0f;
            return;
        }

        DetectAggro();
        _currentState?.Tick();
    }

    public void ChangeState(IMobState newState)
    {
        _currentState?.Exit();

        _currentState = newState;

        _currentState.Enter();
    }

    private void DetectAggro()
    {
        if (Target != null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            Position, AggroRange);

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].TryGetComponent<Player>(out var player))
                continue;

            if (!player.IsAlive)
                continue;

            float dist = Vector3.Distance(
                Position, player.Transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player.Transform;
            }
        }

        Target = closest;
    }
}