using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public enum CharacterButtonState
    {
        None,
        IsDown,
        IsPressed,
        IsUp
    }

    public interface ICharacterButton
    {
        public bool IsDown { get; set; }
        public bool IsPressed { get; set; }
        public bool IsUp { get; set; }
        public string ID { get; set; }

        public bool IsState(CharacterButtonState state);
    }

    public interface ICharacterAxis
    {
        public float Value { get; set; }
        public float RawValue { get; set; }
        public string ID { get; set; }
    }

    public interface ICharacterInput
    {
        public ICharacterButton GetButton(string id);
        public ICharacterAxis GetAxis(string id);
    }

    [System.Serializable]
    public struct MKCharacterButton
    {
        public string ID;
        public CharacterButtonState State;

        public bool IsValid => _rawState != null;

        public ICharacterButton RawState => _rawState;
        private ICharacterButton _rawState;

        private bool _state;

        public bool ReadFrom(ICharacterInput input)
        {
            _rawState = input.GetButton(ID);

            var buttonState = _rawState.IsState(State);

            if (buttonState)
                _state = true;

            return buttonState;
        }

        public bool ReadState()
        {
            return _state;
        }

        public void Flush()
        {
            _state = false;
        }
    }

    [System.Serializable]
    public struct MKCharacterAxis
    {
        public string ID;
        [Tooltip("Threshold of input")]
        public float InputThreshold;
        [Tooltip("Instead of smooth transition of value, use 1,0,-1")]
        public bool RawInput;

        private float _state;

        public float ReadFrom(ICharacterInput input)
        {
            _state = RawInput ? input.GetAxis(ID).RawValue : input.GetAxis(ID).Value;

            if (Mathf.Abs(_state) < InputThreshold)
                _state = 0;

            return _state;
        }

        public float ReadState()
        {
            return _state;
        }

        public void Flush()
        {
            _state = 0f;
        }
    }

}
