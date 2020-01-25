using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public interface IHierarchicalFSM<TState, TTrigger> : IFSM<TState, TTrigger>, IFSMWithGuardConditions<TState, TTrigger>,
        IFSMEventDispatcher<TState, TTrigger>
    {
        event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        event HierarchyChanged<TState> OnHierarchyChanged;

        void SetImmediateSubstateRelation(TState parentState, TState substate);
        void RemoveImmediateSubstateRelation(TState parentState, TState substate);

        bool AreRelatives(TState parentState, TState substate);

        IEnumerable<TState> GetActiveHierarchyPath();

        TState[] GetChildsOf(TState parent);

        TState GetParentOf(TState child);

        void SetInitialStateTo(TState parentState, TState initialState);

        TState[] GetRoots();
    }
}