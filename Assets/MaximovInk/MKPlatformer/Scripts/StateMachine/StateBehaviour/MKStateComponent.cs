using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [System.Serializable]
    public abstract class MKStateComponent<T> where T : MKControllerComponent
    {
        protected MKCharacterController Controller { get; private set; }

        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {

        }

        public virtual void Refresh(MKCharacterController controller)
        {
            Controller = controller;
        }

        public abstract void UpdateState(T from);
    }
}
