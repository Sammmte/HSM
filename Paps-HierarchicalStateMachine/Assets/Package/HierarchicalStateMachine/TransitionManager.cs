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
        private Queue<TTrigger> _pendingTriggers;

        private bool _isEvaluatingTransitions;

        public TransitionManager(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer, 
            StateHierarchyBehaviourScheduler<TState> stateHierarchyBehaviourScheduler, ITransitionValidator<TState, TTrigger> transitionValidator)
        {
            _stateComparer = stateComparer;
            _triggerComparer = triggerComparer;
            _transitionEqualityComparer = new TransitionEqualityComparer<TState, TTrigger>(_stateComparer, _triggerComparer);

            _stateHierarchyBehaviourScheduler = stateHierarchyBehaviourScheduler;
            _transitionValidator = transitionValidator;
            _transitions = new HashSet<Transition<TState, TTrigger>>(_transitionEqualityComparer);
            _pendingTriggers = new Queue<TTrigger>();
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

        public void Trigger(TTrigger trigger)
        {
            _pendingTriggers.Enqueue(trigger);

            if (_isEvaluatingTransitions == false)
            {
                _isEvaluatingTransitions = true;
                
                ProcessPendingTriggers();

                _isEvaluatingTransitions = false;
            }
        }

        private void ProcessPendingTriggers()
        {
            while (_pendingTriggers.Count > 0)
            {
                var activeHierarchyPath = _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();
                var trigger = _pendingTriggers.Dequeue();
                TState finalStateTo = default;
            
                bool hasOneValid = false;

                for (int i = activeHierarchyPath.Count - 1; i >= 0; i--)
                {
                    var stateFrom = activeHierarchyPath[i].Key;
                
                    foreach(var transition in _transitions)
                    {
                        if(Matches(transition, stateFrom, trigger))
                        {
                            if(_transitionValidator.IsValid(transition))
                            {
                                if (hasOneValid)
                                {
                                    _pendingTriggers.Clear();
                                    throw new MultipleValidTransitionsFromSameStateException(stateFrom.ToString(), trigger.ToString());
                                }
                                else
                                {
                                    hasOneValid = true;
                                    finalStateTo = transition.StateTo;
                                }
                                    
                            }
                        }
                    }
                }
                
                if(hasOneValid)
                    _stateHierarchyBehaviourScheduler.SwitchTo(finalStateTo);
            }
        }

        private bool Matches(Transition<TState, TTrigger> transition, TState stateFrom, TTrigger trigger)
        {
            return _stateComparer.Equals(transition.StateFrom, stateFrom) && _triggerComparer.Equals(transition.Trigger, trigger);
        }
    }
}