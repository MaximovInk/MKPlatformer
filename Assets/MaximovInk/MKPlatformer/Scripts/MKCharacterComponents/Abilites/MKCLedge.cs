using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCLedge : MKControllerComponent
    {
        private bool _lastIsWallClimb;

        [SerializeField] private bool _autoRotateToWall = true;
        [SerializeField] private bool _autoMagnetToWall = true;
        [SerializeField] private float _autoMagnetRayLenght = 0.5f;


        [Range(0,1)]
        [SerializeField] private float _tolerance;


        //7
        //0
        //6
        //

        private bool CheckWall(MKHitInfo info)
        {
            return
                //  info.RaySuccessCount == info.RayCount - 1 &&
                Controller.CheckWallValid(info, _tolerance);
        }

        private bool ProcessWall(MKSide side, MovementDirection targetDirection, MKHitInfo info, float minWallAngle, float maxWallAngle)
        {
            var attached = CheckWall(info) && !info.TopEdge;

            if (!attached && _autoMagnetToWall)
            {
                info = Raycast.CheckRays(side, _autoMagnetRayLenght, minWallAngle, maxWallAngle);
                attached = CheckWall(info) && !info.TopEdge;
            }

            if (attached)
            {
                if (_autoRotateToWall && Controller.Direction != targetDirection)
                    Controller.SetDirection(targetDirection);

                attached &= Controller.Direction == targetDirection;
            }

            return attached;
        }

        public override void Process(float deltaTime)
        {
            var minWallAngle = Controller.Parameters.MinWallAngle;
            var maxWallAngle = Controller.Parameters.MaxWallAngle;

            base.Process(deltaTime);

            var isWallClimb = false;

            var firstWall = false;
            var secondWall = false;

            var dir = Controller.Direction;

            if (Controller.Direction == MovementDirection.Right)
            {
                firstWall = ProcessWall(MKSide.Right, MovementDirection.Right, Controller.CollisionState.Right,
                    minWallAngle, maxWallAngle);
                
                if(!firstWall)
                    secondWall= ProcessWall(MKSide.Left, MovementDirection.Left, Controller.CollisionState.Left,
                        minWallAngle, maxWallAngle);

                dir = firstWall ? MovementDirection.Right : MovementDirection.Left;
            }

            if (Controller.Direction == MovementDirection.Left)
            {
                firstWall = ProcessWall(MKSide.Left, MovementDirection.Left, Controller.CollisionState.Left,
                    minWallAngle, maxWallAngle);

                if(!firstWall)
                    secondWall = ProcessWall(MKSide.Right, MovementDirection.Right, Controller.CollisionState.Right,
                        minWallAngle, maxWallAngle);

                dir = firstWall ? MovementDirection.Left : MovementDirection.Right;
            }

            var isOnWall = ((firstWall) || secondWall) && !Controller.IsGrounded;

            if (isOnWall)
            {
                Controller.SetVelocityY(0);
                isWallClimb = true;
            }

            if (_lastIsWallClimb != isWallClimb)
            {
                if (isWallClimb)
                {
                    Controller.StateMachine.PushState(MKControllerStates.WallClimb);

                    Controller.SetVelocityX(dir == MovementDirection.Left ? -5 : 5);
                    Controller.SetVelocityY(0);

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
