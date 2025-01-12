using System;
using System.Linq;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [RequireComponent(typeof(MKCharacterController))]
    public class MKControllerInput : MonoBehaviour
    {
        private MKCharacterController _controller;
        private MKInputManager _input;

        private MKCharacterInput _adapter;

        private void Awake()
        {
            _controller = GetComponent<MKCharacterController>();
            _input = MKInputManager.Instance;

            _adapter = new MKCharacterInput(
                MKInputManager.AllocButtonSize,
                MKInputManager.AllocAxisSize);
        }

        private void Update()
        {
            _adapter.UpdateState(_input.State);
            _controller.HandleInput(_adapter);
        }
    }

    public struct MKCharacterInput : ICharacterInput
    {
        public MKCharacterButtonAdapter[] Buttons;
        public MKCharacterAxisAdapter[] Axes;

        public MKCharacterInput(int buttonsAlloc, int axesAlloc)
        {
            Buttons = new MKCharacterButtonAdapter[buttonsAlloc];
            Axes = new MKCharacterAxisAdapter[axesAlloc];
        }

        public void UpdateState(MKInputState state)
        {
            for (int i = 0; i < state.Keys.Length; i++)
            {
                var key  = state.Keys[i];
                Buttons[i].IsDown = key.IsDown;
                Buttons[i].IsPressed = key.IsPressed;
                Buttons[i].IsUp = key.IsUp;
                Buttons[i].ID = key.ID;
            }

            for (int i = 0; i < state.Axes.Length; i++)
            {
                var axis = state.Axes[i];
                Axes[i].Value = axis.Value;
                Axes[i].RawValue = axis.RawValue;
                Axes[i].ID = axis.ID;
            }
        }

        public ICharacterButton GetButton(string id)
        {
            return Buttons.FirstOrDefault(n => n.ID == id);
        }

        public ICharacterAxis GetAxis(string id)
        {
            return Axes.FirstOrDefault(n => n.ID == id);
        }
    }

    public struct MKCharacterAxisAdapter : ICharacterAxis
    {
        public float Value { get; set; }
        public float RawValue { get; set; }
        public string ID { get; set; }
    }

    public struct MKCharacterButtonAdapter : ICharacterButton
    {
        public bool IsDown { get; set; }
        public bool IsPressed { get; set; }
        public bool IsUp { get; set; }
        public string ID { get; set; }

        public bool IsState(CharacterButtonState state)
        {
            return state switch
            {
                CharacterButtonState.None => !IsDown && !IsPressed && !IsUp,
                CharacterButtonState.IsDown => IsDown,
                CharacterButtonState.IsPressed => IsPressed,
                CharacterButtonState.IsUp => IsUp,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}
