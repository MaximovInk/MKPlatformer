using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using static UnityEngine.Rendering.DebugUI.Table;
using Action = System.Action;

namespace MaximovInk.MKPlatformer
{
    public enum MKSide
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public enum MovementDirection
    {
        NonInitialized,
        Left,
        Right
    }

    public enum UpdateMode
    {
        FixedUpdate,
        Update
    }

    /*
     Gravity
     HorizontalMovement Walk,Run,Sprint
     Dash, Dash to target, Damage dash
     Jump, DoubleJump, LongJump (Coyote time)
     Bounce (hit ground => jump)
     Crouch (in air, in ground)
     Dangling (look at hole)
     Dive (through oneWayPlatforms or other)
     FallDamage
     Fly, Jetpack
     Glide (slow air falling)
     WallCling, WallClimb, Grip (attach to wall)
     GroundNormalGravity (change gravity to normal)
     HandleObject/Throw
     Handle Weapon/Throw
     WallClimb, Ladder
     LedgeHang (climb on edges of wall)
     LookUp
     Push
     Roll
     RotateAroundNormal
     Stairs
     Stun
     Swimming
     TimeControl
     WallJump
     */

    [RequireComponent(typeof(Rigidbody2D))]
    public class MKCharacterController : MonoBehaviour
    {
        public event Action<int> OnMoveDirectionChanged;

        public bool IsGrounded => IsCollisionBelow;
        public bool IsGroundedAndSlopeOk => IsCollisionBelow && IsSlopeOk;

        public bool IsSlopeOk => _collisionState.SlopeAngleIsOk;
        public bool IsCollisionLeft => _collisionState.Left.IsColliding;
        public bool IsCollisionRight => _collisionState.Right.IsColliding;
        public bool IsCollisionAbove => _collisionState.Above.IsColliding;
        public bool IsCollisionBelow => _collisionState.Below.IsColliding;
        public bool IsJumping => _isJumping;
        public bool IsFalling => _isFalling;

        public float TimeGroundAirborne => _timeGroundAirborne;
        public bool LockDirectionChange => _lockDirectionChange;
        public MovementDirection Direction => _movementDirection;
        public float Rotation => _rotation;
        public Quaternion TRotation => _tRotation;
        public Quaternion InverseTRotation => _tInverseRotation;

        public Transform Graphics => _graphics;
        public MKControllerRaycast Raycast => _raycast;
        public MKControllerCollision Collision => _collision;
        public MKCharacterParameters Parameters => _parameters;
        public MKCharacterCollisionState CollisionState => _collisionState;

        public MKStateMachine<MKControllerState, float> StateMachine => _stateMachine;

        public MKCGravity Gravity => _gravity;
        public UpdateMode UpdateMode => _updateMode;
        public float DeltaTime => _deltaTime;

        [SerializeField] private MKCharacterParameters _parameters;
        [SerializeField] private UpdateMode _updateMode = UpdateMode.FixedUpdate;

        private bool _lockDirectionChange;
        private MovementDirection _movementDirection 
            = MovementDirection.NonInitialized;

        private float _deltaTime;
        private float _timeGroundAirborne;
        private float _rotation;
        private bool _lastIsGrounded;
        private bool _isJumping;
        private bool _isFalling;

        [SerializeField] private Transform _graphics;
        private MKCharacterCollisionState _collisionState;
        private MKControllerRaycast _raycast;
        private MKControllerCollision _collision;
        private MKCGravity _gravity;
        private Rigidbody2D _rigidbody;
        private Transform _transform;

        private Quaternion _tRotation;
        private Quaternion _tInverseRotation;

        private MKStateMachine<MKControllerState, float> _stateMachine;

        private MKControllerComponent[] _abilities;
        private readonly Dictionary<Type, List<MKControllerComponent>> _cachedAbilities 
            = new();

        private Stack<Action<MKCharacterController>> _preProcessActions = new();
        private Stack<Action<MKCharacterController>> _postProcessActions = new();

        public void PushPostProcessAction(Action<MKCharacterController> action)
        {
            _postProcessActions.Push(action);
        }

