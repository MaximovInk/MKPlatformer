using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaximovInk
{
    [System.Serializable]
    public struct MKInputAxis
    {
        public string ID;
        public float Value;
        public float RawValue;
    }

    [System.Serializable]
    public struct MKInputButton
    {
        public string ID;
        public bool IsPressed;
        public bool IsDown;
        public bool IsUp;

        public bool IsState(ButtonState state)
        {
            return state switch
            {
                ButtonState.None => !IsDown && !IsPressed && !IsUp,
                ButtonState.IsPressed => IsPressed,
                ButtonState.IsDown => IsDown,
                ButtonState.IsUp => IsUp,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }

    public enum ButtonState
    {
        None,
        IsPressed,
        IsDown,
        IsUp
    }

    [System.Serializable]
    public struct MKInputState
    {
        public Vector2 MovementInput;
        public MKInputAxis[] Axes;
        public MKInputButton[] Keys;

        public MKInputAxis GetAxis(string id)
        {
            return Axes.FirstOrDefault(n => n.ID == id);
        }

        public MKInputButton GetButton(string id)
        {
            return Keys.FirstOrDefault(n => n.ID == id);
        }

        public void Reset()
        {
            for (int i = 0; i < Axes.Length; i++)
            {
                Axes[i].Value = 0f;
            }

            for (int i = 0; i < Keys.Length; i++)
            {
                Keys[i].IsDown = false;
                Keys[i].IsPressed = false;
                Keys[i].IsUp = false;
            }
        }
    }

    public class MKInputManager : MonoBehaviourSingleton<MKInputManager>
    {
        public event Action<MKInputState> OnStateUpdated;

        public MKInputState State => _state;
        private MKInputState _state;

        private Dictionary<int, KeyCode> _keysAttached;
        private Dictionary<int, string> _axesAttached;

        public const int AllocAxisSize = 20;
        public const int AllocButtonSize = 20;

        private void Awake()
        {
            _state = new MKInputState()
            {
                Axes = new MKInputAxis[AllocAxisSize],
                Keys = new MKInputButton[AllocButtonSize],
            };

            _keysAttached = new Dictionary<int, KeyCode>();
            _axesAttached = new Dictionary<int, string>();

            AttachAxis("MoveX", "Horizontal");
            AttachAxis("MoveY", "Vertical");

            AttachKey("Jump", KeyCode.Space);
            AttachKey("Jetpack", KeyCode.Space);
            AttachKey("Dash", KeyCode.F);
            AttachKey("OneWay", KeyCode.S);
            AttachKey("Crouch", KeyCode.LeftControl);
            AttachKey("Sprint", KeyCode.LeftShift);
            AttachKey("Walk", KeyCode.C);

            AttachKey("Look", KeyCode.LeftAlt);
            AttachAxis("LookX", "Mouse X");
            AttachAxis("LookY", "Mouse Y");

            AttachKey("Fire", KeyCode.Mouse0);
            AttachKey("FireAlt", KeyCode.Mouse1);
        }

        public void AttachKey(string ID, KeyCode key)
        {
            var idx = _keysAttached.Count;

            if (_keysAttached.TryAdd(idx, key))
            {
                _state.Keys[idx] = new MKInputButton()
                {
                    ID = ID
                };
            }

            _keysAttached[idx] = key;
        }

        public void AttachAxis(string ID, string axis)
        {
            var idx = _axesAttached.Count;

            if (_axesAttached.TryAdd(idx, axis))
            {
                _state.Axes[idx] = new MKInputAxis()
                {
                    ID = ID
                };
            }

            _axesAttached[idx] = axis;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            for (int i = 0; i < _keysAttached.Count; i++)
            {
                var key = _keysAttached[i];

                var state = _state.Keys[i];

                state.IsPressed = Input.GetKey(key);
                state.IsDown = Input.GetKeyDown(key);
                state.IsUp = Input.GetKeyUp(key);

                _state.Keys[i] = state;
            }

            for (int i = 0; i < _axesAttached.Count; i++)
            {
                var axis = _axesAttached[i];

                var state = _state.Axes[i];

                state.Value = Input.GetAxis(axis);
                state.RawValue = Input.GetAxisRaw(axis);

                _state.Axes[i] = state;
            }

            OnStateUpdated?.Invoke(_state);
        }
    }
}
