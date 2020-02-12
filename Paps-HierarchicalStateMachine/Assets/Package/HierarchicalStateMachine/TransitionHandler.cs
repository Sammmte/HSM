using System;
using System.Collections.Generic;
using System.Linq;

namespace Paps.StateMachines
{
    internal class TransitionHandler<TState, TTrigger>
    {
        public int TransitionCount => _transitions.Count;

        private IEqualityComparer<TState> _stateComparer;
        private IEqualityComparer<TTrigger> _triggerComparer;
        private TransitionEqualityComparer<TState, TTrigger> _transitionEqualityComparer;

        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private ITransitionValidator<TState, TTrigger> _transitionValidator;

        private HashSet<Transition<TState, TTrigger>> _transitions;
        private Queue<TTrigger> _pendingTriggers;

        public event Action OnTransitionEvaluationBegan;
        public event Action OnTransitionEvaluationFinished;
        public event Action<Transition<TState, TTrigger>> OnTransitionValidated;

        public bool IsEvaluatingTransitions { get; private set; }

        public TransitionHandler(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer, 
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

        public void RemoveTransitionsRelatedTo(TState stateId)
        {
            List<Transition<TState, TTrigger>> toRemove = new List<Transition<TState, TTrigger>>();
            
            foreach (var transition in _transitions)
            {
                if(AreEquals(transition.StateFrom, stateId) || AreEquals(transition.StateTo, stateId))
                    toRemove.Add(transition);
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                _transitions.Remove(toRemove[i]);
            }
            
            toRemove.Clear();
        }

        public bool ContainsTransition(Transition<TState, TTrigger> transition)
        {
            return _transitions.Contains(transition);
        }

        public Transition<TState, TTrigger>[] GetTransitions()
        {
            if (_transitions.Count > 0)
                return _transitions.ToArray();
            else
                return null;
        }

        public void Trigger(TTrigger trigger)
        {
            _pendingTriggers.Enqueue(trigger);

            if (IsEvaluatingTransitions == false)
            {
                IsEvaluatingTransitions = true;
                
                OnTransitionEvaluationBegan?.Invoke();
                
                ProcessPendingTriggers();

                IsEvaluatingTransitions = false;
                
                OnTransitionEvaluationFinished?.Invoke();
            }
        }

        private void ProcessPendingTriggers()
        {
            while (_pendingTriggers.Count > 0)
            {
                var activeHierarchyPath = _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();
                var trigger = _pendingTriggers.Dequeue();
                Transition<TState, TTrigger> validTransition = default;
            
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
                                    validTransition = transition;
                                }
                                    
                            }
                        }
                    }
                }
                
                if(hasOneValid)
                {
                    OnTransitionValidated?.Invoke(validTransition);
                    _stateHierarchyBehaviourScheduler.SwitchTo(validTransition.StateTo);
                }
                    
            }
        }

        private bool Matches(Transition<TState, TTrigger> transition, TState stateFrom, TTrigger trigger)
        {
            return _stateComparer.Equals(transition.StateFrom, stateFrom) && _triggerComparer.Equals(transition.Trigger, trigger);
        }

        private bool AreEquals(TState stateId1, TState stateId2)
        {
            return _stateComparer.Equals(stateId1, stateId2);
        }
    }
}