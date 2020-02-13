using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Paps.StateMachines
{
    internal class HierarchicalEventDispatcher<TState>
    {
        private IEqualityComparer<TState> _stateComparer;
        private Dictionary<TState, List<IStateEventHandler>> _eventHandlers;

        private StateHierarchyBehaviourScheduler<TState> _stateHierarchyBehaviourScheduler;

        public HierarchicalEventDispatcher(IEqualityComparer<TState> stateComparer, StateHierarchyBehaviourScheduler<TState> stateHierarchyBehaviourScheduler)
        {
            _stateComparer = stateComparer ?? throw new ArgumentNullException(nameof(stateComparer));

            _eventHandlers = new Dictionary<TState, List<IStateEventHandler>>(_stateComparer);
            _stateHierarchyBehaviourScheduler = stateHierarchyBehaviourScheduler;
        }

        public void AddEventHandlerTo(TState stateId, IStateEventHandler eventHandler)
        {
            ValidateIsNotNull(eventHandler);

            if (_eventHandlers.ContainsKey(stateId) == false)
                _eventHandlers.Add(stateId, new List<IStateEventHandler>());

            _eventHandlers[stateId].Add(eventHandler);
        }

        private void ValidateIsNotNull(IStateEventHandler eventHandler)
        {
            if (eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));
        }

        public bool RemoveEventHandlerFrom(TState stateId, IStateEventHandler eventHandler)
        {
            if (_eventHandlers.ContainsKey(stateId))
            {
                bool removed = _eventHandlers[stateId].Remove(eventHandler);

                if (_eventHandlers[stateId].Count == 0)
                    _eventHandlers.Remove(stateId);

                return removed;
            }

            return false;
        }

        public bool HasEventHandlerOn(TState stateId, IStateEventHandler eventHandler)
        {
            if (_eventHandlers.ContainsKey(stateId))
                return _eventHandlers[stateId].Contains(eventHandler);

            return false;
        }

        public IStateEventHandler[] GetEventHandlersOf(TState stateId)
        {
            if (_eventHandlers.ContainsKey(stateId))
                return _eventHandlers[stateId].ToArray();
            else
                return null;
        }

        public bool SendEvent(IEvent ev)
        {
            var activeHierarchyPath = _stateHierarchyBehaviourScheduler.GetActiveHierarchyPath();

            for(int i = activeHierarchyPath.Count - 1; i >= 0; i--)
            {
                if(_eventHandlers.ContainsKey(activeHierarchyPath[i].Key))
                {
                    var eventHandlers = _eventHandlers[activeHierarchyPath[i].Key];

                    for(int j = 0; j < eventHandlers.Count; j++)
                    {
                        if (eventHandlers[j].HandleEvent(ev))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}