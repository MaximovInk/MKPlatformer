using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class ScaleToMoveDirection : MonoBehaviour
    {
        [SerializeField] private MKCharacterController _controller;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
            if (_controller == null)
                _controller = GetComponentInParent<MKCharacterController>();

            _controller.OnMoveDirectionChanged += _controller_OnMoveDirectionChanged;
        }

        private void _controller_OnMoveDirectionChanged(int obj)
        {
            var scale = _transform.localScale;

            scale.x = obj;

            _transform.localScale = scale;
        }
    }
}
