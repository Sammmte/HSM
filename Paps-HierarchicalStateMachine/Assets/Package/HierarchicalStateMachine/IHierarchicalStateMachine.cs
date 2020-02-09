using System.Collections.Generic;

namespace Paps.StateMachines
{
    public interface IHierarchicalStateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>, IGuardedStateMachine<TState, TTrigger>,
        IEventDispatcherStateMachine<TState, TTrigger>
    {
        event HierarchyChanged<TState> OnBeforeHierarchyChanges;
        event HierarchyChanged<TState> OnHierarchyChanged;

        void AddChildTo(TState parentState, TState substate);
        bool RemoveChildFrom(TState parentState, TState substate);

        bool AreImmediateParentAndChild(TState parentState, TState substate);

        IEnumerable<TState> GetActiveHierarchyPath();

        TState[] GetImmediateChildsOf(TState parent);

        TState GetParentOf(TState child);

        void SetInitialStateTo(TState parentState, TState initialState);
        TState GetInitialStateOf(TState parentState);

        TState[] GetRoots();
    }
}