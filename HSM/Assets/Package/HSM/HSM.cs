using System.Collections.Generic;
using System;

namespace Paps.FSM.HSM
{
    public class HSM<TState, TTrigger> : IHSM<TState, TTrigger>, IHSMWithGuardConditions<TState, TTrigger>
    {
        public int StateCount => _stateHierarchy.StateCount;

        public int TransitionCount => throw new System.NotImplementedException();

        public bool IsStarted { get; private set; }

        public TState InitialState => _stateHierarchy.InitialState;

        public event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        public event HierarchyChanged<TState> OnHierarchyChanged;

        private Comparer<TState> _stateComparer;
        private Comparer<TTrigger> _triggerComparer;

        private StateHierarchy<TState> _stateHierarchy;

        public HSM(IEqualityComparer<TState> stateComparer, IEqualityComparer<TTrigger> triggerComparer)
        {
            if (stateComparer == null) throw new ArgumentNullException(nameof(stateComparer));
            if (triggerComparer == null) throw new ArgumentNullException(nameof(triggerComparer));

            _stateComparer = new Comparer<TState>();
            _triggerComparer = new Comparer<TTrigger>();

            SetStateComparer(stateComparer);
            SetTriggerComparer(triggerComparer);

            _stateHierarchy = new StateHierarchy<TState>(_stateComparer);
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

        public bool ContainsSubstateRelation(TState superState, TState substate)
        {
            return _stateHierarchy.ContainsSubstateRelation(superState, substate);
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

        public void RemoveSubstateRelation(TState superState, TState substate)
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

        public void SetInitialState(TState stateId)
        {
            _stateHierarchy.InitialState = stateId;
        }

        public void SetSubstateRelation(TState superState, TState substate)
        {
            _stateHierarchy.SetSubstateRelation(superState, substate);
        }

        public void Start()
        {
            ValidateIsNotStarted();

            IsStarted = true;

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
                IsStarted = false;

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
