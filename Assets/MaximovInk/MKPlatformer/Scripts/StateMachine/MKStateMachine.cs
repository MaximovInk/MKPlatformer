using System;
using System.Collections.Generic;
using System.Linq;

namespace MaximovInk.MKPlatformer
{
    public class MKStateMachine<T,T1> : MKStateMachineBase<T, T1> where T : struct, IComparable, IEquatable<T> where T1 : struct, IConvertible
    {
        public override IList<T> GetCurrentStates()
        {
            return _currentStates.Select(n=>n.Value).ToList();
        }

        public override T CurrentState => _currentState;
        public override T PreviousState => _previousState;

        //private readonly List<T> _currentStates = new();
        private readonly SortedList<T1, T> _currentStates = new SortedList<T1, T>();

        private T _currentState;
        private T _previousState;

        public MKStateMachine(Func<T, T1> getStatePriority) : base(getStatePriority)
        {
            if (getStatePriority == null)
            {
                throw new ArgumentNullException(nameof(getStatePriority), "GetStatePriority cannot be null");
            }
        }

        public override void PushState(T newState)
        {
            if (_currentStates.ContainsValue(newState))
                return;

            T1 priority = GetStatePriority(newState);
            if (!_currentStates.ContainsKey(priority))
            {
                _currentStates.Add(priority, newState);
            }
            else
            {
                // If there's already a state at this priority, replace it
                _currentStates[priority] = newState;
            }

            CalculateStates();

            // Trigger state enter event
            OnStateEnter?.Invoke(newState);
        }

        /*public override void PushState(T newState)
        {
            if (_currentStates.Contains(newState))
                return;

            _currentStates.Add(newState);

            CalculateStates();
        }*/

        public override void PopState(T state)
        {
            T1 priority = GetStatePriority(state);
            if (_currentStates.Values.Contains(state))
            {
                _currentStates.Remove(priority);

                CalculateStates();

                // Trigger state exit event
                OnStateExit?.Invoke(state);
            }
        }

        /*public override void PopState(T state)
        {
            if (!_currentStates.Contains(state))
                return;

            _currentStates.Remove(state);

            CalculateStates();
        }*/

        public bool HasState(T state) => _currentStates.Values.Contains(state);

        /* public bool HasState(T state)
         {
             return _currentStates.Contains(state);
         }*/

        /* private void CalculateStates()
         {
             var newState = _currentStates.OrderByDescending(n => GetStatePriority(n)).FirstOrDefault();

             if (EqualityComparer<T>.Default.Equals(newState,_currentState))
             {
                 return;
             }

             _previousState = _currentState;
             _currentState = newState;

             InvokeStateChangeEvent();
         }*/

        private void CalculateStates()
        {
            if (_currentStates.Count == 0)
            {
                if (!EqualityComparer<T>.Default.Equals(_currentState, default(T)))
                {
                    _previousState = _currentState;
                    _currentState = default; // Reset to default if no states are active
                    InvokeStateChangeEvent();
                }
                return;
            }

            T newState = _currentStates.Values.Last(); // Last item due to reverse sorting in SortedList

            if (!EqualityComparer<T>.Default.Equals(newState, _currentState))
            {
                _previousState = _currentState;
                _currentState = newState;
                InvokeStateChangeEvent();
            }
        }


        // Method to clear all states
        public void ClearStates()
        {
            foreach (var state in _currentStates.Values.ToList())
            {
                PopState(state);
            }
        }
    }
}
