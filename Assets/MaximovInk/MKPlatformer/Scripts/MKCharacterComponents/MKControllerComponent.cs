using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKControllerComponent : MonoBehaviour
    {
        public bool ReadInput = true;

        protected MKCharacterController Controller { get; private set; }
        protected MKControllerCollision Collision { get; private set; }
        protected MKControllerRaycast Raycast { get; private set; }

        public virtual bool IsActive => enabled && gameObject.activeSelf;

        public bool IsInitialized { get; private set; }

        public bool LockDirectionChange { get; protected set; }


        protected virtual void Start()
        {
            Initialization();
        }

        private void Initialization()
        {
            if(!IsInitialized)
                GetComponentInParent<MKCharacterController>()?.RefreshAbilities();
        }

        public virtual void Refresh(MKCharacterController controller)
        {
            Controller = controller;
            Collision = controller.Collision;
            Raycast = controller.Raycast;

            IsInitialized = true;
        }

        public virtual void HandleInput(ICharacterInput input)
        {
            
        }

        public virtual void ResetInput()
        {

        }

        public virtual void PreProcess(float deltaTime)
        {

        }

        public virtual void Process(float deltaTime)
        {

        }

        public virtual void PostProcess(float deltaTime)
        {

        }

        public virtual void TriggerEnter(Collider2D other)
        {

        }

        public virtual void TriggerStay(Collider2D other)
        {

        }

        public virtual void TriggerExit(Collider2D other)
        {

        }
    }
}
