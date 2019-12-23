namespace Paps.FSM.HSM
{
    public interface IHSMWithParallelStates<TState, TTrigger> : IHSM<TState, TTrigger>
    {
        void CreateParallelState(TState regionStateId, params TState[] innerStates);

        TState[] GetInnerStatesOf(TState regionStateId);

        bool IsParallel(TState stateId);
    }
}