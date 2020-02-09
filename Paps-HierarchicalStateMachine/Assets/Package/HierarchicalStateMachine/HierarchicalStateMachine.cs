﻿using System.Collections.Generic;
using System;
using System.Linq;

namespace Paps.StateMachines
{
    public class HierarchicalStateMachine<TState, TTrigger> : IHierarchicalStateMachine<TState, TTrigger>, IStartableStateMachine<TState, TTrigger>, IUpdatableStateMachine<TState, TTrigger>
    {
        public int StateCount => _stateHierarchy.StateCount;

        public int TransitionCount => _transitions.Count;

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

        public event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        public event HierarchyChanged<TState> OnHierarchyChanged;

        private Comparer<TState> _stateComparer;
        private Comparer<TTrigger> _triggerComparer;
        private TransitionEqualityComparer<TState, TTrigger> _transitionEqualityComparer;

        private StateHierarchy<TState> _stateHierarchy;
        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;
        private HashSet<Transition<TState, TTrigger>> _transitions;
        private TransitionValidator<TState, TTrigger> _transitionValidator;

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
            _transitions = new HashSet<Transition<TState, TTrigger>>(_transitionEqualityComparer);
            _transitionValidator = new TransitionValidator<TState, TTrigger>(_stateComparer, _triggerComparer);
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

            _transitions.Add(transition);
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
            return _transitions.Contains(transition);
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            return _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();
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
            return _transitions.ToArray();
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
            return _transitions.Remove(transition);
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
            if (IsStarted) throw new StateMachineStartedException();
            if (StateCount == 0) throw new EmptyStateMachineException();

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
            if(IsStarted)
            {
                _stateHierarchyBehaviourScheduler.Exit();

                IsStarted = false;
            }
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
