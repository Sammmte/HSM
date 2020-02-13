using System;
using System.Collections.Generic;
using System.Linq;

namespace Paps.StateMachines
{
    internal class StateHierarchy<TState>
    {
        public int StateCount => _states.Count;

        public int RootCount => _roots.Count;

        private IEqualityComparer<TState> _stateComparer;
        private Dictionary<TState, StateHierarchyNode> _states;
        private Dictionary<TState, StateHierarchyNode> _roots;

        private TState _initialState;

        public TState InitialState
        {
            get
            {
                return _initialState;
            }

            set
            {
                ValidateContainsId(value);

                _initialState = value;
            }
        }

        public StateHierarchy(IEqualityComparer<TState> stateComparer)
        {
            _stateComparer = stateComparer ?? EqualityComparer<TState>.Default;
            _states = new Dictionary<TState, StateHierarchyNode>(_stateComparer);
            _roots = new Dictionary<TState, StateHierarchyNode>(_stateComparer);
        }

        public StateHierarchy() : this(EqualityComparer<TState>.Default)
        {

        }

        public void AddState(TState stateId, IState stateObj)
        {
            ValidateDoesNotContainsStateId(stateId);
            ValidateStateObjectIsNotNull(stateObj);

            var node = new StateHierarchyNode(stateId, stateObj, _stateComparer);

            _states.Add(stateId, node);
            _roots.Add(stateId, node);

            if(StateCount == 1)
            {
                InitialState = stateId;
            }
        }

        private void ValidateStateObjectIsNotNull(IState stateObj)
        {
            if (stateObj == null) throw new ArgumentNullException(nameof(stateObj));
        }

        private void ValidateDoesNotContainsStateId(TState stateId)
        {
            if (ContainsState(stateId)) throw new StateIdAlreadyAddedException(stateId.ToString());
        }

        public bool RemoveState(TState stateId)
        {
            if(ContainsState(stateId))
            {
                RemoveChildsOf(stateId);

                _states.Remove(stateId);
                _roots.Remove(stateId);

                if(StateCount == 0)
                {
                    _initialState = default;
                }
            }

            return false;
        }

        private void RemoveChildsOf(TState stateId)
        {
            var node = NodeOf(stateId);

            if (HasParent(node))
            {
                RemoveChildFromParent(stateId);
            }

            var childs = GetImmediateChildsOf(stateId);

            if (childs != null)
            {
                for (int i = 0; i < childs.Length; i++)
                {
                    RemoveChildFromParent(childs[i]);
                }
            }
        }

        private bool HasParent(StateHierarchyNode node)
        {
            return node.Parent != null;
        }

        public bool HasParent(TState stateId)
        {
            ValidateContainsId(stateId);

            return NodeOf(stateId).Parent != null;
        }

        public bool HasChilds(TState stateId)
        {
            ValidateContainsId(stateId);

            return NodeOf(stateId).Childs.Count > 0;
        }

        public void AddChildTo(TState parentId, TState childId)
        {
            ValidateContainsId(parentId);
            ValidateContainsId(childId);

            if(AreImmediateParentAndChild(parentId, childId) == false)
            {
                ValidateChildHasNotParent(childId);
                ValidateParentAndChildAreNotTheSame(parentId, childId);
                ValidateChildIsNotParentOfParent(parentId, childId);

                var parentNode = NodeOf(parentId);
                var childNode = NodeOf(childId);

                parentNode.Childs.Add(childId, childNode);

                childNode.Parent = parentNode;

                _roots.Remove(childId);

                if(ChildCountOf(parentId) == 1)
                {
                    SetInitialStateTo(parentId, childId);
                }
            }
        }

        private void ValidateChildHasNotParent(TState childId)
        {
            var childNode = NodeOf(childId);

            if (HasParent(childNode)) 
                throw new CannotAddChildException("State with id " + childId.ToString() + " has parent with id " + childNode.Parent.StateId.ToString());
        }

        private void ValidateParentAndChildAreNotTheSame(TState parentId, TState childId)
        {
            if (AreEquals(parentId, childId)) 
                throw new CannotAddChildException("Cannot set substate relation with parent and child with same id");
        }

        private void ValidateChildIsNotParentOfParent(TState parentId, TState childId)
        {
            var parentNode = NodeOf(parentId);

            if (HasParent(parentNode) && AreEquals(parentNode.Parent.StateId, childId))
                throw new CannotAddChildException("State with id " + parentId.ToString() + " cannot be parent of " + childId.ToString() + " because the last is parent of the first");
        }

        public bool RemoveChildFromParent(TState childId)
        {
            ValidateContainsId(childId);

            var childNode = NodeOf(childId);

            if (childNode.Parent != null)
            {
                var parentNode = childNode.Parent;
                
                parentNode.Childs.Remove(childId);
                childNode.Parent = null;

                _roots.Add(childId, childNode);

                if(ChildCountOf(parentNode.StateId) == 0)
                {
                    parentNode.InitialState = default;
                }

                return true;
            }

            return false;
        }

