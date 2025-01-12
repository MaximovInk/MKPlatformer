using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCCrouch : MKControllerComponent
    {
        [SerializeField] private MKCharacterButton _crouchButton = new() { ID = "Crouch" };

        private MKCHorizontalMovement _movement;
        private bool _movementIsValid;

        [SerializeField] private float _crouchScaleFactor = 0.5f;
        [SerializeField] private float _crouchOffsetFactor = 0.5f;

        [SerializeField] private float _movementMultiplier = 0.5f;

        public override void Refresh(MKCharacterController controller)
        {
            base.Refresh(controller);
            _movement = controller.FindAbility<MKCHorizontalMovement>();

            _movementIsValid = _movement != null;
        }

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);
            _crouchButton.ReadFrom(input);
        }

        private bool _lastIsCrouch = false;
        private bool _lastIsMoving = false;

        private float _crouchOffsetY;
        private bool _crouchedInAir;

        private bool CanStand()
        {
            var hitInfo = Raycast.CheckRays(MKSide.Top, Mathf.Abs(_crouchOffsetY));

            return !hitInfo.IsColliding;
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var isCrouch = _crouchButton.ReadState();

            if (_lastIsCrouch && !isCrouch && !CanStand())
            {
                isCrouch = true;
            }

            if (Controller.StateMachine.HasState(MKControllerStates.Slide))
            {
                isCrouch = false;
            }

            var isMovingCrouch = _movementIsValid && _movement.IsMoving && isCrouch;

            if (isCrouch != _lastIsCrouch)
            {
                if (isCrouch)
                {
                    _crouchedInAir = !Controller.IsGrounded;
                    
                    var size = Controller.Collision.OriginalSize;
                    size.y *= _crouchScaleFactor;
                    var offset = Controller.Collision.OriginalOffset;
                    offset.y -= _crouchedInAir ? -size.y * _crouchOffsetFactor : size.y * _crouchOffsetFactor;
                    _crouchOffsetY = offset.y;

                    Controller.Graphics.transform.localPosition += Vector3.up * offset.y;
                    Controller.Collision.Resize(size, offset);
                    Controller.StateMachine.PushState(MKControllerStates.Crouch);
                }
                else
                {
                    if (Controller.IsGrounded && _crouchedInAir)
                    {
                        var resetOffset = Mathf.Abs(_crouchOffsetY);

                        Controller.transform.position += Vector3.up * resetOffset;
                        Controller.SetVelocityY(resetOffset);
                    }

                    Controller.Graphics.transform.localPosition = Vector3.zero;
                    Controller.Collision.ResetSize();
                    Controller.StateMachine.PopState(MKControllerStates.Crouch);
                }
            }

            if (_lastIsMoving != isMovingCrouch)
            {
                if (_movementIsValid)
                {
                    if (_movement.IsMoving && isCrouch)
                    {
                        Controller.StateMachine.PushState(MKControllerStates.CrouchWalk);
                        _movement.CustomMultiplier = _movementMultiplier;
                    }
                    else
                    {
                        Controller.StateMachine.PopState(MKControllerStates.CrouchWalk);
                        _movement.CustomMultiplier = 1f;
                    }
                }
            }

            _lastIsCrouch = isCrouch;
            _lastIsMoving = isMovingCrouch;

            _crouchButton.Flush();
        }
    }
}
