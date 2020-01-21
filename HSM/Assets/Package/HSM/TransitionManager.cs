using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    internal class TransitionManager<TState, TTrigger>
    {
        private StateHierarchy<TState> _stateHierarchy;
        private IEqualityComparer<TState> _stateComparer;
        private IEqualityComparer<TTrigger> _triggerComparer;
        private TransitionEqualityComparer _transitionComparer;
        public int TransitionCount => _transitions.Count;
        
        private HashSet<Transition<TState, TTrigger>> _transitions;
        private Dictionary<Transition<TState, TTrigger>, List<IGuardCondition>> _guardConditions;

        public TransitionManager(StateHierarchy<TState> stateHierarchy, IEqualityComparer<TState> stateComparer = null, IEqualityComparer<TTrigger> triggerComparer = null)
        {
            _stateHierarchy = stateHierarchy;
            _stateComparer = stateComparer ?? EqualityComparer<TState>.Default;
            _triggerComparer = triggerComparer ?? EqualityComparer<TTrigger>.Default;
            _transitionComparer = new TransitionEqualityComparer(_stateComparer, _triggerComparer);
            _transitions = new HashSet<Transition<TState, TTrigger>>();
            _guardConditions = new Dictionary<Transition<TState, TTrigger>, List<IGuardCondition>>();
        }

        public void SetStateComparer(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer = stateComparer;
            _transitionComparer.StateComparer = _stateComparer;
        }

        public void SetTriggerComparer(IEqualityComparer<TTrigger> triggerComparer)
        {
            _triggerComparer = triggerComparer;
            _transitionComparer.TriggerComparer = triggerComparer;
        }

        public void AddTransition(Transition<TState, TTrigger> transition)
        {
            _transitions.Add(transition);
        }

        public bool RemoveTransition(Transition<TState, TTrigger> transition)
        {
            return _transitions.Remove(transition);
        }

        public bool ContainsTransition(Transition<TState, TTrigger> transition)
        {
            return _transitions.Contains(transition);
        }

        public void AddGuardConditionTo(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            ValidateContainsTransition(transition);
            
            if(_guardConditions.ContainsKey(transition) == false)
                _guardConditions.Add(transition, new List<IGuardCondition>());
            
            _guardConditions[transition].Add(guardCondition);
        }

        public bool RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            if (_guardConditions.ContainsKey(transition))
            {
                return _guardConditions[transition].Remove(guardCondition);
            }

            return false;
        }

        private void ValidateContainsTransition(Transition<TState, TTrigger> transition)
        {
            if(ContainsTransition(transition) == false) 
                throw new TransitionNotAddedException(transition.StateFrom.ToString(),
                    transition.Trigger.ToString(),
                    transition.StateTo.ToString());
        }

        public bool TryTransition(TTrigger trigger)
        {
            return false;
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