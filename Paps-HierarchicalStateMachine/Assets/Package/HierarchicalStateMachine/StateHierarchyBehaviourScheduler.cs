using System.Collections.Generic;

namespace Paps.StateMachines
{
    internal class StateHierarchyBehaviourScheduler<TState>
    {
        private readonly StateHierarchy<TState> _stateHierarchy;

        private List<KeyValuePair<TState, IState>> _activeHierarchyPath;

        private IEqualityComparer<TState> _stateComparer;

        public StateHierarchyBehaviourScheduler(StateHierarchy<TState> stateHierarchy, IEqualityComparer<TState> stateComparer)
        {
            _stateHierarchy = stateHierarchy;
            _stateComparer = stateComparer;

            _activeHierarchyPath = new List<KeyValuePair<TState, IState>>();
        }

        public void Enter()
        {
            ValidateInitialStatesStartingFrom(_stateHierarchy.InitialState);

            AddFrom(_stateHierarchy.InitialState);

            ExecuteEnterEventsFrom(_stateHierarchy.InitialState);
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

        private void AddFrom(TState stateId)
        {
            var previous = stateId;
            var current = _stateHierarchy.GetInitialStateOf(previous);

            _activeHierarchyPath.Add(NewKeyValueFor(previous));

            while (_stateHierarchy.AreImmediateParentAndChild(previous, current))
            {
                _activeHierarchyPath.Add(NewKeyValueFor(current));

                previous = current;
                current = _stateHierarchy.GetInitialStateOf(current);
            }
        }

        private void ExecuteEnterEventsFrom(TState stateId)
        {
            for(int i = 0; i < _activeHierarchyPath.Count; i++)
            {
                if(AreEquals(_activeHierarchyPath[i].Key, stateId))
                {
                    for(int j = i; j < _activeHierarchyPath.Count; j++)
                    {
                        _activeHierarchyPath[j].Value.Enter();
                    }

                    return;
                }
            }
        }

        public void Exit()
        {
            ExitUntil(_activeHierarchyPath[0].Key);

            _activeHierarchyPath.Clear();
        }

        private void ExitUntil(TState stateId)
        {
            for(int i = _activeHierarchyPath.Count - 1; i >= 0; i--)
            {
                _activeHierarchyPath[i].Value.Exit();

                if (AreEquals(_activeHierarchyPath[i].Key, stateId)) return;
            }
        }

        public void Update()
        {
            for(int i = 0; i < _activeHierarchyPath.Count; i++)
            {
                _activeHierarchyPath[i].Value.Update();
            }
        }

        private KeyValuePair<TState, IState> NewKeyValueFor(TState stateId)
        {
            return new KeyValuePair<TState, IState>(stateId, _stateHierarchy.GetStateById(stateId));
        }

        private bool AreEquals(TState stateId1, TState stateId2)
        {
            return _stateComparer.Equals(stateId1, stateId2);
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            TState[] array = new TState[_activeHierarchyPath.Count];

            for(int i = 0; i < array.Length; i++)
            {
                array[i] = _activeHierarchyPath[i].Key;
            }

            return array;
        }
    }
}