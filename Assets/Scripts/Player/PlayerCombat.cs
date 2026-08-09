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
    private Player _player;
    private AttackConfig attackConfig;
    private StatsConfig statsConfig;
    private float baseAttackSpeed;

    public bool IsAttacking { get; private set; }
    public bool IsSwinging { get; private set; }
    public event Action<bool> OnAttackStateChanged;
    public event Action<bool> OnSwingStateChanged;

    public Attack CurrentAttack;

    private string _attackVfxPath;
    private GameObject _attackVfxPrefab;
    private GameObject _attackVfxInstance;

    private void Start()
    {
        playerInputs = GetComponent<PlayerInputs>();

    }
    internal void   Initilizate(Player player)
    {
        _player = player;
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

        if (TryInteractWithDrop()) return;

        Attack candidateAttack = FindAttackById("basic_attack");
        if (candidateAttack == null) return;

        if (IsSwinging) return;

        playerMovement.CancelMoveTo();

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

        if (active)
            SpawnAttackVfx();
        else
            DespawnAttackVfx();

        if (!active)
            CurrentAttack = null;

        OnAttackStateChanged?.Invoke(IsAttacking);
    }

    private void SpawnAttackVfx()
    {
        if (CurrentAttack == null || string.IsNullOrEmpty(CurrentAttack.vfx)) return;

        if (_attackVfxPrefab == null || _attackVfxPath != CurrentAttack.vfx)
        {
            _attackVfxPath = CurrentAttack.vfx;
            _attackVfxPrefab = Resources.Load<GameObject>(_attackVfxPath);
        }

        if (_attackVfxPrefab == null)
        {
            Debug.LogWarning($"[Combat] VFX '{CurrentAttack.vfx}' no encontrado en Resources.");
            return;
        }

        _attackVfxInstance = Instantiate(_attackVfxPrefab, transform);
        _attackVfxInstance.transform.localPosition =
            new Vector3(0f, 1.2f, CurrentAttack.range * 0.5f);
    }

    private void DespawnAttackVfx()
    {
        if (_attackVfxInstance == null) return;

        Destroy(_attackVfxInstance);
        _attackVfxInstance = null;
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

    private bool TryInteractWithDrop()
    {
        if (playerMovement == null) return false;

        Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        WorldDrop drop = DropRegistry.FindDropAtScreenPoint(mousePosition);
        if (drop == null) return false;

        float sqrDistance = (drop.transform.position - transform.position).sqrMagnitude;
        if (sqrDistance <= drop.PickupRadius * drop.PickupRadius)
        {
            drop.TryPickup(_player);
        }
        else
        {
            playerMovement.MoveTo(drop.transform.position, drop.PickupRadius, () => drop.TryPickup(_player));
        }

        return true;
    }
}
