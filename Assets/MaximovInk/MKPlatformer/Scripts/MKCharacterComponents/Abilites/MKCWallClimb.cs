using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCWallClimb : MKControllerComponent
    {
        [Range(0,1)]
        [SerializeField] private float _slowGravityFactor;

        [Range(0, 1)] [SerializeField] private float _tolerance;
        [SerializeField] private bool _stopOnClimb;

        private bool _lastIsWallClimb = false;

        private bool CheckWall(MKHitInfo info)
        {
            return Controller.CheckWallValid(info, _tolerance); 
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var isWallClimb = false;

            var attachedToLeft = CheckWall(Controller.CollisionState.Left) && Controller.Direction == MovementDirection.Left;
            var attachedToRight = CheckWall(Controller.CollisionState.Right) && Controller.Direction == MovementDirection.Right;

            var isOnWall = ((attachedToLeft) || attachedToRight) && !Controller.IsGrounded;

            if (isOnWall)
            {
                Controller.SetVelocityY(Mathf.Lerp(Controller.Velocity.y, 0, _slowGravityFactor));
                isWallClimb = true;
            }

            if (_lastIsWallClimb != isWallClimb)
            {
                if (isWallClimb)
                {
                    Controller.StateMachine.PushState(MKControllerStates.WallClimb);

                    if (_stopOnClimb)
                    {
                        Controller.SetVelocityX(attachedToLeft ? -5 : 5);
                        Controller.SetVelocityY(0);
                    }
                }
                else
                {
                    Controller.StateMachine.PopState(MKControllerStates.WallClimb);
                }


                _lastIsWallClimb = isWallClimb;
            }
        }
    }
}
