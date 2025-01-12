using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCWallJump : MKControllerComponent
    {
        [SerializeField]
        private MKCharacterButton _jumpButton = new()
        {
            ID = "Jump",
            State = CharacterButtonState.IsDown
        };

        [SerializeField] private Vector2 _force;

        [Range(0, 1)][SerializeField] private float _tolerance;
        [SerializeField] private float _coyoteTime = 0.2f;

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _jumpButton.ReadFrom(input);
        }

        private bool CheckWall(MKHitInfo info)
        {
            return Controller.CheckWallValid(info, _tolerance);
        }

        private bool _isWallJumping = false;
        private float _jumpTimer = 0;

        private bool _invokeJump;

        private float _timeSinceIsWall;
        private bool _lastIsOnWall;
        private MovementDirection _wallDirection;

        private bool ApplyCoyoteTime(bool canJump)
        {
            if (!_isWallJumping && _timeSinceIsWall < _coyoteTime)
                canJump = !Controller.IsGrounded;

            return canJump;
        }

        public override void PostProcess(float deltaTime)
        {
            base.Process(deltaTime);

            var leftIsWall = CheckWall(Controller.CollisionState.Left);
            var rightIsWall = CheckWall(Controller.CollisionState.Right);

            var isOnWall = (leftIsWall || rightIsWall) && !Controller.IsGrounded;

            if (isOnWall && !_isWallJumping)
            {
                _wallDirection = leftIsWall ? MovementDirection.Left : MovementDirection.Right;
            }

            var canJump = isOnWall;
            _invokeJump = _jumpButton.ReadState();

            canJump = ApplyCoyoteTime(canJump);

            if (canJump && _invokeJump)
            {
                Jump();
            }

            if (_isWallJumping)
            {
                _jumpTimer += Controller.DeltaTime;
            }

            if (_jumpTimer > 0.1f && _isWallJumping &&
                (Controller.Velocity.y < 0 || Controller.IsGrounded || isOnWall))
            {
                _isWallJumping = false;
                Controller.StateMachine.PopState(MKControllerStates.WallJump);
            }

            _jumpButton.Flush();

            if (!isOnWall)
            {
                _timeSinceIsWall += Time.deltaTime;
            }

            if (isOnWall != _lastIsOnWall)
            {
                _timeSinceIsWall = 0f;
                _lastIsOnWall = isOnWall;
            }
        }

        private void Jump()
        {
            _jumpTimer = 0f;

            Controller.StateMachine.PushState(MKControllerStates.WallJump);
            _isWallJumping = true;

            var force = _force;
            force.x *= -Controller.GetDirection(_wallDirection);

            Controller.SetDirection(_wallDirection == MovementDirection.Right ? MovementDirection.Left : MovementDirection.Right);

            Controller.SetVelocity(force);
        }
    }
}
