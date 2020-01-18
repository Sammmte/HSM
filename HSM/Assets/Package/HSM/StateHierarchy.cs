using System;
using System.Collections.Generic;
using System.Linq;

namespace Paps.FSM.HSM
{
    internal class StateHierarchy<TState>
    {
        public int StateCount => _states.Count;

        public int RootCount => _hierarchies.Count;

        private IEqualityComparer<TState> _stateComparer;
        private Dictionary<TState, StateHierarchyNode<TState>> _states;
        private Dictionary<TState, StateHierarchyNode<TState>> _hierarchies;
        private StateHierarchyNode<TState> _currentHierarchyRootNode;

        public bool IsStarted { get; private set; }
        
        public TState InitialState { get; set; }

        public StateHierarchy(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer = stateComparer ?? EqualityComparer<TState>.Default;
            _states = new Dictionary<TState, StateHierarchyNode<TState>>(_stateComparer);
            _hierarchies = new Dictionary<TState, StateHierarchyNode<TState>>(_stateComparer);
        }

        public StateHierarchy() : this(EqualityComparer<TState>.Default)
        {

        }

        public void AddState(TState stateId, IState state)
        {
            ValidateCanAddState(stateId, state);

            var stateNode = new StateHierarchyNode<TState>(stateId, state, _stateComparer);

            _states.Add(stateId, stateNode);

            _hierarchies.Add(stateId, stateNode);
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

                StateHierarchyNode<TState> node = _states[stateId];

                if (node.Parent != null)
                {
                    RemoveSubstateRelation(node.Parent.StateId, stateId);
                }
                else
                {
                    IEnumerable<StateHierarchyNode<TState>> nodeChilds = node.GetImmediateChilds();

                    foreach (var child in nodeChilds)
                    {
                        RemoveSubstateRelation(stateId, child.StateId);
                    }
                }

                _hierarchies.Remove(stateId);
                _states.Remove(stateId);

                return true;
            }

            return false;
        }

        private void ValidateCanRemoveState(TState stateId)
        {
            if (IsInActiveHierarchyPath(stateId) && IsHierarchyRoot(stateId))
                throw new InvalidOperationException("Cannot remove state because it is the root of the active hierarchy path");
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

            _hierarchies.Remove(child);

            _states[parent].AddChild(childNode);
        }

        public bool RemoveSubstateRelation(TState parent, TState child)
        {
            if(ContainsSubstateRelation(parent, child))
            {
                var parentNode = _states[parent];
                var childNode = parentNode.GetImmediateChild(child);

                parentNode.RemoveChild(child);

                _hierarchies.Add(childNode.StateId, childNode);

                return true;
            }

            return false;
        }

        private void ValidateCanSetSubstateRelation(TState parent, TState child)
        {
            ValidateContainsStateId(parent);
            ValidateContainsStateId(child);
            ValidateHasNoParent(child);
            ValidateParentAndChildAreNotTheSame(parent, child);
            ValidateChildIsNotGrandfather(parent, child);
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
            if(ContainsSubstateRelation(child, parent))
                throw new InvalidSubstateRelationException("Child cannot be parent's parent");
        }

        public bool ContainsSubstateRelation(TState parent, TState child)
        {
            if(ContainsState(parent) && ContainsState(child))
            {
                return _states[parent].ContainsChild(child);
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

            if(IsValidSwitch(stateId))
            {
                if(IsHierarchyRoot(stateId))
                {
                    _currentHierarchyRootNode.Exit();

                    _currentHierarchyRootNode = _states[stateId];

                    _currentHierarchyRootNode.Enter();
                }
                else
                {
                    var previous = _states[stateId];

                    previous.Exit();

                    var next = previous.Parent.GetImmediateChild(stateId);

                    next.Enter();
                }
            }
        }
        
        private bool IsValidSwitch(TState stateId)
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
            return _hierarchies.ContainsKey(stateId);
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

            ValidateInitialStateOfRootNode();
        }

        private void ValidateInitialStateOfRootNode()
        {
            if (_states[InitialState].IsValidInitialStateRecursively() == false) throw new InvalidInitialStateException();
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

            _currentHierarchyRootNode.Enter();
        }

        public void Update()
        {
            ValidateIsStarted();

            _currentHierarchyRootNode.Update();
        }

        public void Stop()
        {
            if(IsStarted)
            {
                try
                {
                    _currentHierarchyRootNode.Exit();

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

            if(node.ChildCount > 0)
            {
                return ToArray(node.GetImmediateChilds());
            }

            return null;
        }

        private TState[] ToArray(IEnumerable<StateHierarchyNode<TState>> nodes)
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
            return _hierarchies.Keys.ToArray();
        }

        private bool IsInActiveHierarchyPath(TState stateId)
        {
            return _states[stateId].IsActive;
        }
    }
}


