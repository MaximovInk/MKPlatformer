using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCHorizontalMovement : MKControllerComponent
    {
        protected const float MOVEMENT_THRESHOLD = 0.05f;
        protected const float SLOPE_STICK_NORMAL_Y_THRESHOLD = 0.05f;
        protected const float SLOPE_STICK_THRESHOLD = 5;

        public bool IsMoving =>
            _isMoving
            && Mathf.Abs(Controller.Velocity.x) > MOVEMENT_THRESHOLD
            && Controller.IsGrounded;
        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        public float AirAcceleration = 1f;
        public float AirDeceleration = 1f;

        public MKCharacterAxis Axis = new() { ID= "MoveX" };

        public bool CanMoveOnBadSlopes => _canMoveOnBadSlopes;

        public float CustomMultiplier = 1f;
        [SerializeField] protected float _speed = 10;
        [Range(0,1)]
        [SerializeField] protected float _airControl = 1f;
        [SerializeField] protected bool _canMoveOnBadSlopes;
        [SerializeField] protected bool _applyMoveDirection = true;
        [SerializeField] protected bool _slopesStick = false;
        [SerializeField] protected MKSHorizontalMovement _state;
        [SerializeField] private MKSurfaceParameters _defaultSurfaceParameters = new();

        protected float _moveInput;
        protected bool _lastGrounded = false;
        protected float _lastHorizontalForce;
        protected bool _isMoving;
        protected MKSurfaceParameters _currentSurface;

        public override void Refresh(MKCharacterController controller)
        {
            base.Refresh(controller);

            _state.Refresh(controller);
        }

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _moveInput = Axis.ReadFrom(input);
        }

        protected void ApplyMovementDirection()
        {
            switch (_moveInput)
            {
                case > MOVEMENT_THRESHOLD when Controller.Direction != MovementDirection.Right:
                    Controller.SetDirection(MovementDirection.Right);
                    break;
                case < -MOVEMENT_THRESHOLD when Controller.Direction != MovementDirection.Left:
                    Controller.SetDirection(MovementDirection.Left);
                    break;
            }
        }

        protected void SlopeStickMovement(Vector2 moveNormal, float xForce,float acceleration)
        {
            var moveForce = Controller.InverseTRotation * new Vector2(moveNormal.y, -moveNormal.x) * xForce;

            moveForce.x = Mathf.Lerp(Controller.Velocity.x, moveForce.x, acceleration);
            moveForce.y = Mathf.Lerp(Controller.Velocity.y, moveForce.y, acceleration);

            Controller.SetVelocityX(moveForce.x);
            Controller.SetVelocityY(moveForce.y);
        }

        protected void DefaultMovement(float xForce,float acceleration)
        {
            xForce = Mathf.Lerp(Controller.Velocity.x, xForce, acceleration);

            Controller.SetVelocityX(xForce);
        }

        protected MKSurfaceParameters GetCurrentSurfaceParameters()
        {
            if (_currentSurface != null)
            {
                return _currentSurface;
            }
            return _defaultSurfaceParameters;
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var grounded = Controller.IsGrounded;
            var xForce = _moveInput * _speed * CustomMultiplier;

            if (_lastGrounded != grounded)
            {
                if (_lastGrounded)
                {
                    // Preserve horizontal velocity when transitioning from ground to air
                    _lastHorizontalForce = xForce;
                }
                else
                {
                    // Preserve horizontal velocity when transitioning from air to ground
                    Controller.SetVelocityX(Controller.Velocity.x);
                    xForce = Controller.Velocity.x;
                }
            }

            if (!grounded)
                xForce = Mathf.Lerp(_lastHorizontalForce, xForce, _airControl);

            _isMoving = false;

            var moveNormal = Controller.CollisionState.Below.Normal;

            var surfaceParams = GetCurrentSurfaceParameters();
            var acceleration = grounded ? surfaceParams.Acceleration : AirAcceleration;

            if (Mathf.Abs(xForce) > MOVEMENT_THRESHOLD)
            {
                if (_slopesStick
                    && Controller.IsGroundedAndSlopeOk
                    && Mathf.Abs(moveNormal.y) > SLOPE_STICK_NORMAL_Y_THRESHOLD
                    && Controller.CollisionState.SlopeAngle > SLOPE_STICK_THRESHOLD)
                {
                    SlopeStickMovement(moveNormal, xForce, acceleration);

                    _isMoving = true;
                }
                else if (Controller.IsGroundedAndSlopeOk
                         || !Controller.IsGrounded
                         || (Controller.IsGrounded && _canMoveOnBadSlopes))
                {
                    DefaultMovement(xForce, acceleration);

                    _isMoving = true;
                }
            }
           
            _lastGrounded = grounded;

            if(_applyMoveDirection && _isMoving)
                ApplyMovementDirection();

            if (!_isMoving)
            {
                if (Controller.IsGroundedAndSlopeOk || (Controller.IsGrounded && _canMoveOnBadSlopes))
                {
                    float deceleration = surfaceParams.Deceleration * (1 + surfaceParams.FrictionCoefficient);
                    Controller.SetVelocityX(Mathf.Lerp(Controller.Velocity.x, 0, deceleration));
                }

                if (!Controller.IsGrounded)
                {
                    Controller.SetVelocityX(Mathf.Lerp(Controller.Velocity.x, 0, AirDeceleration));
                }
            }

            UpdateState();
        }

        protected void UpdateState()
        {
            _state.UpdateState(this);
        }

        public void SetSurface(MKSurfaceParameters parameters)
        {
            _currentSurface = parameters;
        }

        public void ResetSurface(MKSurfaceParameters parameters)
        {
            if (_currentSurface == parameters)
                _currentSurface = null;
        }
    }
}
