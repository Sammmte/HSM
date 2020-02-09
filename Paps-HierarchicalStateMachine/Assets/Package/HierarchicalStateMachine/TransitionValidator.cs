using System;
using System.Collections.Generic;

namespace Paps.StateMachines
{
    internal class TransitionValidator<TState, TTrigger> : ITransitionValidator<TState, TTrigger>
    {
        private Dictionary<Transition<TState, TTrigger>, List<IGuardCondition>> _guardConditions;

        private IEqualityComparer<TState> _stateComparer;
        private IEqualityComparer<TTrigger> _triggerComparer;

        private TransitionEqualityComparer _transitionComparer;

        public TransitionValidator(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            _stateComparer = stateComparer ?? throw new ArgumentNullException(nameof(stateComparer));
            _triggerComparer = triggerComparer ?? throw new ArgumentNullException(nameof(triggerComparer));

            _transitionComparer = new TransitionEqualityComparer(_stateComparer, _triggerComparer);

            _guardConditions = new Dictionary<Transition<TState, TTrigger>, List<IGuardCondition>>(_transitionComparer);
        }

        public void AddGuardConditionTo(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            if (_guardConditions.ContainsKey(transition) == false)
                _guardConditions.Add(transition, new List<IGuardCondition>());

            _guardConditions[transition].Add(guardCondition);
        }

        public bool RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            if (_guardConditions.ContainsKey(transition))
            {
                bool removed = _guardConditions[transition].Remove(guardCondition);

                if (_guardConditions[transition].Count == 0)
                    _guardConditions.Remove(transition);

                return removed;
            }

            return false;
        }

        public bool ContainsGuardConditionOn(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            return _guardConditions[transition].Contains(guardCondition);
        }

        public IGuardCondition[] GetGuardConditionsOf(Transition<TState, TTrigger> transition)
        {
            if (_guardConditions.ContainsKey(transition))
                return _guardConditions[transition].ToArray();
            else
                return null;
        }

        public bool IsValid(Transition<TState, TTrigger> transition)
        {
            var guardConditionsList = _guardConditions[transition];

            for(int i = 0; i < guardConditionsList.Count; i++)
            {
                if (guardConditionsList[i].IsValid() == false)
                    return false;
            }

            return true;
        }

        private class TransitionEqualityComparer : IEqualityComparer<Transition<TState, TTrigger>>
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

}