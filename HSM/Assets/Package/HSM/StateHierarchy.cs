using System;
using System.Collections.Generic;
using System.Linq;

namespace Paps.FSM.HSM
{
    internal class StateHierarchy<TState>
    {
        public int StateCount => _states.Count;

        public int RootCount => _roots.Count;

        private StateEqualityComparer _stateComparer;
        private Dictionary<TState, StateHierarchyNode> _states;
        private Dictionary<TState, StateHierarchyNode> _roots;
        private StateHierarchyNode _currentHierarchyRootNode;
        private List<Exception> _eventExceptionList;

        public bool IsStarted { get; private set; }
        
        public TState InitialState { get; set; }

        public StateHierarchy(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer = new StateEqualityComparer(stateComparer ?? EqualityComparer<TState>.Default);
            _states = new Dictionary<TState, StateHierarchyNode>(_stateComparer);
            _roots = new Dictionary<TState, StateHierarchyNode>(_stateComparer);
            _eventExceptionList = new List<Exception>();
        }

        public StateHierarchy() : this(EqualityComparer<TState>.Default)
        {

        }

        public void SetStateComparer(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer.StateComparer = stateComparer;
        }

        public void AddState(TState stateId, IState state)
        {
            ValidateCanAddState(stateId, state);

            var stateNode = new StateHierarchyNode(stateId, state, _stateComparer);

            _states.Add(stateId, stateNode);

            _roots.Add(stateId, stateNode);
        }

        private void ValidateCanAddState(TState stateId, IState state)
        {
            if (_states.ContainsKey(stateId)) throw new StateIdAlreadyAddedException();
        }

        public bool RemoveState(TState stateId)
        {
            if(_states.ContainsKey(stateId))
            {
                ValidateCanRemoveState(stateId);

                StateHierarchyNode node = _states[stateId];

                if (node.Parent != null)
                {
                    RemoveSubstateRelation(node.Parent.StateId, stateId);
                }
                else
                {
                    var childNodes = node.Childs.Values.ToArray();

                    for (int i = 0; i < childNodes.Length; i++)
                    {
                        RemoveSubstateRelation(stateId, childNodes[i].StateId);
                    }
                }

                _roots.Remove(stateId);
                _states.Remove(stateId);

                return true;
            }

            return false;
        }

        private void ValidateCanRemoveState(TState stateId)
        {
            ValidateIsNotActiveRoot(stateId);
        }

        private void ValidateIsNotActiveRoot(TState stateId)
        {
            if (IsInActiveHierarchyPath(stateId) && IsHierarchyRoot(stateId))
                throw new InvalidOperationException("Cannot remove state because it is the root of the active hierarchy path");
        }

        private void ValidateSwitchToInitialStateIfIsAnActiveState(TState stateId)
        {
            if (IsInActiveHierarchyPath(stateId))
            {
                var node = _states[stateId];

                if(IsValidNodeInitialStateRecursively(node) == false || _stateComparer.Equals(node.InitialState, stateId))
                {
                    throw new InvalidOperationException("Cannot remove state because a switch to the initial state would be invalid");
                }
            }
        }

        private bool IsValidNodeInitialStateRecursively(StateHierarchyNode node)
        {
            if (IsValidNodeInitialState(node))
            {
                if (node.Childs.ContainsKey(node.InitialState))
                    return IsValidNodeInitialStateRecursively(_states[node.InitialState]);
                else
                    return true;
            }

            return false;
        }

        private bool IsValidNodeInitialState(StateHierarchyNode node)
        {
            return (node.Childs.Count > 0 && node.Childs.ContainsKey(node.InitialState)) || node.Childs.Count == 0;
        }

        public bool ContainsState(TState stateId)
        {
            return _states.ContainsKey(stateId);
        }

        public TState[] GetStates()
        {
            return _states.Keys.ToArray();
        }

        public IState GetStateById(TState id)
        {
            if (_states.ContainsKey(id) == false) throw new StateIdNotAddedException();

            return _states[id].StateObject;
        }

