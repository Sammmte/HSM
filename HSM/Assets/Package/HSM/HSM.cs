using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.FSM.HSM
{
    public class HSM<TState, TTrigger> : IHSMWithParallelStates<TState, TTrigger>
    {
        public int StateCount => throw new System.NotImplementedException();

        public int TransitionCount => throw new System.NotImplementedException();

        public bool IsStarted => throw new System.NotImplementedException();

        public TState InitialState => throw new System.NotImplementedException();

        public event StateChange<TState, TTrigger> OnBeforeStateChanges;
        public event StateChange<TState, TTrigger> OnStateChanged;

        public void AddGuardConditionTo(TState stateFrom, TTrigger trigger, TState stateTo, IGuardCondition<TState, TTrigger> guardCondition)
        {
            throw new System.NotImplementedException();
        }

        public void AddState(TState stateId, IState state)
        {
            throw new System.NotImplementedException();
        }

        public void AddTransition(TState stateFrom, TTrigger trigger, TState stateTo)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsGuardConditionOn(TState stateFrom, TTrigger trigger, TState stateTo, IGuardCondition<TState, TTrigger> guardCondition)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsState(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsSubstateRelation(TState superState, TState substate)
        {
            throw new System.NotImplementedException();
        }

        public bool ContainsTransition(TState stateFrom, TTrigger trigger, TState stateTo)
        {
            throw new System.NotImplementedException();
        }

        public void CreateParallelState(TState regionStateId, params TState[] innerStates)
        {
            throw new System.NotImplementedException();
        }

        public KeyValuePair<ITransition<TState, TTrigger>, IGuardCondition<TState, TTrigger>[]>[] GetGuardConditions()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TState>[] GetHierarchies()
        {
            throw new System.NotImplementedException();
        }

        public TState GetIdOf(IState state)
        {
            throw new System.NotImplementedException();
        }

        public TState[] GetInnerStatesOf(TState regionStateId)
        {
            throw new System.NotImplementedException();
        }

        public IState[] GetStates()
        {
            throw new System.NotImplementedException();
        }

        public ITransition<TState, TTrigger>[] GetTransitions()
        {
            throw new System.NotImplementedException();
        }

        public bool IsInState(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public bool IsParallel(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveGuardConditionFrom(TState stateFrom, TTrigger trigger, TState stateTo, IGuardCondition<TState, TTrigger> guardCondition)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveState(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveSubstateRelation(TState superState, TState substate)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveTransition(TState stateFrom, TTrigger trigger, TState stateTo)
        {
            throw new System.NotImplementedException();
        }

        public bool SendEvent(IEvent messageEvent)
        {
            throw new System.NotImplementedException();
        }

        public void SetInitialState(TState stateId)
        {
            throw new System.NotImplementedException();
        }

        public void SetSubstateRelation(TState superState, TState substate)
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public void Trigger(TTrigger trigger)
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}