using UnityEngine;

namespace MaximovInk.MKPlatformer
{

    public class MKCRotation : MKControllerComponent
    {
        private MKCGravity _gravity;

        [SerializeField] private bool _rotateToGravity = true;
       // [SerializeField] private bool _rotateToNormal;

        private bool _gravityIsValid;

        public override void Refresh(MKCharacterController controller)
        {
            base.Refresh(controller);

            _gravity = Controller.FindAbility<MKCGravity>();
            _gravityIsValid = _gravity != null;
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var applyGravity =  _rotateToGravity && _gravityIsValid;
           // var applyNormal = _rotateToNormal && Controller.IsGrounded && Controller.CollisionState.SlopeAngleIsOk;

            if(_gravityIsValid)
                Controller.transform.localRotation =
                 Quaternion.Euler(0, 0, applyGravity ? _gravity.Angle : 0);

            /*  if (applyNormal != _lastRotateToNormal)
              {
                  Controller.Graphics.localRotation =
                      Quaternion.Euler(0, 0, applyNormal ? Controller.CollisionState.Below.WorldAngle : 0);
              }

              _lastRotateToNormal = applyNormal;*/
        }
    }
}
