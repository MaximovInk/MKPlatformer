using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MKControllerCollision : MonoBehaviour
    {
        public MKCharacterController Controller => _controller;

        public BoxCollider2D BoundsCollider { get; private set; }

        public Vector2 OriginalSize => _initSize;
        public Vector2 OriginalOffset => _initOffset;

        public float Width => _width;
        public float Height => _height;

        public float HalfWidth => _halfWidth;
        public float HalfHeight => _halfHeight;

        public Vector2 Size => Vector2.Scale(transform.lossyScale, BoundsCollider.size);
        public Vector2 Offset => BoundsCollider.offset;

        public Vector2 WorldCenter => BoundsCollider.bounds.center;
        public Bounds Bounds => BoundsCollider.bounds;

        private Vector2 _initSize;
        private Vector2 _initOffset;

        private float _width;
        private float _height;

        private float _halfWidth;
        private float _halfHeight;

        private MKCharacterController _controller;

        private readonly List<Collider2D> _ignoredColliders = new();

        private IColliderAdapter[] _adapters;

        private Collider2D[] _attached;

        public virtual void Initialize()
        {
            BoundsCollider = GetComponent<BoxCollider2D>();

            _controller = GetComponentInParent<MKCharacterController>();
            _attached = GetComponentsInChildren<Collider2D>();
            _adapters = GetComponentsInChildren<IColliderAdapter>();

            _initSize = BoundsCollider.size;
            _initOffset = BoundsCollider.offset;

            foreach (var adapter in _adapters)
            {
                adapter.Initialize();
            }

            CalculateParameters();
        }

        public virtual void Resize(Vector2 newSize, Vector2 offset)
        {
            BoundsCollider.size = newSize;
            BoundsCollider.offset = offset;

            foreach (var adapter in _adapters)
            {
                adapter.Size = newSize;
                adapter.Offset = offset;
            }

            CalculateParameters();
        }

        public virtual void ResetSize()
        {
            BoundsCollider.size = _initSize;
            BoundsCollider.offset = _initOffset;

            foreach (var adapter in _adapters)
            {
                adapter.ResetSize();
            }

            CalculateParameters();
        }

        public virtual void ResizeByFactor(Vector2 factor, Vector2 offset)
        {
            BoundsCollider.size = _initSize * factor;
            BoundsCollider.offset = offset;

            foreach (var adapter in _adapters)
            {
                adapter.Size = adapter.OriginalSize * factor;
                adapter.Offset = offset;
            }

        }

        private void CalculateParameters()
        {
            var size = Size;

            _width = size.x;
            _height = size.y;

            _halfWidth = size.x / 2f;
            _halfHeight = size.y / 2f;
        }

        public void AddIgnoreCollider(Collider2D other)
        {
            foreach (var coll in _attached)
            {
                Physics2D.IgnoreCollision(other, coll);
            }
            
            _ignoredColliders.Add(other);
        }

        public void RemoveIgnoreCollider(Collider2D other)
        {
            foreach (var coll in _attached)
            {
                Physics2D.IgnoreCollision(other, coll, false);
            }

            _ignoredColliders.Remove(other);
        }

        public bool IsIgnore(Collider2D other)
        {
            return _ignoredColliders.Contains(other);
        }

        public bool IsThis(Collider2D other)
        {
            return _attached.Any(n => n == other);
        }
    }
}
