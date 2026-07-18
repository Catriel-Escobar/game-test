using System;
using System.Collections;
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
    public event Action<bool> OnAttackStateChanged;

    private Coroutine _attackWindowRoutine;
    private float _attackEndsAt;

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
        CurrentAttack = candidateAttack;
        Vector3 mouseWorld = GetMouseWorldPoint();
        if (!IsAttacking) playerMovement.FaceWorldPosition(mouseWorld);
        

        float effectiveAttackSpeed =
            baseAttackSpeed +
            (playerStats.Dexterity *
             statsConfig.Dexterity.attackSpeedPerPoint);

        float actualDuration =
            candidateAttack.duration / effectiveAttackSpeed;

        if (!TryBeginAttackWindow(actualDuration)) return;

        playerAnimation.AttacksMeeles(
            candidateAttack, effectiveAttackSpeed);
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


        // Helper principal: recibe duration y gestiona el estado "atacando".
    private bool TryBeginAttackWindow(float duration)
    {
        duration = Mathf.Max(0.01f, duration);

        if (IsAttacking && Time.time < _attackEndsAt)
            return false;

        if (_attackWindowRoutine != null)
            StopCoroutine(_attackWindowRoutine);

        _attackWindowRoutine = StartCoroutine(AttackWindowRoutine(duration));
        return true;
    }

    private IEnumerator AttackWindowRoutine(float duration)
    {
        SetAttacking(true);
        _attackEndsAt = Time.time + duration;

        yield return new WaitForSeconds(duration);

        SetAttacking(false);
        CurrentAttack = null;
        _attackWindowRoutine = null;
    }

    private void SetAttacking(bool value)
    {
        if (IsAttacking == value) return;
        IsAttacking = value;
        OnAttackStateChanged?.Invoke(IsAttacking);
    }

    private void OnDisable()
    {
        if (_attackWindowRoutine != null)
            StopCoroutine(_attackWindowRoutine);

        _attackWindowRoutine = null;
        SetAttacking(false);
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