        public void PushPreProcessAction(Action<MKCharacterController> action)
        {
            _preProcessActions.Push(action);
        }

        public Vector2 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        private Vector2 _velocity;
        private Vector2 _externalVelocity;

        protected virtual void Awake()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            _transform = transform;
            _rigidbody = GetComponent<Rigidbody2D>();
            _collision = GetComponentInChildren<MKControllerCollision>();
            _raycast = GetComponentInChildren<MKControllerRaycast>();
            _gravity = GetComponentInChildren<MKCGravity>();
            _stateMachine = new MKStateMachine<MKControllerState, float> (MKControllerStates.GetPriority);

            if (_graphics == null)
            {
                Debug.LogWarning("Graphics of character not set! Using root transform.");
                _graphics = transform;
            }

            _collisionState = new MKCharacterCollisionState();

            _collision.Initialize();
            _raycast.Initialize(this);

            RefreshAbilities();
        }

        public void RefreshAbilities()
        {
            _abilities = GetComponentsInChildren<MKControllerComponent>();

            _cachedAbilities.Clear();

            foreach (var ability in _abilities)
            {
                var t = ability.GetType();

                if (_cachedAbilities.TryGetValue(t, out var cachedAbility))
                {
                    cachedAbility.Add(ability);
                }
                else
                {
                    _cachedAbilities.Add(t, new List<MKControllerComponent> { ability });
                }
            }

            foreach (var ability in _abilities)
            {
                ability.Refresh(this);
            }
        }

        public List<T> FindAbilities<T>() where T : MKControllerComponent
        {
            var searchedAbilityType = typeof(T);

            if (_cachedAbilities.TryGetValue(searchedAbilityType, out var value))
            {
                return value as List<T>;
            }

            return null;
        }

        public T FindAbility<T>() where T : MKControllerComponent
        {
            var searchedAbilityType = typeof(T);

            if (_cachedAbilities.TryGetValue(searchedAbilityType, out var value))
            {
                if (value.Count > 0)
                    return value[0] as T;
            }

            return null;
        }

        protected virtual void Update()
        {
            if (_updateMode == UpdateMode.Update)
            {
                UpdateController();
            }
        }

        private void FixedUpdate()
        {
            if (_updateMode == UpdateMode.FixedUpdate)
            {
                UpdateController();
            }
        }

        protected virtual void LateUpdate()
        {
            
        }

        protected void UpdateLockDirection()
        {
            _lockDirectionChange = false;

            foreach (var ability in _abilities)
            {
                if (!ability.IsActive) continue;

                if (ability.LockDirectionChange)
                {
                    _lockDirectionChange = true;
                    break;
                }
            }
        }

        private void UpdateController()
        {
            _tRotation = _transform.rotation;
            _tInverseRotation = Quaternion.Inverse(_tRotation);
            _rotation = _tRotation.eulerAngles.z;

            _deltaTime = _updateMode == UpdateMode.FixedUpdate
                ? Time.fixedDeltaTime
                : Time.deltaTime;

            PopVelocityFromRb();
            UpdateLockDirection();

            CheckCollisionSides();
            SlopesLogic();

            foreach (var action in _preProcessActions)
            {
                action?.Invoke(this);
            }

            PreProcessAbilities();
            ProcessAbilities();
            PostProcessAbilities();

            foreach (var action in _postProcessActions)
            {
                action?.Invoke(this);
            }

            ClampVelocity();

            PushVelocityToRb();
#if UNITY_EDITOR
            Debug.DrawRay(_transform.position, _rigidbody.linearVelocity, Color.red);
#endif

            if (!IsGrounded)
            {
                _timeGroundAirborne += Time.deltaTime;
            }

            if (_lastIsGrounded != IsGrounded)
            {
                if(!IsGrounded)
                    _timeGroundAirborne = 0f;

                _lastIsGrounded = IsGrounded;
            }

            _isJumping = !IsGrounded && _velocity.y > 0;
            _isFalling = !IsGrounded && _velocity.y < 0;

            _preProcessActions.Clear();
            _postProcessActions.Clear();
        }

