using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public interface IHSM<TState, TTrigger>
    {
        int StateCount { get; }
        int TransitionCount { get; }

        event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        event HierarchyChanged<TState> OnHierarchyChanged;

        bool IsStarted { get; }

        TState InitialState { get; }

        void AddState(TState stateId, IState state);
        void RemoveState(TState stateId);

        bool ContainsState(TState stateId);

        TState[] GetStates();

        void AddTransition(Transition<TState, TTrigger> transition);
        void RemoveTransition(Transition<TState, TTrigger> transition);

        bool ContainsTransition(Transition<TState, TTrigger> transition);

        Transition<TState, TTrigger>[] GetTransitions();

        void SetInitialState(TState stateId);

        bool IsInState(TState stateId);

        IState GetStateById(TState stateId);

        void Start();
        void Update();
        void Stop();

        void Trigger(TTrigger trigger);

        bool SendEvent(IEvent messageEvent);

        void SetSubstateRelation(TState superState, TState substate);
        void RemoveSubstateRelation(TState superState, TState substate);

        bool ContainsSubstateRelation(TState superState, TState substate);

        IEnumerable<TState> GetActiveHierarchyPath();

        TState[] GetChildsOf(TState parent);

        TState GetParentOf(TState child);
    }
}