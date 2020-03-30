using System.Collections.Generic;
using System;

namespace Paps.StateMachines
{
    internal class StateHierarchyBehaviourScheduler<TState>
    {
        private readonly StateHierarchy<TState> _stateHierarchy;

        private List<KeyValuePair<TState, IState>> _activeHierarchyPath;

        private IEqualityComparer<TState> _stateComparer;

        public event Action OnBeforeActiveHierarchyPathChanges;
        public event Action OnActiveHierarchyPathChanged;

        public event Action OnTransitionFinished;

        private bool isUpdating;
        private bool hasSwitchedOnUpdate;

        public StateHierarchyBehaviourScheduler(StateHierarchy<TState> stateHierarchy, IEqualityComparer<TState> stateComparer)
        {
            _stateHierarchy = stateHierarchy;
            _stateComparer = stateComparer;

            _activeHierarchyPath = new List<KeyValuePair<TState, IState>>();

            OnTransitionFinished += CatchSwitchOnUpdate;
        }

        public void Enter()
        {
            AddToActivesFrom(_stateHierarchy.InitialState);

            EnterActivesFrom(_stateHierarchy.InitialState);
        }

        private void AddToActivesFrom(TState stateId)
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

        private void EnterActivesFrom(TState stateId)
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
            ExitActivesUntil(_activeHierarchyPath[0].Key);

            _activeHierarchyPath.Clear();
        }

        private void RemoveFromActivesUntil(TState stateId)
        {
            for(int i = _activeHierarchyPath.Count - 1; i >= 0; i--)
            {
                var current = _activeHierarchyPath[i];

                _activeHierarchyPath.RemoveAt(i);

                if (AreEquals(current.Key, stateId)) return;
            }
        }

        private void ExitActivesUntil(TState stateId)
        {
            for(int i = _activeHierarchyPath.Count - 1; i >= 0; i--)
            {
                _activeHierarchyPath[i].Value.Exit();

                if (AreEquals(_activeHierarchyPath[i].Key, stateId)) return;
            }
        }

        public void Update()
        {
            isUpdating = true;
            
            for(int i = 0; i < _activeHierarchyPath.Count && !hasSwitchedOnUpdate; i++)
            {
                _activeHierarchyPath[i].Value.Update();
            }

            hasSwitchedOnUpdate = false;
            isUpdating = false;
        }

        private void CatchSwitchOnUpdate()
        {
            if (isUpdating)
                hasSwitchedOnUpdate = true;
        }

        private KeyValuePair<TState, IState> NewKeyValueFor(TState stateId)
        {
            return new KeyValuePair<TState, IState>(stateId, _stateHierarchy.GetStateById(stateId));
        }

        private bool AreEquals(TState stateId1, TState stateId2)
        {
            return _stateComparer.Equals(stateId1, stateId2);
        }

        public List<KeyValuePair<TState, IState>> GetActiveHierarchyPath()
        {
            return _activeHierarchyPath;
        }

        public bool IsInState(TState stateId)
        {
            for(int i = 0; i < _activeHierarchyPath.Count; i++)
            {
                if (AreEquals(_activeHierarchyPath[i].Key, stateId))
                    return true;
            }

            return false;
        }

        public bool IsValidSwitchTo(TState stateId, out TState activeSibling)
        {
            for(int i = 0; i < _activeHierarchyPath.Count; i++)
            {
                if (AreEquals(_activeHierarchyPath[i].Key, stateId) ||
                    _stateHierarchy.AreSiblings(_activeHierarchyPath[i].Key, stateId))
                {
                    activeSibling = _activeHierarchyPath[i].Key;
                    return true;
                } 
            }

            activeSibling = default;
            return false;
        }

        public void SwitchTo(TState newActiveState)
        {
            if (IsValidSwitchTo(newActiveState, out TState activeSibling) == false)
                throw new InvalidOperationException("Cannot switch to " + newActiveState);

            OnBeforeActiveHierarchyPathChanges?.Invoke();

            ExitActivesUntil(activeSibling);

            RemoveFromActivesUntil(activeSibling);

            AddToActivesFrom(newActiveState);

            OnActiveHierarchyPathChanged?.Invoke();

            EnterActivesFrom(newActiveState);
            
            OnTransitionFinished?.Invoke();
        }
    }
}