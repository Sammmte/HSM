using System.Collections.Generic;
using System.Linq;

namespace Paps.StateMachines
{
    internal class TransitionManager<TState, TTrigger>
    {
        public int TransitionCount => _transitions.Count;

        private IEqualityComparer<TState> _stateComparer;
        private IEqualityComparer<TTrigger> _triggerComparer;
        private TransitionEqualityComparer<TState, TTrigger> _transitionEqualityComparer;

        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private ITransitionValidator<TState, TTrigger> _transitionValidator;

        private HashSet<Transition<TState, TTrigger>> _transitions;

        public TransitionManager(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer, 
            StateHierarchyBehaviourScheduler<TState> stateHierarchyBehaviourScheduler, ITransitionValidator<TState, TTrigger> transitionValidator)
        {
            _stateComparer = stateComparer;
            _triggerComparer = triggerComparer;
            _transitionEqualityComparer = new TransitionEqualityComparer<TState, TTrigger>(_stateComparer, _triggerComparer);

            _stateHierarchyBehaviourScheduler = stateHierarchyBehaviourScheduler;
            _transitionValidator = transitionValidator;
            _transitions = new HashSet<Transition<TState, TTrigger>>(_transitionEqualityComparer);
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

        public Transition<TState, TTrigger>[] GetTransitions()
        {
            return _transitions.ToArray();
        }

        public bool Trigger(TState stateFrom, TTrigger trigger)
        {
            bool hasOneValid = false;

            foreach(var transition in _transitions)
            {
                if(Matches(transition, stateFrom, trigger))
                {
                    if(_transitionValidator.IsValid(transition))
                    {
                        if (hasOneValid) 
                            throw new MultipleValidTransitionsFromSameStateException(stateFrom.ToString(), trigger.ToString());
                        else
                            hasOneValid = true;
                    }
                }
            }

            return hasOneValid;
        }

        private bool Matches(Transition<TState, TTrigger> transition, TState stateFrom, TTrigger trigger)
        {
            return _stateComparer.Equals(transition.StateFrom, stateFrom) && _triggerComparer.Equals(transition.Trigger, trigger);
        }
    }
}