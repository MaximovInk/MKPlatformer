using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [System.Serializable]
    public class MKSHorizontalMovement : MKStateComponent<MKCHorizontalMovement>
    {
        [SerializeField] private float _minSpeedForWalkAnimation = 0.1f;
        [SerializeField] private float _minSpeedForRunAnimation = 2.0f;
        [SerializeField] private float _idleAnimationSpeed = 1.0f;
        [SerializeField] private float _walkAnimationSpeed = 1.2f;
        [SerializeField] private float _runAnimationSpeed = 1.5f;

        private MKControllerState WalkState => MKControllerStates.Walk;
        private MKControllerState RunState => MKControllerStates.Run;

        private bool _lastIsWalking = false;
        private bool _lastIsRun = false;

        public override void UpdateState(MKCHorizontalMovement from)
        {
            var speed = Mathf.Abs(Controller.Velocity.x);

            var isWalking = from.IsMoving && speed > _minSpeedForWalkAnimation && speed < _minSpeedForRunAnimation;
            var isRunning = from.IsMoving && speed > _minSpeedForRunAnimation;

            if (_lastIsWalking != isWalking)
            {
                if (isWalking)
                    Controller.StateMachine.PushState(WalkState);
                else
                    Controller.StateMachine.PopState(WalkState);
            }

            if(_lastIsRun != isRunning)
            {
                if (isRunning)
                    Controller.StateMachine.PushState(RunState);
                else
                    Controller.StateMachine.PopState(RunState);
            }

            _lastIsRun = isRunning;
            _lastIsWalking = isWalking;
        }
    }
}
