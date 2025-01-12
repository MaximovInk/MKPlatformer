using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class FollowRotation : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        [SerializeField] private float _speed = 5;

        private void FixedUpdate()
        {
            if (_target == null) return;

            transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, Time.deltaTime * _speed);
        }
    }
}
