using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCSlide : MKControllerComponent
    {
        private MKCHorizontalMovement _movement;

        [SerializeField] private float _predelayTimeOfState = 0.1f;

        private float _timer;

        public override void Refresh(MKCharacterController controller)
        {
            base.Refresh(controller);

            _movement = controller.FindAbility<MKCHorizontalMovement>();
        }

        private bool CanAnimate()
        {
            if (_movement == null) return true;
            return !_movement.CanMoveOnBadSlopes;
        }

        private bool _lastIsSliding;

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var isSliding = false;

            var below = Controller.CollisionState.Below;

            if (Controller.IsGrounded && !Controller.IsSlopeOk && Controller.Velocity.y < 0.5f && CanAnimate())
            {
                _timer += Time.deltaTime;

                if (_timer > _predelayTimeOfState)
                {
                    isSliding = true;
                }
            }

            else
            {
                _timer = 0f;
            }


            if (_lastIsSliding != isSliding)
            {
                _lastIsSliding = isSliding;

                if (isSliding)
                {
                    Controller.StateMachine.PushState(MKControllerStates.Slide);
                    Controller.Graphics.transform.up = below.Normal;
                    if (!Controller.LockDirectionChange)
                    {
                        var a = Vector2.SignedAngle(Controller.transform.up, below.Normal);
                        Controller.SetDirection(a < 0 ? MovementDirection.Right : MovementDirection.Left);
                    }

                    LockDirectionChange = true;
                }
                else
                {
                    Controller.StateMachine.PopState(MKControllerStates.Slide);
                    Controller.Graphics.transform.up = Controller.transform.up;
                    LockDirectionChange = false;
                }
            }

        }
    }
}
