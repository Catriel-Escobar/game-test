public abstract class StatusEffect
{
    public float Remaining { get; private set; }

    protected StatusEffect(float duration)
    {
        Remaining = duration;
    }

    public bool Tick(float deltaTime)
    {
        Remaining -= deltaTime;
        return Remaining > 0f;
    }

    public void Apply()
    {
        OnApply();
    }

    public void Expire()
    {
        OnExpire();
    }

    protected abstract void OnApply();
    protected abstract void OnExpire();
}
