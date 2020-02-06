using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public interface IHierarchicalFSM<TState, TTrigger> : IFSM<TState, TTrigger>, IFSMWithGuardConditions<TState, TTrigger>,
        IFSMEventDispatcher<TState, TTrigger>
    {
        event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        event HierarchyChanged<TState> OnHierarchyChanged;

        void EstablishSubstateRelation(TState parentState, TState substate);
        void BreakSubstateRelation(TState parentState, TState substate);

        bool AreImmediateParentAndChild(TState parentState, TState substate);

        IEnumerable<TState> GetActiveHierarchyPath();

        TState[] GetImmediateChildsOf(TState parent);

        TState GetParentOf(TState child);

        void SetInitialStateTo(TState parentState, TState initialState);
        TState GetInitialStateOf(TState parentState);

        TState[] GetRoots();
    }
}