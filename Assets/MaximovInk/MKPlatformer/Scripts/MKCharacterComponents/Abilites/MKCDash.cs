using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public enum MKDashCondition
    {
        None,
        Anywhere,
        OnGround,
        OnAir
    }

    public enum DashType
    {
        Custom,
        Faced,
        Mouse,
    }

    public class MKCDash : MKControllerComponent
    {
        [SerializeField]
        private MKCharacterButton _dashButton = new()
        {
            ID = "Dash",
            State = CharacterButtonState.IsDown
        };

        [SerializeField] private float _dashDuration = 0.5f;
        [SerializeField] private float _dashResetDuration = 0.1f;
        [SerializeField] private MKDashCondition _dashCondition = MKDashCondition.Anywhere;
        [SerializeField] private DashType _dashType = DashType.Custom;
        [SerializeField] private Vector2 _dashForce = new(10,10);

        private float _dashTimer;
        private float _dashResetTimer;
        private bool _isDashing;

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _dashButton.ReadFrom(input);
        }

        private bool CanDash()
        {
            switch (_dashCondition)
            {
                case MKDashCondition.None:
                    return false;
                    break;
                case MKDashCondition.Anywhere:
                    return true;
                    break;
                case MKDashCondition.OnGround:
                    return Controller.IsGrounded;
                    break;
                case MKDashCondition.OnAir:
                    return !Controller.IsGrounded;
                    break;
            }

            return true;
        }

        private Vector2 GetDashForce()
        {
            switch (_dashType)
            {
                case DashType.Custom:
                    return _dashForce;
                    break;
                case DashType.Faced:
                    return new Vector2(Controller.GetDirection() * _dashForce.x, _dashForce.y);
                    break;
                case DashType.Mouse:
                    return _dashForce;
                    break;
            }

            return _dashForce;
        }

        public override void PostProcess(float deltaTime)
        {
            base.PostProcess(deltaTime);

            if (_dashButton.ReadState() && CanDash() && _dashResetTimer <= 0)
            {
                StartDash();
            }

            if (_isDashing)
            {
                DashProcess();

                if (_dashTimer > _dashDuration)
                {
                    StopDash();
                }
            }

            if (!_isDashing && _dashResetTimer > -0.1f)
            {
                _dashResetTimer -= Time.deltaTime;
            }

            _dashButton.Flush();
        }

        private void StartDash()
        {
            _isDashing = true;
            _dashTimer = 0f;

            Controller.StateMachine.PushState(MKControllerStates.Dash);
            LockDirectionChange = true;
        }

        private void DashProcess()
        {
            _dashTimer += Time.deltaTime;
            Controller.SetVelocity(GetDashForce());
        }

        private void StopDash()
        {
            _isDashing = false;
            _dashResetTimer = _dashResetDuration;
            LockDirectionChange = false;

            Controller.StateMachine.PopState(MKControllerStates.Dash);
        }

       
    }
}
