using System.Collections.Generic;
using System;
using System.Linq;

namespace Paps.StateMachines
{
    public class HierarchicalStateMachine<TState, TTrigger> : IHierarchicalStateMachine<TState, TTrigger>, IStartableStateMachine<TState, TTrigger>, IUpdatableStateMachine<TState, TTrigger>
    {
        private enum InternalState
        {
            Stopped,
            Stopping,
            Idle,
            EvaluatingTransitions,
            Transitioning,
        }

        public int StateCount => _stateHierarchy.StateCount;

        public int TransitionCount => _transitionHandler.TransitionCount;

        public bool IsStarted => _internalState != InternalState.Stopped;

        public TState InitialState
        {
            get
            {
                return _stateHierarchy.InitialState;
            }

            set
            {
                ValidateContainsId(value);

                _stateHierarchy.InitialState = value;
            }
        }

        public event Action OnBeforeActiveHierarchyPathChanges
        {
            add { _stateHierarchyBehaviourScheduler.OnBeforeActiveHierarchyPathChanges += value; }
            remove { _stateHierarchyBehaviourScheduler.OnBeforeActiveHierarchyPathChanges -= value; }
        }
        public event Action OnActiveHierarchyPathChanged
        {
            add { _stateHierarchyBehaviourScheduler.OnActiveHierarchyPathChanged += value; }
            remove { _stateHierarchyBehaviourScheduler.OnActiveHierarchyPathChanged -= value; }
        }

        private Comparer<TState> _stateComparer;
        private Comparer<TTrigger> _triggerComparer;

        private StateHierarchy<TState> _stateHierarchy;
        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private TransitionValidator<TState, TTrigger> _transitionValidator;
        private TransitionHandler<TState, TTrigger> _transitionHandler;
        private HierarchicalEventDispatcher<TState> _hierarchicalEventDispatcher;

        private InternalState _internalState;
        private Transition<TState, TTrigger> _currentValidatedTransition;

        public HierarchicalStateMachine(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            if (stateComparer == null) throw new ArgumentNullException(nameof(stateComparer));
            if (triggerComparer == null) throw new ArgumentNullException(nameof(triggerComparer));

            _stateComparer = new Comparer<TState>();
            _triggerComparer = new Comparer<TTrigger>();

            SetStateComparer(stateComparer);
            SetTriggerComparer(triggerComparer);

            _stateHierarchy = new StateHierarchy<TState>(_stateComparer);
            _stateHierarchyBehaviourScheduler = new StateHierarchyBehaviourScheduler<TState>(_stateHierarchy, _stateComparer);
            _transitionValidator = new TransitionValidator<TState, TTrigger>(_stateComparer, _triggerComparer, _stateHierarchyBehaviourScheduler);
            _transitionHandler = new TransitionHandler<TState, TTrigger>(_stateComparer, _triggerComparer, _stateHierarchyBehaviourScheduler, _transitionValidator);
            _hierarchicalEventDispatcher = new HierarchicalEventDispatcher<TState>(_stateComparer, _stateHierarchyBehaviourScheduler);

            SubscribeToEventsForInternalStateChanging();
            SubscribeToEventsForSavingValidTransition();
        }

        public HierarchicalStateMachine() : this(EqualityComparer<TState>.Default, EqualityComparer<TTrigger>.Default)
        {

        }

        private void SubscribeToEventsForInternalStateChanging()
        {
            OnBeforeActiveHierarchyPathChanges += () => SetInternalState(InternalState.Transitioning);

            _stateHierarchyBehaviourScheduler.OnTransitionFinished +=
                () => SetInternalState(InternalState.EvaluatingTransitions);

            _transitionHandler.OnTransitionEvaluationBegan +=
                () => SetInternalState(InternalState.EvaluatingTransitions);
            _transitionHandler.OnTransitionEvaluationFinished +=
                () => SetInternalState(InternalState.Idle);
        }

        private void SubscribeToEventsForSavingValidTransition()
        {
            _transitionHandler.OnTransitionValidated += transition => _currentValidatedTransition = _currentValidatedTransition = transition;
            _stateHierarchyBehaviourScheduler.OnActiveHierarchyPathChanged += () => _currentValidatedTransition = default;
        }

        public void SetStateComparer(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer.EqualityComparer = stateComparer;
        }

        public void SetTriggerComparer(IEqualityComparer<TTrigger> triggerComparer)
        {
            _triggerComparer.EqualityComparer = triggerComparer;
        }

        public void AddGuardConditionTo(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            ValidateContainsTransition(transition);
            ValidateIsNotIn(InternalState.EvaluatingTransitions);

            _transitionValidator.AddGuardConditionTo(transition, guardCondition);
        }

        public void AddState(TState stateId, IState state)
        {
            _stateHierarchy.AddState(stateId, state);
        }

