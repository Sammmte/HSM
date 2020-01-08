using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Paps.FSM.HSM
{
    internal class StateHierarchyNode<TState> : IEquatable<StateHierarchyNode<TState>>, IEnumerable<StateHierarchyNode<TState>>
    {
        public readonly TState StateId;
        public IState StateObject { get; set; }
        
        public StateHierarchyNode<TState> Parent { get; set; }
        public StateHierarchyNode<TState> ActiveChild { get; private set; }
        public TState InitialState { get; set; }

        private Dictionary<TState, StateHierarchyNode<TState>> _childs;
        private IEqualityComparer<TState> _stateComparer;

        public bool IsActive { get; private set; }
        public int ChildCount => _childs.Count;

        public StateHierarchyNode(TState stateId, IState stateObject, IEqualityComparer<TState> stateComparer = null)
        {
            StateId = stateId;
            StateObject = stateObject;
            _stateComparer = stateComparer ?? EqualityComparer<TState>.Default;

            _childs = new Dictionary<TState, StateHierarchyNode<TState>>(_stateComparer);
        }

        public void AddChild(StateHierarchyNode<TState> child)
        {
            ValidateCanAddChild(child);

            child.Parent = this;
            _childs.Add(child.StateId, child);

            if (IsActive && _childs.Count == 1)
            {
                EnterInitialChild();
            }
        }

        public void RemoveChild(TState stateId)
        {
            ValidateCanRemove(stateId);

            _childs[stateId].Parent = null;
            _childs.Remove(stateId);

            if (IsActive)
            {
                ExitActiveChild();
                
                if(_childs.Count > 0)
                    EnterInitialChild();
            }
        }

        public bool ContainsChild(TState stateId)
        {
            return GetChild(stateId) != null;
        }

        public StateHierarchyNode<TState> GetImmediateChild(TState stateId)
        {
            if(_childs.ContainsKey(stateId))
            {
                return _childs[stateId];
            }

            return null;
        }

        public IEnumerable<StateHierarchyNode<TState>> GetImmediateChilds()
        {
            return _childs.Values.ToArray();
        }

        public StateHierarchyNode<TState> GetChild(TState stateId)
        {
            var childNode = GetImmediateChild(stateId);

            if (childNode != null) return childNode;

            foreach(var child in _childs)
            {
                childNode = child.Value.GetChild(stateId);
            }

            return childNode;
        }

        public IEnumerable<StateHierarchyNode<TState>> GetChilds()
        {
            var childs = new HashSet<StateHierarchyNode<TState>>();

            GetChildsRecursively(childs);

            return childs;
        }

        private void GetChildsRecursively(HashSet<StateHierarchyNode<TState>> childs)
        {
            foreach(var child in _childs)
            {
                childs.Add(child.Value);
                child.Value.GetChildsRecursively(childs);
            }
        }

        private void ValidateCanRemove(TState stateId)
        {
            if (ActiveChild != null && _stateComparer.Equals(ActiveChild.StateId, stateId))
                throw new InvalidOperationException("Cannot remove state while it's active");
        }

        private void ValidateCanAddChild(StateHierarchyNode<TState> child)
        {
            ValidateDoesNotHasStateId(child.StateId);
            ValidateFirstStateIsInitialStateWhenActive(child.StateId);
        }

        private void ValidateDoesNotHasStateId(TState stateId)
        {
            if (_stateComparer.Equals(StateId, stateId) || _childs.ContainsKey(stateId)) throw new StateIdAlreadyAddedException();
        }

        private void ValidateFirstStateIsInitialStateWhenActive(TState stateId)
        {
            if(IsActive && _childs.Count == 0 && _stateComparer.Equals(InitialState, stateId) == false)
                throw new InvalidOperationException("Substate " + stateId + " must be the initial state");
        }

        private void ValidateContainsChild(TState stateId)
        {
            if (ContainsChild(stateId) == false) throw new StateIdNotAddedException();
        }

        public void Enter()
        {
            ValidateCanEnter();

            IsActive = true;

            StateObject.Enter();

            if(_childs.Count > 0)
            {
                EnterInitialChild();
            }
        }

        private void EnterInitialChild()
        {
            ActiveChild = _childs[InitialState];

            if (ActiveChild != null)
            {
                ActiveChild.Enter();
            }
        }

        private void ValidateCanEnter()
        {
            ValidateInitialState();
        }

        private void ValidateInitialState()
        {
            if (_childs.Count > 0 && _childs.ContainsKey(InitialState) == false) throw new InvalidInitialStateException();
        }

        public void Update()
        {
            StateObject.Update();

            if(ActiveChild != null)
            {
                ActiveChild.Update();
            }
        }

        public void Exit()
        {
            ExitActiveChild();

            StateObject.Exit();

            IsActive = false;
        }

        private void ExitActiveChild()
        {
            if(ActiveChild != null)
            {
                ActiveChild.Exit();
                ActiveChild = null;
            }
        }

        public bool Equals(StateHierarchyNode<TState> other)
        {
            return _stateComparer.Equals(StateId, other.StateId);
        }

        public IEnumerator<StateHierarchyNode<TState>> GetEnumerator()
        {
            return _childs.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}