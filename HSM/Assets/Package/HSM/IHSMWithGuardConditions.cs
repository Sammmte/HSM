using System.Collections.Generic;

namespace Paps.FSM.HSM
{
    public interface IHSMWithGuardConditions<TState, TTrigger> : IHSM<TState, TTrigger>
    {
        void AddGuardConditionTo(Transition<TState, TTrigger> transition, IGuardCondition guardCondition);
        bool ContainsGuardConditionOn(Transition<TState, TTrigger> transition, IGuardCondition guardCondition);
        KeyValuePair<Transition<TState, TTrigger>, IGuardCondition[]>[] GetGuardConditions();
        void RemoveGuardConditionFrom(Transition<TState, TTrigger> transition, IGuardCondition guardCondition);
    }
}