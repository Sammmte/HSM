using System;
using System.Collections.Generic;

namespace Paps.StateMachines
{
    public interface IHierarchicalStateMachine<TState, TTrigger> : IStateMachine<TState, TTrigger>, IGuardedStateMachine<TState, TTrigger>,
        IEventDispatcherStateMachine<TState, TTrigger>
    {
        event HierarchyPathChanged<TTrigger> OnBeforeActiveHierarchyPathChanges;
        event HierarchyPathChanged<TTrigger> OnActiveHierarchyPathChanged;

        void AddChildTo(TState parentState, TState childState);
        bool RemoveChildFromParent(TState childState);

        bool AreImmediateParentAndChild(TState parentState, TState substate);

        IEnumerable<TState> GetActiveHierarchyPath();

        TState[] GetImmediateChildsOf(TState parent);

        TState GetParentOf(TState child);

        void SetInitialStateTo(TState parentState, TState initialState);
        TState GetInitialStateOf(TState parentState);

        TState[] GetRoots();
    }
}