        public void AddTransition(Transition<TState, TTrigger> transition)
        {
            ValidateContainsId(transition.StateFrom);
            ValidateContainsId(transition.StateTo);
            ValidateIsNotIn(InternalState.EvaluatingTransitions);

            _transitionHandler.AddTransition(transition);
        }

        public bool ContainsGuardConditionOn(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            ValidateContainsTransition(transition);

            return _transitionValidator.ContainsGuardConditionOn(transition, guardCondition);
        }

        public bool ContainsState(TState stateId)
        {
            return _stateHierarchy.ContainsState(stateId);
        }

        public bool AreImmediateParentAndChild(TState superState, TState substate)
        {
            return _stateHierarchy.AreImmediateParentAndChild(superState, substate);
        }

        public bool ContainsTransition(Transition<TState, TTrigger> transition)
        {
            return _transitionHandler.ContainsTransition(transition);
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            var activeHierarchyPath = _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();

            TState[] array = new TState[activeHierarchyPath.Count];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = activeHierarchyPath[i].Key;
            }

            return array;
        }

        public IGuardCondition[] GetGuardConditionsOf(Transition<TState, TTrigger> transition)
        {
            ValidateContainsTransition(transition);

            return _transitionValidator.GetGuardConditionsOf(transition);
        }

        public IState GetStateById(TState stateId)
        {
            return _stateHierarchy.GetStateById(stateId);
        }

        public TState[] GetStates()
        {
            return _stateHierarchy.GetStates();
        }

        public Transition<TState, TTrigger>[] GetTransitions()
        {
            return _transitionHandler.GetTransitions();
        }

        public bool IsInState(TState stateId)
        {
            return _stateHierarchyBehaviourScheduler.IsInState(stateId);
        }

        public bool RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            ValidateContainsTransition(transition);

