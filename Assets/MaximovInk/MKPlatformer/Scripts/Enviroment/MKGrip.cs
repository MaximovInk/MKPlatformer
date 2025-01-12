using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKGrip : MonoBehaviour
    {
        public Vector2 Offset => _offset;

        [SerializeField] private Vector2 _offset;

        private MKCharacterController _controller;

        public Vector3 GetPosition()
        {
            return transform.position + (Vector3)_offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var collision = other.GetComponent<MKControllerCollision>();

            _controller = collision != null ? collision.Controller : null;

            if (_controller != null)
            {
                var mkcGrip = _controller.FindAbility<MKCGrip>();

                if (mkcGrip != null)
                {
                    mkcGrip.SetGrip(this);
                }
            }
        }
    }
}
