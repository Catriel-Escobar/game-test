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
        _input.Disable();
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        _isWalking = context.ReadValueAsButton();
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
