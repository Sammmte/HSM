using System.Collections;
using System.Collections.Generic;

namespace Paps.StateMachines.Extensions
{
    public static class HierarchicalStateMachineExtensions
    {
        public static bool IsRoot<TState, TTrigger>(this IHierarchicalStateMachine<TState, TTrigger> hsm, TState stateId)
        {
            if (hsm.GetParentOf(stateId).Equals(stateId))
                return true;

            return false;
        }

        public static void ForeachInActiveHierarchyPath<TState, TTrigger>(this IHierarchicalStateMachine<TState, TTrigger> hsm, ReturnTrueToFinishIteration<TState> finishable)
        {
            var activeHierarchyPath = hsm.GetActiveHierarchyPath();

            foreach(TState state in activeHierarchyPath)
            {
                if(finishable(state))
                {
                    return;
                }
            }
        }
    }
}