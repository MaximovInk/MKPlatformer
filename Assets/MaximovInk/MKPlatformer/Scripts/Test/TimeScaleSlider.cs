using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaximovInk.MKPlatformer
{
    [RequireComponent(typeof(Slider))]
    public class TimeScaleSlider : MonoBehaviour
    {
        private Slider _slider;

        [SerializeField] private TextMeshProUGUI _textInfo;

        [SerializeField] private float _minScale = 0f;
        [SerializeField] private float _maxScale = 2f;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(UpdateTime);

            UpdateText();
        }

        private void UpdateTime(float newValue)
        {
            Time.timeScale = Mathf.Lerp(_minScale,_maxScale, _slider.normalizedValue);

            UpdateText();
        }

        private void UpdateText()
        {
            if (_textInfo == null) return;

            _textInfo.text = $"Time scale: {Time.timeScale}";
        }
    }
}
