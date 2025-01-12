using System;
using System.Collections.Generic;

namespace MaximovInk.MKPlatformer
{
    public delegate void OnStateChanged();

    public abstract class MKStateMachineBase<T, T1> where T : struct, IComparable, IEquatable<T> where T1 : struct, IConvertible
    {
        public Action<T> OnStateEnter;
        public Action<T> OnStateExit;

        public virtual event OnStateChanged OnStateChange;

        public abstract IList<T> GetCurrentStates();

        public abstract T CurrentState { get; }
        public abstract T PreviousState { get; }

        protected Func<T, T1> GetStatePriority;

        public MKStateMachineBase(Func<T, T1> getStatePriority)
        {
            GetStatePriority = getStatePriority;
        }

        public abstract void PushState(T newState);
        public abstract void PopState(T state);

        protected virtual void InvokeStateChangeEvent() => OnStateChange?.Invoke();
    }
}