        public bool CheckWallValid(MKHitInfo info, float tolerance)
        {
            return info is { IsColliding: true, IsValidAngle: true } && info.CheckTolerance(tolerance);
        }

        public bool SetDirection(MovementDirection direction)
        {
            if (_lockDirectionChange) return false;

            _movementDirection = direction;

            OnMoveDirectionChanged?.Invoke(GetDirection());

            return true;
        }

        public bool SwitchDirection()
        {
            if (_lockDirectionChange) return false;

            _movementDirection = _movementDirection == MovementDirection.Right
                ? MovementDirection.Left
                : MovementDirection.Right;

            OnMoveDirectionChanged?.Invoke(GetDirection());

            return true;
        }

        public int GetDirection()
        {
            return GetDirection(_movementDirection);
        }

        public int GetDirection(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.NonInitialized:
                    return 1;
                    break;
                case MovementDirection.Left:
                    return -1;
                    break;
                case MovementDirection.Right:
                    return 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector2 _lastExternalVelocity;

        private void PopVelocityFromRb()
        {
            _velocity = (Vector2)(_tInverseRotation * _rigidbody.linearVelocity) - _lastExternalVelocity;
        }

        private Vector2 PushVelocityToRb()
        {
            _lastExternalVelocity = _externalVelocity;

            var finalVel = _velocity + _externalVelocity;

            _rigidbody.linearVelocity = _tRotation * finalVel;

            return finalVel;
        }

        public void HandleInput(ICharacterInput input)
        {
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive) continue;
                if (!ability.ReadInput) continue;

                ability.HandleInput(input);
            }
        }

        public virtual void PreProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive) continue;

                ability.PreProcess(_deltaTime);
            }
        }

        public virtual void ProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive) continue;

                ability.Process(_deltaTime);
            }
        }

        public virtual void PostProcessAbilities()
        {
            foreach (var ability in _abilities)
            {
                if (!ability.IsActive) continue;

                ability.PostProcess(_deltaTime);
            }
        }

        protected virtual void ClampVelocity()
        {
            _velocity.x = Mathf.Clamp(_velocity.x, -_parameters.MaxVelocity.x, _parameters.MaxVelocity.x);
            _velocity.y = Mathf.Clamp(_velocity.y, -_parameters.MaxVelocity.y, _parameters.MaxVelocity.y);
        }

        public void SetVelocity(Vector2 velocity)
        {
             _velocity = velocity;
        }

        public void SetVelocityX(float x)
        {
            _velocity.x = x ;
        }

        public void SetVelocityY(float y)
        {
            _velocity.y = y ;
        }

        public void AddVelocityX(float x)
        {
            _velocity.x += x; 
        }

        public void AddVelocityY(float y)
        {
            _velocity.y += y;
        }

        public void AddVelocity(Vector2 velocity)
        {
            _velocity += velocity;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            _externalVelocity = velocity;
        }

        public void SetExternalVelocityX(float x)
        {
            _externalVelocity.x = x;
        }

        public void SetExternalVelocityY(float y)
        {
            _externalVelocity.y = y;
        }

        public void AddExternalVelocityX(float x)
        {
            _externalVelocity.x += x;
        }

        public void AddExternalVelocityY(float y)
        {
            _externalVelocity.y += y;
        }

        public void AddExternalVelocity(Vector2 velocity)
        {
            _externalVelocity += velocity;
        }

        public void SetPosition(Vector2 position)
        {
            _rigidbody.MovePosition(position);
        }

        public void AddPosition(Vector2 delta)
        {
            _rigidbody.MovePosition(_rigidbody.position + delta);
        }

        public void CheckCollisionSides()
        {
            _collisionState.Reset();

            var minWallAngle =  _parameters.MinWallAngle;
            var maxWallAngle =  _parameters.MaxWallAngle;
            var slopeAngle =  _parameters.MaxSlopeAngle;

            _collisionState.Below = _raycast.CheckRays(MKSide.Bottom, -slopeAngle, slopeAngle);
            _collisionState.Above = _raycast.CheckRays(MKSide.Top);

            if (IsCollisionAbove)
                _raycast.AddIgnore(_collisionState.Above.Collider);
            if (IsCollisionBelow)
                _raycast.AddIgnore(_collisionState.Below.Collider);

            _collisionState.Right = _raycast.CheckRays(MKSide.Right, minWallAngle, maxWallAngle);
            _collisionState.Left = _raycast.CheckRays(MKSide.Left, minWallAngle, maxWallAngle);

            if (IsCollisionAbove)
                _raycast.RemoveIgnore(_collisionState.Above.Collider);
            if (IsCollisionBelow)
                _raycast.RemoveIgnore(_collisionState.Below.Collider);
        }

        private void SlopesLogic()
        {
            if (!IsCollisionBelow)
            {
                _collisionState.SlopeAngleIsOk = false;
                _collisionState.SlopeAngle = 0f;
                return;
            }

            var below = _collisionState.Below;

            _collisionState.SlopeAngle = below.Angle;
            _collisionState.SlopeAngleIsOk = below.IsValidAngle;

            var slopePerpendicular = Vector2.Perpendicular(below.Normal);
            var slopeContactPoint = below.Point;

            if (_collisionState.SlopeAngleIsOk)
            {
                Debug.DrawRay(slopeContactPoint, transform.up * 1, Color.green);
                Debug.DrawRay(slopeContactPoint, slopePerpendicular * 1, Color.green);
            }
            else
            {
                Debug.DrawRay(slopeContactPoint, transform.up * 1, Color.red);
                Debug.DrawRay(slopeContactPoint, slopePerpendicular * 1, Color.red);
            }

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (_collision == null || _rigidbody == null) return "Not initialized";

            sb.Append("</mspace>");
            sb.Append($"Entity: {gameObject.name}\n");
            sb.Append($"IsFacingRight: {_movementDirection} ({GetDirection()})\n");
            sb.Append($"IsJumping: {IsJumping}\n");
            sb.Append($"IsFalling: {IsFalling}\n");
            sb.Append($"Time airborne: {_timeGroundAirborne}\n");
            sb.Append($"==Physics==\n");
            sb.Append($"ColliderSize: {(_collision.Size)}\n");
            sb.Append($"Velocity: {(_velocity)}({_velocity.magnitude})\n");
            sb.Append($"External Velocity: {(_externalVelocity)}({_externalVelocity.magnitude})\n");
            sb.Append($"World state:{_collisionState}\n");
            var slopeOk = _collisionState.SlopeAngleIsOk ? "Ok" : "None";
            sb.Append($"Rotation: {_rotation}\n");
            sb.Append($"Slope: {slopeOk}; angle: {_collisionState.SlopeAngle}\n");
            sb.Append($"==State==\n");
            sb.Append($"Current: {_stateMachine.CurrentState}\n");
            sb.Append($"Preview:{_stateMachine.PreviousState})\n");

            var states = _stateMachine.GetCurrentStates();

            for (int i = 0; i < states.Count; i++)
            {
                sb.Append($"{i+1}) {states[i].ID} {states[i].Priority}\n");
            }


            /*
          sb.Append($"State: {_state}\n");

            sb.Append($"IsSprint: {isSprint}\n");
            sb.Append($"IsCrouch: {_isCrouch}\n");
            sb.Append($"Input Move: {input}\n");

            sb.Append("IgnoredCollision: ");

            if (IgnoredColliders.Count == 0) sb.Append("0\n");
            else sb.Append("\n");

            for (int i = 0; i < IgnoredColliders.Count; i++)
            {
                var index = i;
                sb.Append($"[{index + 1}]\t{IgnoredColliders[index].name}\n");
            }

            sb.Append("OneWayPlatforms: ");

            if (_oneWayPlatforms.Count == 0) sb.Append("0\n");
            else sb.Append("\n");

            for (int i = 0; i < _oneWayPlatforms.Count; i++)
            {
                var index = i;
                sb.Append($"[{index + 1}]\t{_oneWayPlatforms[index].name}\n");
            }*/

            return sb.ToString();
        }
    }
}