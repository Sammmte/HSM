using System.Collections.Generic;
using System;

namespace Paps.StateMachines
{
    public class HierarchicalStateMachine<TState, TTrigger> : IHierarchicalStateMachine<TState, TTrigger>, IStartableStateMachine<TState, TTrigger>, IUpdatableStateMachine<TState, TTrigger>
    {
        public int StateCount => _stateHierarchy.StateCount;

        public int TransitionCount => throw new System.NotImplementedException();

        public bool IsStarted { get; private set; }

        public TState InitialState
        {
            get
            {
                return _stateHierarchy.InitialState;
            }

            set
            {
                _stateHierarchy.InitialState = value;
            }
        }

        public event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        public event HierarchyChanged<TState> OnHierarchyChanged;

        private Comparer<TState> _stateComparer;
        private Comparer<TTrigger> _triggerComparer;

        private StateHierarchy<TState> _stateHierarchy;
        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private TransitionManager<TState, TTrigger> _transitionManager;

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
            _transitionManager = new TransitionManager<TState, TTrigger>(_stateHierarchy, _stateComparer, _triggerComparer);
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
            throw new System.NotImplementedException();
        }

        public void AddState(TState stateId, IState state)
        {
            _stateHierarchy.AddState(stateId, state);
        }

        public void AddTransition(Transition<TState, TTrigger> transition)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsGuardConditionOn(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            return _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();
        }

        public IGuardCondition[] GetGuardConditionsOf(Transition<TState, TTrigger> transition)
        {
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public bool IsInState(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveState(TState stateId)
        {
            return _stateHierarchy.RemoveState(stateId);
        }

        public void BreakSubstateRelation(TState superState, TState substate)
        {
            _stateHierarchy.BreakSubstateRelation(superState, substate);
        }

        public bool RemoveTransition(Transition<TState, TTrigger> transition)
        {
            throw new System.NotImplementedException();
        }

        public bool SendEvent(IEvent messageEvent)
        {
            throw new System.NotImplementedException();
        }

        public void EstablishSubstateRelation(TState superState, TState substate)
        {
            _stateHierarchy.EstablishSubstateRelation(superState, substate);
        }

        public void Start()
        {
            try
            {
                IsStarted = true;

                _stateHierarchyBehaviourScheduler.Enter();
            }
            catch(InvalidInitialStateException)
            {
                IsStarted = false;

                throw;
            }
        }

        public void Stop()
        {
            _stateHierarchyBehaviourScheduler.Exit();

            IsStarted = false;
        }

        public void Trigger(TTrigger trigger)
        {
            throw new System.NotImplementedException();
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
