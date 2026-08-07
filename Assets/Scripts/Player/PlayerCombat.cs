using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat:MonoBehaviour
{
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerResources playerResources;
    [SerializeField] private PlayerMovement playerMovement;
    private AttackConfig attackConfig;
    private StatsConfig statsConfig;
    private float baseAttackSpeed;

    public bool IsAttacking { get; private set; }
    public bool IsSwinging { get; private set; }
    public event Action<bool> OnAttackStateChanged;
    public event Action<bool> OnSwingStateChanged;

    public Attack CurrentAttack;

    private void Start()
    {
        playerInputs = GetComponent<PlayerInputs>();

    }
    internal void   Initilizate(Player player)
    {
        playerAnimation = player.Animation;
        playerStats = player.Stats;
        playerResources = player.Resources;
        attackConfig = player.AttackConfig;
        statsConfig = player.StatsConfig;
        baseAttackSpeed = player.PlayerConfig.combat.attackSpeed;
        playerMovement = player.Movement;
    }

    internal void OnBasicAttack(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;

        Attack candidateAttack = FindAttackById("basic_attack");
        if (candidateAttack == null) return;

        if (IsSwinging) return;

        CurrentAttack = candidateAttack;

        Vector3 mouseWorld = GetMouseWorldPoint();
        playerMovement.FaceWorldPosition(mouseWorld);

        float effectiveAttackSpeed =
            baseAttackSpeed +
            (playerStats.Dexterity *
             statsConfig.Dexterity.attackSpeedPerPoint);

        playerAnimation.AttacksMeeles(
            candidateAttack, effectiveAttackSpeed);
    }

    public void BeginSwing()
    {
        if (IsSwinging) return;

        IsSwinging = true;
        OnSwingStateChanged?.Invoke(true);
    }

    public void EndSwing()
    {
        if (!IsSwinging) return;

        IsSwinging = false;
        OnSwingStateChanged?.Invoke(false);
    }

    public void SetAttackActive(bool active)
    {
        if (IsAttacking == active) return;

        IsAttacking = active;

        if (!active)
            CurrentAttack = null;

        OnAttackStateChanged?.Invoke(IsAttacking);
    }

    private Attack FindAttackById(string attackId)
    {
        if (attackConfig?.attacks == null) return null;

        for (int i = 0; i < attackConfig.attacks.Length; i++)
        {
            if (attackConfig.attacks[i].id == attackId)
                return attackConfig.attacks[i];
        }

        return null;
    }

    private void OnDisable()
    {
        EndSwing();
        SetAttackActive(false);
    }

    private Vector3 GetMouseWorldPoint()
    {
        Camera camera = Camera.main;
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return transform.position + transform.forward;
    }
}
