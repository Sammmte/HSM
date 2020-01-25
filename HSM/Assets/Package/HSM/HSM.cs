using System.Collections.Generic;
using System;

namespace Paps.FSM.HSM
{
    public class HSM<TState, TTrigger> : IHierarchicalFSM<TState, TTrigger>, IFSMStartable<TState, TTrigger>, IFSMUpdatable<TState, TTrigger>
    {
        public int StateCount => _stateHierarchy.StateCount;

        public int TransitionCount => throw new System.NotImplementedException();

        public bool IsStarted => _stateHierarchy.IsStarted;

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
        private TransitionManager<TState, TTrigger> _transitionManager;

        public HSM(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            if (stateComparer == null) throw new ArgumentNullException(nameof(stateComparer));
            if (triggerComparer == null) throw new ArgumentNullException(nameof(triggerComparer));

            _stateComparer = new Comparer<TState>();
            _triggerComparer = new Comparer<TTrigger>();

            SetStateComparer(stateComparer);
            SetTriggerComparer(triggerComparer);

            _stateHierarchy = new StateHierarchy<TState>(_stateComparer);
            _transitionManager =
                new TransitionManager<TState, TTrigger>(_stateHierarchy, _stateComparer, _triggerComparer);
        }

        public HSM() : this(EqualityComparer<TState>.Default, EqualityComparer<TTrigger>.Default)
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

        public bool AreRelatives(TState superState, TState substate)
        {
            return _stateHierarchy.AreRelatives(superState, substate);
        }

        public bool ContainsTransition(Transition<TState, TTrigger> transition)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            return _stateHierarchy.GetActiveHierarchyPath();
        }

        public KeyValuePair<Transition<TState, TTrigger>, IGuardCondition[]>[] GetGuardConditions()
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
            return _stateHierarchy.IsInState(stateId);
        }

        public void RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveState(TState stateId)
        {
            _stateHierarchy.RemoveState(stateId);
        }

        public void RemoveImmediateSubstateRelation(TState superState, TState substate)
        {
            _stateHierarchy.RemoveSubstateRelation(superState, substate);
        }

        public void RemoveTransition(Transition<TState, TTrigger> transition)
        {
            throw new System.NotImplementedException();
        }

        public bool SendEvent(IEvent messageEvent)
        {
            throw new System.NotImplementedException();
        }

        public void SetImmediateSubstateRelation(TState superState, TState substate)
        {
            _stateHierarchy.SetSubstateRelation(superState, substate);
        }

        public void Start()
        {
            ValidateIsNotStarted();

            _stateHierarchy.Start();
            
        }

        private void ValidateIsNotStarted()
        {
            if (IsStarted) throw new StateMachineStartedException();
        }

        private void ValidateIsStarted()
        {
            if (IsStarted == false) throw new StateMachineNotStartedException();
        }

        public void Stop()
        {
            if(IsStarted)
            {
                _stateHierarchy.Stop();
            }
        }

        public void Trigger(TTrigger trigger)
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            ValidateIsStarted();

            _stateHierarchy.Update();
        }

        public TState[] GetChildsOf(TState parent)
        {
            return _stateHierarchy.GetChildsOf(parent);
        }

        public TState GetParentOf(TState child)
        {
            return _stateHierarchy.GetParentOf(child);
        }

        public void SetInitialStateTo(TState parentState, TState initialState)
        {
            _stateHierarchy.SetInitialStateTo(parentState, initialState);
        }

        public TState[] GetRoots()
        {
            return _stateHierarchy.GetRoots();
        }

        public void SubscribeEventHandlerTo(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeEventHandlerFrom(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool HasEventHandler(TState stateId, IStateEventHandler eventHandler)
        {
            throw new NotImplementedException();
        }

        public bool HasEventListener(TState stateId)
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
