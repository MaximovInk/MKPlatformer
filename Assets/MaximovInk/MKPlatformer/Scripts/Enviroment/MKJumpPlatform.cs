using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKJumpPlatform:MonoBehaviour
    {
        [SerializeField] private float _jumpHeight;

        private MKCharacterController _controller;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collision = other.GetComponent<MKControllerCollision>();

            _controller = collision != null ? collision.Controller : null;
        }

        private void Update()
        {
            if (_controller != null)
            {
                var gravity = _controller.FindAbility<MKCGravity>();

                var force = _jumpHeight;

                if (gravity != null)
                {
                    force = Utility.GetJumpForce(_jumpHeight, gravity.Force);
                }
                //_controller.SetExternalVelocityY(force);
                //_controller.SetVelocityY(force);
                //_controller.InvokeResetExternalVelocityOnApply();

                _controller.PushPostProcessAction((c) =>
                {
                    c.SetVelocityY(force);
                });

                _controller = null;
            }
        }
    }
}
