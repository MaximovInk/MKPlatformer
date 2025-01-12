using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKMovingPlatform : MonoBehaviour, IMovingPlatform
    {
        public Vector2 GetSpeed { get=> new Vector2(Speed, Speed); }
        public Vector3 Position { get=>_transform.position; }
        public Vector3 GetDestination { get=> _target; }
        public Transform Transform { get=>_transform; }

        public Transform[] Targets;

        public float Speed = 1f;
        public float Duration = 1f;
        [SerializeField] private bool loop = true;

        private int _currentPointIndex = 0;
        private Vector3 _previousPosition;
        private Vector3 _velocity;

        private Transform _transform;
        private Vector3 _getDestination;

        private Vector3 _target;

        private void Awake()
        {
            _transform = transform;

            _previousPosition = transform.position;
        }

        private void Update()
        {
            _target = Targets[_currentPointIndex].position;
            transform.position = Vector3.MoveTowards(transform.position, _target, Speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _target) < 0.01f)
            {
                _currentPointIndex++;
                if (_currentPointIndex >= Targets.Length)
                {
                    _currentPointIndex = loop ? 0 : Targets.Length - 1;
                }
            }

            _velocity = (transform.position - _previousPosition) / Time.deltaTime;
            _previousPosition = transform.position;
        }
    }
}