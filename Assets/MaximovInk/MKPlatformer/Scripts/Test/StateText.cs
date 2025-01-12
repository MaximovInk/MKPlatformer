using TMPro;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class StateText : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        [SerializeField] private MKCharacterController _controller;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            if (_controller == null) return;

            _text.text = _controller.ToString();
        }
    }
}