        public void SetSubstateRelation(TState parent, TState child)
        {
            ValidateCanSetSubstateRelation(parent, child);

            var childNode = _states[child];

            _roots.Remove(child);
            
            AddChildTo(_states[parent], childNode);
        }

        private void AddChildTo(StateHierarchyNode parent, StateHierarchyNode child)
        {
            child.Parent = parent;
            parent.Childs.Add(child.StateId, child);

            if (parent.IsActive && parent.Childs.Count == 1)
            {
                EnterInitialChildOf(parent);
            }
        }

        private void RemoveChildFrom(StateHierarchyNode parent, StateHierarchyNode child)
        {
            if (parent.ActiveChild != null && _stateComparer.Equals(parent.ActiveChild.StateId, child.StateId))
            {
                ExitState(parent.ActiveChild);
                parent.ActiveChild.Parent = null;

                parent.ActiveChild = null;
            }

            parent.Childs.Remove(child.StateId);
                
            if (parent.Childs.Count > 0)
            {
                EnterInitialChildOf(parent);
            }
        }

        private void EnterInitialChildOf(StateHierarchyNode node)
        {
            node.ActiveChild = node.Childs[node.InitialState];

            if (node.ActiveChild != null)
            {
                EnterState(node.ActiveChild);
            }
        }

        private void EnterState(StateHierarchyNode begin)
        {
            ValidateInitialStatesRecursively(begin);
            EnterNodesRecursively(begin);
            CallEnterEventRecursively(begin);
        }

        private void ValidateInitialStatesRecursively(StateHierarchyNode begin)
        {
            if(IsValidNodeInitialStateRecursively(begin) == false) throw new InvalidInitialStateException();
        }

        private void EnterNodesRecursively(StateHierarchyNode begin)
        {
            var node = begin;

            while(node != null)
            {
                node.IsActive = true;

                if(node.Childs.Count > 0)
                {
                    node.ActiveChild = node.Childs[node.InitialState];
                    node = node.ActiveChild;
                }
                else
                {
                    node = null;
                }
            }
        }

        private void CallEnterEventRecursively(StateHierarchyNode begin)
        {
            var node = begin;

            while(node != null)
            {
                DoEventSafely(node.StateObject.Enter, _eventExceptionList);

                node = node.ActiveChild;
            }

            ThrowSingleOrAggregateIfNotEmptyAndClear(_eventExceptionList);
        }

        private void ThrowSingleOrAggregateIfNotEmptyAndClear(List<Exception> exceptions)
        {
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    var singleException = exceptions.First();
                    exceptions.Clear();
                    throw singleException;
                }
                else
                {
                    var aggregateException = new AggregateException(exceptions);
                    exceptions.Clear();
                    throw aggregateException;
                }
            }
            