        public bool AreImmediateParentAndChild(TState parentId, TState childId)
        {
            if(ContainsState(parentId) && ContainsState(childId))
            {
                return NodeOf(parentId).Childs.ContainsKey(childId);
            }

            return false;
        }

        public TState[] GetStates()
        {
            if (_states.Count > 0)
                return _states.Keys.ToArray();
            else
                return null;
        }

        public TState[] GetRoots()
        {
            if (_roots.Count > 0) 
                return _roots.Keys.ToArray();
            else 
                return null;
        }

        public TState[] GetImmediateChildsOf(TState stateId)
        {
            ValidateContainsId(stateId);

            var node = NodeOf(stateId);

            if (node.Childs.Count > 0)
                return node.Childs.Keys.ToArray();
            else
                return null;
        }

        public TState GetParentOf(TState stateId)
        {
            ValidateContainsId(stateId);

            var node = NodeOf(stateId);

            if (HasParent(node)) 
                return node.Parent.StateId;
            else 
                return stateId;
        }

        public bool ContainsState(TState stateId)
        {
            if (stateId == null)
                return false;

            return _states.ContainsKey(stateId);
        }

        public IState GetStateById(TState stateId)
        {
            ValidateContainsId(stateId);

            return _states[stateId].StateObject;
        }

        public void SetInitialStateTo(TState parentId, TState initialChildId)
        {
            ValidateContainsId(parentId);
            ValidateContainsId(initialChildId);
            ValidateAreParentAndChild(parentId, initialChildId);

            NodeOf(parentId).InitialState = initialChildId;
        }

        public TState GetInitialStateOf(TState parentId)
        {
            ValidateContainsId(parentId);

            return NodeOf(parentId).InitialState;
        }

        private void ValidateAreParentAndChild(TState parentId, TState childId)
        {
            if (AreImmediateParentAndChild(parentId, childId) == false) throw new InvalidInitialStateException("State with id " + parentId.ToString() + " is not parent of " + childId.ToString());
        }

        private void ValidateContainsId(TState stateId)
        {
            if (ContainsState(stateId) == false) throw new StateIdNotAddedException(stateId.ToString());
        }

        private bool AreEquals(TState stateId1, TState stateId2)
        {
            return _stateComparer.Equals(stateId1, stateId2);
        }

        private StateHierarchyNode NodeOf(TState stateId)
        {
            return _states[stateId];
        }

        public bool IsRoot(TState stateId)
        {
            return _roots.ContainsKey(stateId);
        }

        public int ChildCountOf(TState stateId)
        {
            ValidateContainsId(stateId);

            return NodeOf(stateId).Childs.Count;
        }

        public bool AreSiblings(TState stateId1, TState stateId2)
        {
            ValidateContainsId(stateId1);
            ValidateContainsId(stateId2);

            if (AreEquals(stateId1, stateId2))
                return false;

            return NodeOf(stateId1).Parent == NodeOf(stateId2).Parent;
        }

        public bool AreCousins(TState stateId1, TState stateId2)
        {
            ValidateContainsId(stateId1);
            ValidateContainsId(stateId2);

            if (AreEquals(stateId1, stateId2))
                return false;

            if(IsRoot(stateId1) == false)
            {
                if(IsRoot(stateId2) == false)
                {
                    var parent1 = GetParentOf(stateId1);
                    var parent2 = GetParentOf(stateId2);

                    return AreSiblings(parent1, parent2);
                }
            }

            return false;
        }

        public bool AreParentAndChildAtAnyLevel(TState parent, TState child)
        {
            ValidateContainsId(parent);
            ValidateContainsId(child);

            var node = NodeOf(parent);

            if (AreImmediateParentAndChild(node.StateId, child))
                return true;

            foreach (var childId in node.Childs.Keys)
            {
                if (AreParentAndChildAtAnyLevel(childId, child))
                    return true;
            }

            return false;
        }

        public bool AreParentAndInitialChildAtAnyLevel(TState parent, TState child)
        {
            ValidateContainsId(parent);
            ValidateContainsId(child);

            var node = NodeOf(parent);

            while(ChildCountOf(node.StateId) != 0)
            {
                if (AreEquals(node.InitialState, child))
                    return true;

                node = NodeOf(node.InitialState);
            }

            return false;
        }

        private class StateHierarchyNode
        {
            public readonly TState StateId;
            public IState StateObject { get; set; }
        
            public StateHierarchyNode Parent { get; set; }
            public TState InitialState { get; set; }

            public Dictionary<TState, StateHierarchyNode> Childs;

            public StateHierarchyNode(TState stateId, IState stateObject, IEqualityComparer<TState> stateComparer = null)
            {
                StateId = stateId;
                StateObject = stateObject;

                Childs = new Dictionary<TState, StateHierarchyNode>(stateComparer ?? EqualityComparer<TState>.Default);
            }
        }
    }
}


