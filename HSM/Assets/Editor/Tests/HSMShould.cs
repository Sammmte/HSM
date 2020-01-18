using NSubstitute;
using NUnit.Framework;
using Paps.FSM;
using Paps.FSM.HSM;
using System.Linq;
using System;

namespace Tests
{
    public class HSMShould
    {
        [Test]
        public void AddStates()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            Assert.IsTrue(hsm.StateCount == 0);

            hsm.AddState(1, state1);

            Assert.IsTrue(hsm.StateCount == 1);

            hsm.AddState(2, state2);

            Assert.IsTrue(hsm.StateCount == 2);
        }

        [Test]
        public void RemoveStates()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.RemoveState(1);

            Assert.IsTrue(hsm.StateCount == 1);

            hsm.RemoveState(2);

            Assert.IsTrue(hsm.StateCount == 0);
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToAddTheSameStateId()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddState(1, state2));
        }

        [Test]
        public void ReturnCorrespondingValueWhenAskIfContainsState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.IsTrue(hsm.ContainsState(1));
            Assert.IsFalse(hsm.ContainsState(2));

            hsm.AddState(2, state2);

            Assert.IsTrue(hsm.ContainsState(2));

            hsm.RemoveState(1);

            Assert.IsFalse(hsm.ContainsState(1));
            Assert.IsTrue(hsm.ContainsState(2));

            hsm.RemoveState(2);

            Assert.IsFalse(hsm.ContainsState(2));
        }

        [Test]
        public void ReturnStateObjectByStateId()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            Assert.AreEqual(state1, hsm.GetStateById(1));
            Assert.AreEqual(state2, hsm.GetStateById(2));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToGetAStateWithNotExistingId()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetStateById(1));
        }

        [Test]
        public void ReturnStates()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            Assert.IsTrue(hsm.GetStates().Contains(1) && hsm.GetStates().Contains(2));
        }

        [Test]
        public void CreateSubstateRelations()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            Assert.IsTrue(hsm.AreRelatives(1, 2));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToCreateASubstateRelationWithNotExistingIds()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetSubstateRelation(1, 2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.SetSubstateRelation(2, 1));
            Assert.Throws<StateIdNotAddedException>(() => hsm.SetSubstateRelation(2, 3));
        }

        [Test]
        public void RemoveSubstateRelations()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            Assert.IsTrue(hsm.AreRelatives(1, 2));

            hsm.RemoveSubstateRelation(1, 2);

            Assert.IsFalse(hsm.AreRelatives(1, 2));
        }

        [Test]
        public void ReturnParentOfAState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            Assert.IsTrue(hsm.GetParentOf(2) == 1);
        }

        [Test]
        public void ReturnChildsOfAState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(1, 3);

            Assert.IsTrue(hsm.GetChildsOf(1).Contains(2) && hsm.GetChildsOf(1).Contains(3));
        }

        [Test]
        public void LetChangeInitialState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.InitialState = 1;

            Assert.IsTrue(hsm.InitialState == 1);

            hsm.InitialState = 2;

            Assert.IsTrue(hsm.InitialState == 2);
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToStartWithoutInitialState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            Assert.Throws<InvalidInitialStateException>(() => hsm.Start());
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToStartWhenItIsAlreadyStarted()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.Throws<StateMachineStartedException>(() => hsm.Start());
        }

        [Test]
        public void Stop()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.DoesNotThrow(() => hsm.Stop());

            Assert.IsFalse(hsm.IsStarted);
        }

        [Test]
        public void ReturnCorrespondingValueWhenUserAsksIfIsStarted()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            hsm.InitialState = 1;

            Assert.IsFalse(hsm.IsStarted);

            hsm.Start();

            Assert.IsTrue(hsm.IsStarted);

            hsm.Stop();

            Assert.IsFalse(hsm.IsStarted);
        }

        [Test]
        public void ReturnCorrespondingValueWhenUserAsksIfIsInSpecificState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.IsTrue(hsm.IsInState(1));

            hsm.Stop();

            hsm.InitialState = 2;

            hsm.Start();

            Assert.IsTrue(hsm.IsInState(2));
        }

        [Test]
        public void ReturnActiveHierarchyPath()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.InitialState = 1;

            hsm.Start();

            var hierarchyPath = hsm.GetActiveHierarchyPath();

            Assert.IsTrue(hierarchyPath.Contains(1) && (hierarchyPath.Contains(2) == false));
            Assert.IsTrue(hierarchyPath.First() == 1);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetSubstateRelation(1, 2);

            hierarchyPath = hsm.GetActiveHierarchyPath();

            Assert.IsTrue(hierarchyPath.Contains(1) && hierarchyPath.Contains(2));
            Assert.IsTrue(hierarchyPath.First() == 1 && hierarchyPath.Last() == 2);
        }

        [Test]
        public void ReturnCorrespondingValueWhenUserAsksIfContainsSubstateRelation()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            
            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(2, 3);
            
            Assert.IsTrue(hsm.AreRelatives(1, 2));
            Assert.IsTrue(hsm.AreRelatives(1, 3));
            Assert.IsTrue(hsm.AreRelatives(2, 3));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToSetSubstateRelationBetweenParentAndParent()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            
            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetSubstateRelation(1, 1));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToSetSubstateRelationBetweenParentAndGrandfather()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            
            hsm.SetSubstateRelation(1, 2);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetSubstateRelation(2, 1));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToSetSubstateRelationAndChildAlreadyHasAParent()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();
            IState state4 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);

            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(3, 4);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetSubstateRelation(1, 4));
        }

        [Test]
        public void RemoveImmediateSubstateRelationsRelatedToARemovedState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            hsm.RemoveState(1);

            Assert.IsFalse(hsm.AreRelatives(1, 2));

            Assert.IsTrue(hsm.ContainsState(2));
        }

        [Test]
        public void ReturnRootStates()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();
            IState state4 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);

            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(3, 4);

            int[] roots = hsm.GetRoots();

            Assert.IsTrue(roots.Contains(1) && roots.Contains(3));
            Assert.IsFalse(roots.Contains(2) && roots.Contains(4));
        }

        [Test]
        public void ConvertToRootChildsOfARemovedState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            hsm.RemoveState(1);

            Assert.IsTrue(hsm.GetRoots().Contains(2));
        }

        [Test]
        public void ThrowAnExceptionIfInitialStateIsNotRootWhenStarted()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetSubstateRelation(1, 2);

            hsm.InitialState = 2;

            Assert.Throws<InvalidInitialStateException>(hsm.Start);
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToRemoveAStateThatIsRootOfActiveHierarchyPath()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetSubstateRelation(1, 2);

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);

            hsm.Start();

            Assert.Throws<InvalidOperationException>(() => hsm.RemoveState(1));
            Assert.DoesNotThrow(() => hsm.RemoveState(2));
            Assert.DoesNotThrow(() => hsm.RemoveState(3));
        }

        [Test]
        public void PreventSideEffectsIfIsInvalidInitialStateWhenStarting()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();
            IState state4 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);

            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(2, 3);
            hsm.SetSubstateRelation(3, 4);

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            Assert.Throws<InvalidInitialStateException>(hsm.Start);
            Assert.IsFalse(hsm.IsStarted);
        }

        [Test]
        public void LetSetSubstateRelationToAStateThatIsInTheActiveHierarchyPathAndEnterItIfIsTheOnlyOne()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.InitialState = 1;

            hsm.Start();

            hsm.SetInitialStateTo(1, 2);
            hsm.SetSubstateRelation(1, 2);

            state2.Received(1).Enter();

            hsm.SetSubstateRelation(1, 3);

            state3.DidNotReceive().Enter();
        }

        [Test]
        public void LetRemoveSubstateRelationToAStateThatIsInTheActiveHierarchyPathAndSwitchToTheInitialStateIfItWasTheActiveChild()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();
            IState state4 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);

            hsm.SetSubstateRelation(1, 2);
            hsm.SetSubstateRelation(1, 3);
            hsm.SetSubstateRelation(2, 4);

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 4);

            hsm.Start();

            hsm.SetInitialStateTo(1, 3);

            hsm.RemoveState(2);

            state2.Received(1).Exit();
            state4.Received(1).Exit();
            state3.Received(1).Enter();
        }
    }
}