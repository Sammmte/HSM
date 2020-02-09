using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.StateMachines
{
    internal class TransitionEqualityComparer<TState, TTrigger> : IEqualityComparer<Transition<TState, TTrigger>>
    {
        public IEqualityComparer<TState> StateComparer;
        public IEqualityComparer<TTrigger> TriggerComparer;

        public TransitionEqualityComparer(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            StateComparer = stateComparer;
            TriggerComparer = triggerComparer;
        }

        public bool Equals(Transition<TState, TTrigger> x, Transition<TState, TTrigger> y)
        {
            return StateComparer.Equals(x.StateFrom, y.StateFrom) && TriggerComparer.Equals(x.Trigger, y.Trigger) && StateComparer.Equals(x.StateTo, y.StateTo);
        }

        public int GetHashCode(Transition<TState, TTrigger> obj)
        {
            return (obj.StateFrom, obj.Trigger, obj.StateTo).GetHashCode();
        }

        public bool Equals(Transition<TState, TTrigger> transition, TState stateFrom, TTrigger trigger, TState stateTo)
        {
            return StateComparer.Equals(transition.StateFrom, stateFrom) && TriggerComparer.Equals(transition.Trigger, trigger) && StateComparer.Equals(transition.StateTo, stateTo);
        }
    }
}
