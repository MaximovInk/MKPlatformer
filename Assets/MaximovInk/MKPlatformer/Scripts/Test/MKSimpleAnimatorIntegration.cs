using System;
using MaximovInk.MKPlatformer;
using UnityEngine;

namespace MaximovInk.Assets.MaximovInk.MKPlatformer.Scripts.Test
{
    [RequireComponent(typeof(Animator))]
    public class MKSimpleAnimatorIntegration : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private MKCharacterController _controller;

        private void Awake()
        {
            if(_animator == null)
                _animator = GetComponent<Animator>();

            if (_controller == null)
                _controller = GetComponentInParent<MKCharacterController>();

            
        }

        private void Start()
        {
            _controller.StateMachine.OnStateChange += StateMachine_OnStateChange;
        }

        private void StateMachine_OnStateChange()
        {

            var state = _controller.StateMachine.CurrentState;

            _animator.Play(state.ID);
        }
    }
}
