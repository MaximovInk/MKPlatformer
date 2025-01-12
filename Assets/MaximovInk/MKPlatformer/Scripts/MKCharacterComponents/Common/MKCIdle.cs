namespace MaximovInk.MKPlatformer
{
    public class MKCIdle : MKControllerComponent
    {
        private bool _lastIsGrounded;

        public override void PreProcess(float deltaTime)
        {
            base.PreProcess(deltaTime);

            var isGrounded = Controller.IsGrounded;

            if (_lastIsGrounded != isGrounded)
            {
                if (isGrounded)
                    Controller.StateMachine.PushState(MKControllerStates.Idle);
                else
                    Controller.StateMachine.PopState(MKControllerStates.Idle);
            }

            _lastIsGrounded = Controller.IsGrounded;
        }
    }
}
