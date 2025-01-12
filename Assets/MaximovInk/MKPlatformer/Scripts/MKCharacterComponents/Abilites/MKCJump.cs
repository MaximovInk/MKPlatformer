using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public enum JumpCondition
    {
        None,
        OnGround,
        OnGroundSlopeIsOk,
        Anywhere
    }

    //https://anchitsh.github.io/platformer.html
    //http://www.davetech.co.uk/gamedevplatformer
    //https://gamedevbeginner.com/how-to-jump-in-unity-with-or-without-physics/

    public class MKCJump : MKControllerComponent
    {
        private const float FALL_CHECK_TIME = 0.1f;

        [SerializeField] private MKCharacterButton _jumpButton = new()
        {
            ID = "Jump", State = CharacterButtonState.IsDown
        };

        [SerializeField] private bool _isVariableJump = true;
        [SerializeField] private float _jumpHeight = 2f;
        [SerializeField] private JumpCondition _condition = JumpCondition.OnGroundSlopeIsOk;

        [SerializeField] private float _jumpPressDuration = 0.2f;
        [SerializeField] private float _buttonReleaseForce;
        [SerializeField] private int _jumpsCount = 1;
        [SerializeField] private float _inputBufferTime = 0.2f;
        [SerializeField] private float _coyoteTime = 0.2f;

        private int _currentJumpCount;
        private float _pressTimer;
        private bool _isJumping;
        private float _jumpTimer;
        private bool _invokeResetState;

        private bool _invokeJump;
        private bool _lastInvokeJump;
        private bool _bufferInvoked;

        private float _lastJumpInvokedTime;

        private bool _lastGrounded;

        public override void Refresh(MKCharacterController controller)
        {
            base.Refresh(controller);

            _currentJumpCount = _jumpsCount;
        }

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _jumpButton.ReadFrom(input);
        }

        private bool CanJump()
        {
            switch (_condition)
            {
                case JumpCondition.None:
                    return false;
                case JumpCondition.OnGround:
                    return Controller.IsGrounded;
                case JumpCondition.OnGroundSlopeIsOk:
                    return Controller.IsGrounded && Controller.CollisionState.SlopeAngleIsOk;
                case JumpCondition.Anywhere:
                    return true;
            }

            return false;
        }

        private void ReadFromBuffer(bool canJump)
        {
            if (!_invokeJump && _bufferInvoked && _lastJumpInvokedTime < _inputBufferTime && canJump)
            {
                _invokeJump = true;
            }
        }

        private void UpdateBuffer(bool canJump)
        {
            _lastJumpInvokedTime += Controller.DeltaTime;

            if (!canJump && (_invokeJump && !_lastInvokeJump))
            {
                _lastJumpInvokedTime = 0f;
                _bufferInvoked = true;
            }
        }

        private void ResetJumpCountsIfCan(bool canJump)
        {
            if (canJump && _currentJumpCount != _jumpsCount && !_isJumping)
                _currentJumpCount = _jumpsCount;
        }

        private bool ApplyMultiplyJumpCounts(bool canJump)
        {
            if (!Controller.IsGrounded)
            {
                canJump = canJump || _currentJumpCount > 0;
            }

            return canJump;
        }

        private bool ApplyCoyoteTime(bool canJump)
        {
            if (!_isJumping && Controller.TimeGroundAirborne < _coyoteTime)
                canJump = true;

            return canJump;
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var isGrounded = Controller.IsGrounded;
            var canJump = CanJump();

            _invokeJump = _jumpButton.ReadState();

            UpdateBuffer(canJump);

            ReadFromBuffer(canJump);

            if (!isGrounded && _lastGrounded)
            {
                _currentJumpCount--;
            }

            canJump = ApplyCoyoteTime(canJump);

            ResetJumpCountsIfCan(canJump);

            canJump = ApplyMultiplyJumpCounts(canJump);

            if (_invokeJump && canJump)
            {
                if (!_isJumping)
                {
                    _pressTimer = 0f;
                    _jumpTimer = 0f;
                }

                _isJumping = true;
                Jump();
            }

            var isFalling = false;

            if (_isJumping)
            {
                _pressTimer += Time.deltaTime;
                _jumpTimer += Time.deltaTime;

                if (_jumpTimer > FALL_CHECK_TIME && (Controller.Velocity.y < 0.1f || Controller.IsGrounded))
                {
                    isFalling = true;
                }

                var stopJump = isFalling;

                if (_isVariableJump)
                {
                    stopJump = stopJump || !_invokeJump || _pressTimer > _jumpPressDuration;
                }

                if (stopJump)
                {
                    _isJumping = false;
                    _invokeResetState = true;

                    if (_isVariableJump)
                    {
                        Controller.SetVelocityY(_buttonReleaseForce);
                    }
                }
            }

            if (_invokeResetState && (Controller.Velocity.y < 0.1f || Controller.IsGrounded))
            {
                _invokeResetState = false;
                Controller.StateMachine.PopState(MKControllerStates.Jump);
            }

            _jumpButton.Flush();
            _lastInvokeJump = _invokeJump;
            _lastGrounded = isGrounded;
        }

        private void Jump()
        {
            _bufferInvoked = false;

            Controller.StateMachine.PushState(MKControllerStates.Jump);

            var jumpForce =
                Controller.Gravity.Force == 0
                    ? _jumpHeight
                    : Utility.GetJumpForce(_jumpHeight, Controller.Gravity.Force);

            Controller.SetVelocityY(jumpForce);

            if(!Controller.IsGrounded)
                _currentJumpCount--;
        }
    }
}


