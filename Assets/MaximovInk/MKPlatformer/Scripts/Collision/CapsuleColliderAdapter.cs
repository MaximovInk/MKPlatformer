using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class CapsuleColliderAdapter : MonoBehaviour, IColliderAdapter
    {
        public Vector2 Size { get=>_collider.size; set=>_collider.size=value; }
        public Vector2 Offset { get=>_collider.offset; set=>_collider.offset = value; }
        public Vector2 OriginalSize => _initSize;
        public Vector2 OriginalOffset => _initOffset;

        [SerializeField]
        private CapsuleCollider2D _collider;

        private Vector2 _initSize;
        private Vector2 _initOffset;

        public void Initialize()
        {
            if(_collider == null)
                _collider = GetComponent<CapsuleCollider2D>();

            _initSize = _collider.size;
            _initOffset = _collider.offset;
        }

        public void ResetSize()
        {
            _collider.size = _initSize;
            _collider.offset = _initOffset;
        }
    }
}
