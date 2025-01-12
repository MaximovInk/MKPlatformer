using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class InstanceTest : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private int _count = 100;

        private void Start()
        {
            for (int i = 0; i < _count; i++)
            {
                var instance = Instantiate(_target, transform);
            }
        }

    }
}
