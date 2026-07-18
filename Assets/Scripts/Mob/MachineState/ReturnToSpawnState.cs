public class ReturnToSpawnState : IMobState
{
    private readonly MobAI _ai;

    public ReturnToSpawnState(MobAI ai)
    {
        _ai = ai;
    }

    public void Enter()
    {
        _ai.TargetSpeed = 1f;
        _ai.Movement.MoveTo(
            _ai.SpawnPosition);
    }

    public void Tick()
    {
        if (_ai.Movement.HasReachedDestination())
        {
            _ai.ChangeState(
                new PatrolState(_ai));
        }
    }

    public void Exit()
    {
    }
}