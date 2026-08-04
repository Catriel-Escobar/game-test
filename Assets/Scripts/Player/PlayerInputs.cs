using System;

using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(PlayerMovement))]

[RequireComponent(typeof(PlayerCombat))]
public class PlayerInputs : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private InputSystem_Actions _input;
    private PlayerMovement _movement;
    private Player _player;
    private PlayerCombat playerCombat;
    private Vector2 _moveInput;
    private bool _isWalking;
    private bool _isBasicAttack;
    public Vector2 Move => _moveInput;
       private void Awake()
    {
        _input = new InputSystem_Actions();
        playerCombat = GetComponent<PlayerCombat>();
    }

    public void Initialize(Player player)
    {
        _player = player;
        _movement = player.Movement;
    }
   private void OnEnable()
    {
        _input.Enable();

        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;
        _input.Player.Test1.performed +=Test;
        _input.Player.Walk.performed += OnWalk;
        _input.Player.Walk.canceled += OnWalk;
        _input.Player.BasicAttack.performed += playerCombat.OnBasicAttack;
        _input.Player.BasicAttack.canceled += playerCombat.OnBasicAttack;
        _input.Player.Skill1.performed += OnSkill1;
        _input.Player.Skill2.performed += OnSkill2;
        _input.Player.Skill3.performed += OnSkill3;
    }

  

    private void Test(InputAction.CallbackContext context)
    {
          if (!context.performed)
        return;

       
            switch (context.control.name)
    {
        case "1":
                DamageData dmgdata = new DamageData
                {
                    BaseDamage = 1,
                    FinalDamage = 2,
                    IsCritical = true,
                    Source = _player
                };
                _player.TakeDamage(dmgdata);
            break;

        case "2":
            _player.Progression.AddExperience(50);
            break;

        case "3":
            _player.Skills?.DebugPrintSkills();
            break;

        default:
            Debug.Log("Otro control: " + context.control.name);
            break;
    }
    }

    private void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        _input.Player.Walk.performed -= OnWalk;
        _input.Player.Walk.canceled -= OnWalk;
        _input.Player.BasicAttack.performed -= playerCombat.OnBasicAttack;
        _input.Player.BasicAttack.canceled -= playerCombat.OnBasicAttack;
        _input.Player.Skill1.performed -= OnSkill1;
        _input.Player.Skill2.performed -= OnSkill2;
        _input.Player.Skill3.performed -= OnSkill3;
        _input.Disable();
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        _isWalking = context.ReadValueAsButton();
    }

    private void OnSkill1(InputAction.CallbackContext context)
    {
        TryCastEquippedSkill(0);
    }

    private void OnSkill2(InputAction.CallbackContext context)
    {
        TryCastEquippedSkill(1);
    }

    private void OnSkill3(InputAction.CallbackContext context)
    {
        TryCastEquippedSkill(2);
    }

    private void TryCastEquippedSkill(int slotIndex)
    {
        if (_player?.Skills == null || _player.Caster == null) return;

        string[] equippedIds = _player.Skills.GetEquippedSkillIds();
        if (slotIndex >= equippedIds.Length) return;

        _player.Caster.TryCastSkill(equippedIds[slotIndex]);
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
        
    }

    private void Update()
    {
      _movement.Move(_moveInput,_isWalking);
    }
}