            exceptions.Clear();
        }

        private void DoEventSafely(Action action, List<Exception> exceptionsList)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                exceptionsList.Add(e);
            }
        }

        private void ExitState(StateHierarchyNode node)
        {
            var topNode = node.Parent;
            var leaf = GetActiveLeafOf(node);

            while(leaf != topNode)
            {
                DoEventSafely(leaf.StateObject.Exit, _eventExceptionList);

                leaf = leaf.Parent;
            }
            
            ThrowSingleOrAggregateIfNotEmptyAndClear(_eventExceptionList);
        }

        private StateHierarchyNode GetActiveLeafOf(StateHierarchyNode node)
        {
            if(node.IsActive)
            {
                if (node.ActiveChild != null)
                {
                    return GetActiveLeafOf(node.ActiveChild);
                }

                return node;
            }

            return null;
        }

        private void ValidateCanSetSubstateRelation(TState parent, TState child)
        {
            ValidateContainsStateId(parent);
            ValidateContainsStateId(child);
            ValidateHasNoParent(child);
            ValidateParentAndChildAreNotTheSame(parent, child);
            ValidateChildIsNotGrandfather(parent, child);
            ValidateNewChildIsInitialStateIfParentIsActiveAndHasNoChild(parent, child);
            ValidateNewChildHasValidInitialStatesInHierarchyPathIfParentIsActiveAndHasNoChild(parent, child);
        }

        public bool RemoveSubstateRelation(TState parent, TState child)
        {
            if(ContainsState(parent) && ContainsState(child))
            {
                var parentNode = _states[parent];

                if (parentNode.Childs.ContainsKey(child))
                {
                    ValidateCanRemoveSubstateRelation(parent, child);

                    var childNode = parentNode.Childs[child];
                    
                    RemoveChildFrom(parentNode, childNode);

                    _roots.Add(childNode.StateId, childNode);

                    return true;
                }
            }

            return false;
        }

        private void ValidateCanRemoveSubstateRelation(TState parent, TState child)
        {
            ValidateSwitchToInitialStateIfIsAnActiveState(parent);
        }

        private void ValidateNewChildHasValidInitialStatesInHierarchyPathIfParentIsActiveAndHasNoChild(TState parent, TState child)
        {
            var parentNode = _states[parent];

            if (parentNode.IsActive && parentNode.Childs.Count == 0 && IsValidNodeInitialStateRecursively(_states[child]) == false)
                throw new InvalidOperationException("Cannot add child state because parent is active and child has an invalid state in the hierarchy");
        }

        private void ValidateNewChildIsInitialStateIfParentIsActiveAndHasNoChild(TState parent, TState child)
        {
            var parentNode = _states[parent];

            if(parentNode.IsActive && parentNode.Childs.Count == 0 && _stateComparer.Equals(parentNode.InitialState, child) == false)
                throw new InvalidOperationException("Cannot add child state because parent is active and child is not initial state");
        }

        private void ValidateHasNoParent(TState child)
        {
            if (HasParent(child))
                throw new InvalidSubstateRelationException
                ("Cannot set substate relation on state " + child.ToString() + 
                 " because it already has a parent. You could remove its current substate relation and then create a new one");
        }

        private void ValidateParentAndChildAreNotTheSame(TState parent, TState child)
        {
            if(_stateComparer.Equals(parent, child))
                throw new InvalidSubstateRelationException("Parent and child cannot have the same id");
        }

        private void ValidateChildIsNotGrandfather(TState parent, TState child)
        {
            if(AreRelatives(child, parent))
                throw new InvalidSubstateRelationException("Child cannot be parent's parent");
        }

        private bool ContainsChildRecursively(StateHierarchyNode parentNode, TState child)
        {
            if (parentNode.Childs.ContainsKey(child) == false)
            {
                foreach (var childNode in parentNode.Childs.Values)
                {
                    if (ContainsChildRecursively(childNode, child))
                        return true;
                }

                return false;
            }

            return true;
        }

        public bool AreRelatives(TState parent, TState child)
        {
            if(ContainsState(parent) && ContainsState(child))
            {
                return ContainsChildRecursively(_states[parent], child);
            }

            return false;
        }

        public bool AreImmediateRelatives(TState parent, TState child)
        {
            if (ContainsState(parent) && ContainsState(child))
            {
                return _states[parent].Childs.ContainsKey(child);
            }

            return false;
        }

        private bool HasParent(TState state)
        {
            if(_states.ContainsKey(state))
            {
                return _states[state].Parent != null;
            }

            return false;
        }

        private void ValidateContainsStateId(TState stateId)
        {
            if (ContainsState(stateId) == false) throw new StateIdNotAddedException();
        }
        
        public void SwitchTo(TState stateId)
        {
            ValidateIsStarted();

            
        }
        
        public bool IsValidSwitch(TState stateId)
        {
            if(ContainsState(stateId))
            {
                var node = _states[stateId];

                if(IsHierarchyRoot(stateId))
                {
                    return true;
                }
                else
                {
                    return node.Parent.IsActive;
                }
            }

            return false;
        }

        private bool IsHierarchyRoot(TState stateId)
        {
            return _roots.ContainsKey(stateId);
        }

        public void SetInitialStateTo(TState parent, TState stateId)
        {
            ValidateContainsStateId(stateId);
            ValidateContainsStateId(parent);

            _states[parent].InitialState = stateId;
        }

        private void ValidateInitialStates()
        {
            if (IsHierarchyRoot(InitialState) == false)
                throw new InvalidInitialStateException("Initial state is not root");

            ValidateInitialStatesOfState(InitialState);
        }

        private void ValidateInitialStatesOfState(TState stateId)
        {
            if (IsValidNodeInitialStateRecursively(_states[stateId]) == false) throw new InvalidInitialStateException();
        }

        private void ValidateIsStarted()
        {
            if (IsStarted == false) throw new InvalidOperationException("Cannot execute operation because state hierarchy is not started");
        }

        private void ValidateIsNotStarted()
        {
            if (IsStarted == true) throw new InvalidOperationException("Cannot execute operation because state hierarchy is already started");
        }

        public void Start()
        {
            ValidateIsNotStarted();
            ValidateInitialStates();

            IsStarted = true;

            _currentHierarchyRootNode = _states[InitialState];
            
            EnterState(_currentHierarchyRootNode);
        }

        public void Update()
        {
            ValidateIsStarted();

            UpdateState(_currentHierarchyRootNode);
        }

        private void UpdateState(StateHierarchyNode begin)
        {
            var node = begin;

            while (node != null)
            {
                node.StateObject.Update();

                node = node.ActiveChild;
            }
        }

        public void Stop()
        {
            if(IsStarted)
            {
                try
                {
                    ExitState(_currentHierarchyRootNode);

                    IsStarted = false;

                    _currentHierarchyRootNode = null;
                }
                catch
                {
                    IsStarted = false;

                    _currentHierarchyRootNode = null;

                    throw;
                }
            }
        }

        public IEnumerable<TState> GetActiveHierarchyPath()
        {
            ValidateIsStarted();

            var currentNode = _currentHierarchyRootNode;

            while(currentNode != null)
            {
                yield return currentNode.StateId;

                currentNode = currentNode.ActiveChild;
            }
        }

        public bool IsInState(TState stateId)
        {
            if(ContainsState(stateId))
            {
                return _states[stateId].IsActive;
            }

            return false;
        }

        public TState GetParentOf(TState child)
        {
            ValidateContainsStateId(child);

            var node = _states[child].Parent;

            if (node == null)
            {
                return child;
            }
            else
            {
                return node.StateId;
            }
        }

        public TState[] GetChildsOf(TState parent)
        {
            ValidateContainsStateId(parent);

            var node = _states[parent];

            if(node.Childs.Count > 0)
            {
                return node.Childs.Keys.ToArray();
            }

            return null;
        }

        private TState[] ToArray(IEnumerable<StateHierarchyNode> nodes)
        {
            TState[] array = new TState[nodes.Count()];

            int index = 0;

            foreach(var node in nodes)
            {
                array[index] = node.StateId;
                index++;
            }

            return array;
        }

        public TState[] GetRoots()
        {
            return _roots.Keys.ToArray();
        }

        private bool IsInActiveHierarchyPath(TState stateId)
        {
            return _states[stateId].IsActive;
        }

        private class StateHierarchyNode
        {
            public readonly TState StateId;
            public IState StateObject { get; set; }
        
            public StateHierarchyNode Parent { get; set; }
            public StateHierarchyNode ActiveChild { get; set; }
            public TState InitialState { get; set; }

            public Dictionary<TState, StateHierarchyNode> Childs;

            public bool IsActive { get; set; }

            public StateHierarchyNode(TState stateId, IState stateObject, IEqualityComparer<TState> stateComparer = null)
            {
                StateId = stateId;
                StateObject = stateObject;

                Childs = new Dictionary<TState, StateHierarchyNode>(stateComparer ?? EqualityComparer<TState>.Default);
            }
        }

        private class StateEqualityComparer : IEqualityComparer<TState>
        {
            public IEqualityComparer<TState> StateComparer;
            
            public StateEqualityComparer(IEqualityComparer<TState> stateComparer)
            {
                StateComparer = stateComparer;
            }
            
            public bool Equals(TState x, TState y)
            {
                return StateComparer.Equals(x, y);
            }

            public int GetHashCode(TState obj)
            {
                return StateComparer.GetHashCode(obj);
            }
        }
    }
}


