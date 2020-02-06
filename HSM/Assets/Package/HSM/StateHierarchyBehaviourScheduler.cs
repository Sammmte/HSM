using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paps.FSM.HSM
{
    internal class StateHierarchyBehaviourScheduler<TState>
    {
        private readonly StateHierarchy<TState> _stateHierarchy;

        private List<KeyValuePair<TState, IState>> _activeHierarchyPath;

        public StateHierarchyBehaviourScheduler(StateHierarchy<TState> stateHierarchy)
        {
            _stateHierarchy = stateHierarchy;

            _activeHierarchyPath = new List<KeyValuePair<TState, IState>>();
        }

        public void StartFromInitialStates()
        {
            ValidateInitialStatesStartingFrom(_stateHierarchy.InitialState);
        }

        private void ValidateInitialStatesStartingFrom(TState stateId)
        {
            var current = stateId;

            while(true)
            {
                if (_stateHierarchy.ContainsState(current))
                {
                    if (_stateHierarchy.HasChilds(current))
                    {
                        var initialStateOfCurrent = _stateHierarchy.GetInitialStateOf(current);

                        if (_stateHierarchy.AreImmediateParentAndChild(current, initialStateOfCurrent))
                        {
                            current = initialStateOfCurrent;
                        }
                        else throw new InvalidInitialStateException(
                            "Invalid initial state. Parent with id " + current + " has not a child state with id " + initialStateOfCurrent);
                    }
                    else return;
                }
                else throw new InvalidInitialStateException(
                            "Invalid initial state. Parent with id " + current + " was not added to state machine");
            }
        }
    }
}