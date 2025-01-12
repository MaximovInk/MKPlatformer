using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCGravity : MKControllerComponent
    {
        public float Force
        {
            get => _gravityForce;
            set=>_gravityForce = value;
        }

        public float Angle
        {
            get => _gravityAngle;
            set => _gravityAngle = value;
        }

        [SerializeField] private float _gravityForce = -9.81f;
        [SerializeField] private float _gravityAngle;

        public override void PreProcess(float deltaTime)
        {
            base.PreProcess(deltaTime);

            //Controller.transform.localRotation = Quaternion.Euler(0,0,_gravityAngle);

           // if(!Controller.IsGrounded || Mathf.Abs(Controller.Velocity.y) < Mathf.Abs(_gravityForce))
            Controller.AddVelocityY(deltaTime * _gravityForce);

            if (Controller.IsGrounded)
            {
                Controller.StateMachine.PopState(MKControllerStates.Fall);
            }
            else
            {
                Controller.StateMachine.PushState(MKControllerStates.Fall);
            }
           
        }

        public override void PostProcess(float deltaTime)
        {
            base.PostProcess(deltaTime);

            /* if (Controller.IsGroundedAndSlopeOk && Controller.State.Below.Distance > 0.005f)
             {
                 Debug.Log($"adding dst {Controller.State.Below.Distance}");

                 Controller.AddVelocityY(deltaTime * _gravityForce);
                 //Controller.AddVelocityY(-Controller.State.Below.Distance);
             }*/
        }
    }
}
