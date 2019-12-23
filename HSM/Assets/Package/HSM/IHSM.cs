using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public interface IHSM<TState, TTrigger> : IFSMWithGuardConditions<TState, TTrigger>
    {
        void SetSubstateRelation(TState superState, TState substate);
        void RemoveSubstateRelation(TState superState, TState substate);

        bool ContainsSubstateRelation(TState superState, TState substate);

        IEnumerable<TState>[] GetHierarchies();
    }
}