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

        public int TransitionCount => _transitionManager.TransitionCount;

        public bool IsStarted { get; private set; }

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

        public event ActiveHierarchyPathChanged OnBeforeHierarchyChanges
        {
            add { _stateHierarchyBehaviourScheduler.OnBeforeActiveHierarchyPathChanges += value; }
            remove { _stateHierarchyBehaviourScheduler.OnBeforeActiveHierarchyPathChanges -= value; }
        }
        public event ActiveHierarchyPathChanged OnHierarchyChanged
        {
            add { _stateHierarchyBehaviourScheduler.OnActiveHierarchyPathChanged += value; }
            remove { _stateHierarchyBehaviourScheduler.OnActiveHierarchyPathChanged -= value; }
        }

        private Comparer<TState> _stateComparer;
        private Comparer<TTrigger> _triggerComparer;
        private TransitionEqualityComparer<TState, TTrigger> _transitionEqualityComparer;

        private StateHierarchy<TState> _stateHierarchy;
        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private TransitionValidator<TState, TTrigger> _transitionValidator;
        private TransitionManager<TState, TTrigger> _transitionManager;

        private InternalState _internalState;

        public HierarchicalStateMachine(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            if (stateComparer == null) throw new ArgumentNullException(nameof(stateComparer));
            if (triggerComparer == null) throw new ArgumentNullException(nameof(triggerComparer));

            _stateComparer = new Comparer<TState>();
            _triggerComparer = new Comparer<TTrigger>();
            _transitionEqualityComparer = new TransitionEqualityComparer<TState, TTrigger>(_stateComparer, _triggerComparer);

            SetStateComparer(stateComparer);
            SetTriggerComparer(triggerComparer);

            _stateHierarchy = new StateHierarchy<TState>(_stateComparer);
            _stateHierarchyBehaviourScheduler = new StateHierarchyBehaviourScheduler<TState>(_stateHierarchy, _stateComparer);
            _transitionValidator = new TransitionValidator<TState, TTrigger>(_stateComparer, _triggerComparer, _stateHierarchyBehaviourScheduler);
            _transitionManager = new TransitionManager<TState, TTrigger>(_stateComparer, _triggerComparer, _stateHierarchyBehaviourScheduler, _transitionValidator);
        }

        public HierarchicalStateMachine() : this(EqualityComparer<TState>.Default, EqualityComparer<TTrigger>.Default)
        {

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

            _transitionManager.AddTransition(transition);
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
            return _transitionManager.ContainsTransition(transition);
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
            return _transitionManager.GetTransitions();
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
            return _stateHierarchy.RemoveState(stateId);
        }

        public bool RemoveChildFrom(TState superState, TState substate)
        {
            return _stateHierarchy.RemoveChildFrom(superState, substate);
        }

        public bool RemoveTransition(Transition<TState, TTrigger> transition)
        {
            return _transitionManager.RemoveTransition(transition);
        }

        public bool SendEvent(IEvent messageEvent)
        {
            throw new System.NotImplementedException();
        }

        public void SetChildTo(TState superState, TState substate)
        {
            _stateHierarchy.AddChildTo(superState, substate);
        }

        public void Start()
        {
            ValidateIsStarted();
            ValidateIsNotEmpty();

            IsStarted = true;

            _stateHierarchyBehaviourScheduler.Enter();
        }

        private void ValidateIsNotEmpty()
        {
            if (StateCount == 0) throw new EmptyStateMachineException();
        }

        private void ValidateIsStarted()
        {
            ValidateIsNotIn(InternalState.Stopped);
        }

        private void ValidateIsNotIn(InternalState internalState)
        {
            if(_internalState == internalState) ThrowByInternalState();
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

        public void Stop()
        {
            if(IsStarted)
            {
                _stateHierarchyBehaviourScheduler.Exit();

                IsStarted = false;
            }
        }

        public void Trigger(TTrigger trigger)
        {
            ValidateIsStarted();
            ValidateIsNotIn(InternalState.Stopping);

            _transitionManager.Trigger(trigger);
        }

        public void Update()
        {
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

        public void SubscribeEventHandlerTo(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeEventHandlerFrom(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool HasEventHandlerOn(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool HasAnyEventHandlerOn(TState stateId)
        {
            throw new NotImplementedException();
        }

        public IStateEventHandler[] GetEventHandlersOf(TState stateId)
        {
            throw new NotImplementedException();
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
