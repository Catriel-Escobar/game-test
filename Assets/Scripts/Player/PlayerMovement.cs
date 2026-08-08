using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController _ch;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _rotationSpeed = 720f;
    [SerializeField] private float _acceleration = 5f;
    [SerializeField] private float _deceleration = 8f;
    private bool _IsAttacking = false;
    public float Speed => _normalizedSpeed;

    public float MaxSpeed => _speed;

    public float VerticalVelocity => _verticalVelocity;

    public bool IsGrounded => _ch.isGrounded;

    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;
    private Camera _camera;
    private Player _player;
    private float _verticalVelocity;
    private float _normalizedSpeed;

    private bool _isDashing;
    private Vector3 _dashDirection;
    private float _dashSpeed;

    public bool IsDashing => _isDashing;

        private void Awake()
        {
            _ch = GetComponent<CharacterController>();
            _camera = Camera.main;
            _player = GetComponent<Player>();
        }
    public void Initialize(PlayerMovementConfig config)
    {
        Debug.Log($"Desde movement {config.speed}");
        _speed = config.speed;
        _gravity = config.gravity;
        _rotationSpeed = config.rotationSpeed;
    }
    public void Move(Vector2 input, bool isWalking = false)
        {
            if(_IsAttacking) return;
            if(_isDashing) return;
            Vector3 forward = _camera.transform.forward;
            Vector3 right = _camera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            Vector3 move =
                forward * input.y +
                right * input.x;

            move = Vector3.ClampMagnitude(move, 1f);

            // Rotación
            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // Gravedad
            if (_ch.isGrounded)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
            float targetSpeed;

            if (!isWalking)
            {
                targetSpeed = move.magnitude > 0 ? 2f : 1f;
            }
            else
            {
                targetSpeed = move.magnitude > 0 ? 1.5f : 1f;
            }

            float rate =
                targetSpeed > _normalizedSpeed
                    ? _acceleration
                    : _deceleration;

            _normalizedSpeed = Mathf.MoveTowards(
                _normalizedSpeed,
                targetSpeed,
                rate * Time.deltaTime);
            
            _velocity = move * (isWalking ? _speed /2 : _speed) * GetSpeedMultiplier();
            _velocity.y = _verticalVelocity;

        _ch.Move(Velocity * Time.deltaTime);
    }

    private float GetSpeedMultiplier()
    {
        return _player != null ? _player.MovementSpeedMultiplier : 1f;
    }


    public void FaceWorldPosition(Vector3 worldPosition)
    {
        Vector3 direction = worldPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void BeginDash(Vector3 direction, float speed)
    {
        _isDashing = true;
        _dashDirection = direction;
        _dashDirection.y = 0f;
        _dashDirection.Normalize();
        _dashSpeed = speed;
        _normalizedSpeed = 1f;
    }

    public void DashStep()
    {
        if (_ch.isGrounded)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }

        _velocity = _dashDirection * _dashSpeed;
        _velocity.y = _verticalVelocity;

        _ch.Move(_velocity * Time.deltaTime);
    }

    public void EndDash()
    {
        _isDashing = false;
        _velocity = Vector3.zero;
    }
    internal void AttackStateChanged(bool obj)
    {
       _IsAttacking = obj;
       _normalizedSpeed = 1f;
    }
}