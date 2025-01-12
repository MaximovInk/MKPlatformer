
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCPush : MKControllerComponent
    {
        private const float MOVEMENT_THRESHOLD = 0.05f;

        [SerializeField] private string _pushableTag;

        private bool _lastIsPushing;
        private bool _lastIsMovingPushing;

        [SerializeField] private bool _checkAngleIsWall = false;
        [Range(0,1)]
        [SerializeField] private float _checkTolerance = 0.5f;

        private bool IsPushingOther(MKHitInfo info)
        {
            var wall = !_checkAngleIsWall || Controller.CheckWallValid(info, _checkTolerance);

            return wall && info.Collider.CompareTag(_pushableTag);
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var direction = Controller.Direction;

            var isPushing = false;

            if (direction == MovementDirection.Right && Controller.IsCollisionRight)
            {
                isPushing = IsPushingOther(Controller.CollisionState.Right);
            }
            else if (direction == MovementDirection.Left && Controller.IsCollisionLeft)
            {
                isPushing = IsPushingOther(Controller.CollisionState.Left);
            }

            if (_lastIsPushing != isPushing)
            {

                if(isPushing)
                    Controller.StateMachine.PushState(MKControllerStates.PushIdle);
                else
                    Controller.StateMachine.PopState(MKControllerStates.PushIdle);

                _lastIsPushing = isPushing;
            }

            var isMovingPushing = isPushing && Mathf.Abs(Controller.Velocity.x) > MOVEMENT_THRESHOLD;

            if (_lastIsMovingPushing != isMovingPushing)
            {
                if (isMovingPushing)
                    Controller.StateMachine.PushState(MKControllerStates.PushMove);
                else
                    Controller.StateMachine.PopState(MKControllerStates.PushMove);

                _lastIsMovingPushing = isMovingPushing;
            }
        }
    }
}
