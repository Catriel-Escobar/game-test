public interface ICombatEntity : IEntity, IDamageable
{
    CombatStats CombatStats { get; }
}