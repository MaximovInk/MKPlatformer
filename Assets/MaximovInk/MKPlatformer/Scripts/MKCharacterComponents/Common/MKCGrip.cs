using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCGrip : MKControllerComponent
    {
        private MKGrip _attached;

        [SerializeField] private MKCharacterButton _releaseButton;

        [SerializeField] private float _smoothTime = 1f;

        private Vector3 _refVelocity;

        public void SetGrip(MKGrip grip)
        {
            _attached = grip;

        }

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _releaseButton.ReadFrom(input);
        }

        public override void PostProcess(float deltaTime)
        {
            base.PostProcess(deltaTime);

            if (_releaseButton.ReadState())
            {
                _attached = null;
            }

            if (_attached != null)
            {

                Controller.SetVelocity(Vector2.zero);
                Controller.SetExternalVelocity(Vector2.zero);

               // Controller.transform.position = _attached.GetPosition();

                Controller.SetPosition(Vector3.SmoothDamp(
                    Controller.transform.position, 
                    _attached.GetPosition(),
                    ref _refVelocity, 
                    _smoothTime,
                    Controller.Parameters.MaxVelocity.x,
                    Controller.DeltaTime));
            }

            _releaseButton.Flush();
        }
    }
}