            return _transitionValidator.RemoveGuardConditionFrom(transition, guardCondition);
        }

        public bool RemoveState(TState stateId)
        {
            if(ContainsState(stateId))
            {
                ValidateIsNotIn(InternalState.EvaluatingTransitions);
                ValidateIsNotInActiveHierarchy(stateId, "Cannot remove state because it is in the active hierarchy path");
                ValidateIsNotNextStateOrInitialChildOfNextStateOnTransition(stateId);

                bool removed = _stateHierarchy.RemoveState(stateId);

                if(removed)
                    RemoveTransitionsAndGuardConditionsRelatedTo(stateId);

                return removed;
            }

            return false;
        }

        private void RemoveTransitionsAndGuardConditionsRelatedTo(TState stateId)
        {
            var removedTransitions = _transitionHandler.RemoveTransitionsRelatedTo(stateId);

            for (int i = 0; i < removedTransitions.Count; i++)
            {
                _transitionValidator.RemoveAllGuardConditionsFrom(removedTransitions[i]);
            }

            removedTransitions.Clear();
        }

        private void ValidateIsNotNextStateOrInitialChildOfNextStateOnTransition(TState stateId)
        {
            if (_internalState == InternalState.Transitioning &&
                IsInTheNextInitialActiveHierarchyPath(stateId)
                )
            {
                throw new ProtectedStateException("Cannot remove protected state " + stateId + " because it takes part on the current transition");
            }
        }

        private bool IsInTheNextInitialActiveHierarchyPath(TState stateId)
        {
            return (AreEquals(_currentValidatedTransition.StateTo, stateId) ||
                            _stateHierarchy.AreParentAndInitialChildAtAnyLevel(_currentValidatedTransition.StateTo, stateId));
        }

        private void ValidateIsNotInActiveHierarchy(TState stateId, string message)
        {
            if (IsInState(stateId)) throw new InvalidOperationException(message);
        }

        public bool RemoveChildFromParent(TState childState)
        {
            ValidateIsNotIn(InternalState.EvaluatingTransitions);
            ValidateIsNotInActiveHierarchy(childState, "Cannot remove child " + childState + 
                                                       " from its parent because it is in the active hierarchy path");

            return _stateHierarchy.RemoveChildFromParent(childState);
        }

        public bool RemoveTransition(Transition<TState, TTrigger> transition)
        {
            ValidateIsNotIn(InternalState.EvaluatingTransitions);

            bool removed = _transitionHandler.RemoveTransition(transition);

            if (removed)
                _transitionValidator.RemoveAllGuardConditionsFrom(transition);

            return removed;
        }

        public void AddChildTo(TState parentState, TState childState)
        {
            ValidateIsNotIn(InternalState.EvaluatingTransitions);
            ValidateChildIsNotActiveOnAddChild(childState);

            ValidateDoesNotWantToAddChildBeingActiveWithNoOthersChilds(parentState, childState);

            _stateHierarchy.AddChildTo(parentState, childState);
        }

        private void ValidateChildIsNotActiveOnAddChild(TState childState)
        {
            if(IsInState(childState)) throw new CannotAddChildException("Cannot set state " + childState +
                                                                        " because it is in the active hierarchy path");
        }

        private void ValidateDoesNotWantToAddChildBeingActiveWithNoOthersChilds(TState parentState, TState childState)
        {
            if (IsInState(parentState))
            {
                if (_stateHierarchy.ChildCountOf(parentState) == 0)
                {
                    throw new CannotAddChildException("Cannot set child " + childState +
                                                      " to parent " + parentState +
                                                      " because parent actually has no childs and is in the active hierarchy path." +
                                                      " If a state has childs, at least one must be active");
                }
            }
        }

        public void Start()
        {
            ValidateIsNotStarted();
            ValidateIsNotEmpty();
            
            SetInternalState(InternalState.Idle);

            _stateHierarchyBehaviourScheduler.Enter();
        }

        private void ValidateIsNotEmpty()
        {
            if (StateCount == 0) throw new EmptyStateMachineException();
        }

        private void SetInternalState(InternalState state)
        {
            _internalState = state;
        }

        public void Stop()
        {
            if(IsStarted)
            {
                SetInternalState(InternalState.Stopping);
                
                _stateHierarchyBehaviourScheduler.Exit();

                SetInternalState(InternalState.Stopped);
            }
        }

        public void Trigger(TTrigger trigger)
        {
            ValidateIsStarted();
            ValidateIsNotIn(InternalState.Stopping);
            
            _transitionHandler.Trigger(trigger);
        }

        public void Update()
        {
            ValidateIsStarted();

            _stateHierarchyBehaviourScheduler.Update();
        }

        public TState[] GetImmediateChildsOf(TState parent)
        {
            return _stateHierarchy.GetImmediateChildsOf(parent);
        }

        public TState GetParentOf(TState child)
        {
            return _stateHierarchy.GetParentOf(child);
        }

        public void SetInitialStateTo(TState parentState, TState initialState)
        {
            _stateHierarchy.SetInitialStateTo(parentState, initialState);
        }

        public TState GetInitialStateOf(TState parentState)
        {
            return _stateHierarchy.GetInitialStateOf(parentState);
        }

        public TState[] GetRoots()
        {
            return _stateHierarchy.GetRoots();
        }

        public bool SendEvent(IEvent messageEvent)
        {
            ValidateIsStarted();

            return _hierarchicalEventDispatcher.SendEvent(messageEvent);
        }

        public void SubscribeEventHandlerTo(TState stateId, IStateEventHandler eventHandler)
        {
            _hierarchicalEventDispatcher.AddEventHandlerTo(stateId, eventHandler);
        }

        public bool UnsubscribeEventHandlerFrom(TState stateId, IStateEventHandler eventHandler)
        {
            return _hierarchicalEventDispatcher.RemoveEventHandlerFrom(stateId, eventHandler);
        }

        public bool HasEventHandlerOn(TState stateId, IStateEventHandler eventHandler)
        {
            return _hierarchicalEventDispatcher.HasEventHandlerOn(stateId, eventHandler);
        }

        public IStateEventHandler[] GetEventHandlersOf(TState stateId)
        {
            return _hierarchicalEventDispatcher.GetEventHandlersOf(stateId);
        }

        private void ValidateContainsId(TState stateId)
        {
            if (ContainsState(stateId) == false) throw new StateIdNotAddedException(stateId.ToString());
        }

        private void ValidateContainsTransition(Transition<TState, TTrigger> transition)
        {
            if (ContainsTransition(transition) == false)
                throw new TransitionNotAddedException(transition.StateFrom.ToString(),
                    transition.Trigger.ToString(),
                    transition.StateTo.ToString());
        }

        private void ValidateIsStarted()
        {
            ValidateIsNotIn(InternalState.Stopped);
        }

        private void ValidateIsNotStarted()
        {
            ValidateIsIn(InternalState.Stopped);
        }

        private void ValidateIsIn(InternalState state)
        {
            if (_internalState != state) ThrowByInternalState();
        }

        private void ValidateIsNotIn(InternalState internalState)
        {
            if (_internalState == internalState) ThrowByInternalState();
        }

        private void ThrowByInternalState()
        {
            switch (_internalState)
            {
                case InternalState.Stopped:
                    throw new StateMachineNotStartedException();
                case InternalState.Stopping:
                    throw new StateMachineStoppingException();
                case InternalState.Transitioning:
                    throw new StateMachineTransitioningException();
                case InternalState.EvaluatingTransitions:
                    throw new StateMachineEvaluatingTransitionsException();
                case InternalState.Idle:
                    throw new StateMachineStartedException();
            }
        }

        private bool AreEquals(TState stateId1, TState stateId2)
        {
            return _stateComparer.Equals(stateId1, stateId2);
        }

        private class Comparer<T> : IEqualityComparer<T>
        {
            public IEqualityComparer<T> EqualityComparer;

            public bool Equals(T x, T y)
            {
                return EqualityComparer.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return EqualityComparer.GetHashCode(obj);
            }
        }
    }
}
