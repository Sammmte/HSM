using NSubstitute;
using NUnit.Framework;
using Paps.FSM;
using Paps.FSM.HSM;
using System.Linq;
using System;
using Paps.FSM.Extensions;

namespace Tests
{
    public class HSMShould
    {
        [Test]
        public void Add_States()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            Assert.IsTrue(hsm.StateCount == 0);
            Assert.IsFalse(hsm.ContainsState(1));

            hsm.AddState(1, state1);

            Assert.IsTrue(hsm.StateCount == 1);
            Assert.IsTrue(hsm.ContainsState(1));
            Assert.IsTrue(hsm.GetStates().Contains(1));
            Assert.IsTrue(hsm.GetStateById(1) == state1);

            hsm.AddState(2, state2);

            Assert.IsTrue(hsm.StateCount == 2);
            Assert.IsTrue(hsm.ContainsState(1));
            Assert.IsTrue(hsm.ContainsState(2));
            Assert.IsTrue(hsm.GetStates().Contains(1));
            Assert.IsTrue(hsm.GetStates().Contains(2));
            Assert.IsTrue(hsm.GetStateById(2) == state2);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_The_Same_State_Id_Twice()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddState(1, state1));
            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddState(1, state2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_Null_State_Object()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            Assert.Throws<ArgumentNullException>(() => hsm.AddState(1, null));
        }

        [Test]
        public void Throw_An_Exception_If_State_Id_Type_Is_Reference_Type_And_User_Tries_To_Add_A_Null_State_Id()
        {
            HSM<object, int> hsm = new HSM<object, int>();

            IState state1 = Substitute.For<IState>();

            Assert.Throws<ArgumentNullException>(() => hsm.AddState(null, state1));
        }

        [Test]
        public void Set_Substate_Relations()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.IsTrue(hsm.GetParentOf(2) == 1);
            Assert.IsTrue(hsm.GetRoots().Contains(1));
            Assert.IsFalse(hsm.GetRoots().Contains(2));
            Assert.IsTrue(hsm.GetImmediateChildsOf(1).Contains(2));
            Assert.IsTrue(hsm.AreRelatives(1, 2));
        }

        [Test]
        public void Set_Substate_Relations_Between_Two_Roots_Having_Childs_Any_Of_Them()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);

            hsm.SetImmediateSubstateRelation(3, 1);

            Assert.IsTrue(hsm.GetParentOf(1) == 3);
            Assert.IsTrue(hsm.GetRoots().Contains(3));
            Assert.IsFalse(hsm.GetRoots().Contains(1));
            Assert.IsTrue(hsm.GetImmediateChildsOf(3).Contains(1));
            Assert.IsTrue(hsm.GetImmediateChildsOf(1).Contains(2));
            Assert.IsTrue(hsm.AreRelatives(1, 2));
            Assert.IsTrue(hsm.AreRelatives(3, 1));
            Assert.IsTrue(hsm.AreRelatives(3, 2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Substate_Relation_Between_Two_States_That_Were_Not_Added()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(1, 2));

            IState state = Substitute.For<IState>();

            hsm.AddState(1, state);

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(1, 2));

            hsm = new HSM<int, int>();

            hsm.AddState(2, state);

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(1, 2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Substate_Relation_And_Super_State_Is_Equals_To_Substate()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(1, 1));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Substate_Relation_And_Substate_Is_Parent_Or_Grandfather_Of_SuperState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(2, 1));

            hsm.SetImmediateSubstateRelation(2, 3);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(3, 1));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Substate_Relation_With_Substate_Having_A_Parent()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(3, 2));
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Set_An_Already_Exiting_Substate_Relation()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.DoesNotThrow(() => hsm.SetImmediateSubstateRelation(1, 2));
        }

        [Test]
        public void Start_And_Enter_Initial_Hierarchy_Path()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);

            hsm.Start();

            Assert.IsTrue(hsm.IsStarted);

            state1.Received(1).Enter();
            state2.Received(1).Enter();

            state3.DidNotReceive().Enter();
        }

        [Test]
        public void Throw_An_Exception_If_Is_Empty_When_Started()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            Assert.Throws<EmptyStateMachineException>(() => hsm.Start());
        }

        [Test]
        public void Throw_An_Exception_If_Does_Not_Contains_InitialState_When_Started()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.Throws<InvalidInitialStateException>(() => hsm.Start());
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Start_It_Twice_Without_Been_Stopped()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.Throws<StateMachineStartedException>(() => hsm.Start());
        }

        [Test]
        public void Throw_An_Exception_If_Any_State_In_Initial_Active_Hierarchy_Path_Has_Non_Exiting_Initial_State()
        {
            var hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.InitialState = 1;

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            Assert.Throws<InvalidInitialStateException>(() => hsm.Start());
            Assert.IsFalse(hsm.IsStarted);
            state1.DidNotReceive().Enter();
            state2.DidNotReceive().Enter();
            state3.DidNotReceive().Enter();

            hsm.SetInitialStateTo(1, 2);

            Assert.Throws<InvalidInitialStateException>(() => hsm.Start());
            Assert.IsFalse(hsm.IsStarted);
            state1.DidNotReceive().Enter();
            state2.DidNotReceive().Enter();
            state3.DidNotReceive().Enter();

            hsm.SetInitialStateTo(2, 3);

            Assert.DoesNotThrow(() => hsm.Start());
            Assert.IsTrue(hsm.IsStarted);
            state3.Received(1).Enter();
            state1.Received(1).Enter();
            state2.Received(1).Enter();
        }

        /*[Test]
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

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.IsTrue(hsm.AreRelatives(1, 2));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToCreateASubstateRelationWithNotExistingIds()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();

            hsm.AddState(1, state1);

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(1, 2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(2, 1));
            Assert.Throws<StateIdNotAddedException>(() => hsm.SetImmediateSubstateRelation(2, 3));
        }

        [Test]
        public void RemoveSubstateRelations()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.IsTrue(hsm.AreRelatives(1, 2));

            hsm.RemoveImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(1, 3);

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
        public void ReturnCorrespondingValueWhenUserAsksIfStatesAreImmediateRelatives()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);
            
            Assert.IsTrue(hsm.GetParentOf(2) == 1);
            Assert.IsTrue(hsm.GetParentOf(3) == 2);
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
            hsm.SetImmediateSubstateRelation(1, 2);

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
            
            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);
            
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
            
            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(1, 1));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToSetSubstateRelationBetweenParentAndGrandfather()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            
            hsm.SetImmediateSubstateRelation(1, 2);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(2, 1));
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

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(3, 4);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.SetImmediateSubstateRelation(1, 4));
        }

        [Test]
        public void RemoveImmediateSubstateRelationsRelatedToARemovedState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.SetImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(3, 4);

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

            hsm.SetImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);

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

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);
            hsm.SetImmediateSubstateRelation(3, 4);

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
            hsm.SetImmediateSubstateRelation(1, 2);

            state2.Received(1).Enter();

            hsm.SetImmediateSubstateRelation(1, 3);

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

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(1, 3);
            hsm.SetImmediateSubstateRelation(2, 4);

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 4);

            hsm.Start();

            hsm.SetInitialStateTo(1, 3);

            hsm.RemoveState(2);

            state2.Received(1).Exit();
            state4.Received(1).Exit();
            state3.Received(1).Enter();
            Assert.IsNull(hsm.GetParentOf(4));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToAddChildStateWhileParentIsActiveAndChildIsNotInitialState()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.Throws<InvalidOperationException>(() => hsm.SetImmediateSubstateRelation(1, 2));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToAddChildStateWhileParentIsActiveAndHasSomeInvalidInitialStateInHierarchyPath()
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

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.SetImmediateSubstateRelation(2, 3);
            hsm.SetImmediateSubstateRelation(3, 4);

            hsm.Start();

            Assert.Throws<InvalidOperationException>(() => hsm.SetImmediateSubstateRelation(1, 2));

            hsm.SetInitialStateTo(3, 4);

            Assert.DoesNotThrow(() => hsm.SetImmediateSubstateRelation(1, 2));
        }

        [Test]
        public void ThrowAnExceptionIfUserTriesToRemoveStateThatIsInTheActiveHierarchyPathAndSwitchToInitialStateIsInvalid()
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

            hsm.InitialState = 1;

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);
            hsm.SetImmediateSubstateRelation(2, 4);

            hsm.Start();

            hsm.SetInitialStateTo(2, 2);

            Assert.Throws<InvalidOperationException>(() => hsm.RemoveState(3));
            Assert.IsTrue(hsm.ContainsState(3));

            hsm.SetInitialStateTo(2, 4);

            Assert.DoesNotThrow(() => hsm.RemoveState(3));
        }

        [Test]
        public void OnlyRemoveImmediateSubstateRelations()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.RemoveImmediateSubstateRelation(1, 3);

            Assert.IsTrue(hsm.AreRelatives(1, 3));
        }

        [Test]
        public void EnterActiveHierarchyPath()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            Received.InOrder(() =>
            {
                state1.Enter();
                state2.Enter();
                state3.Enter();
            });
        }

        [Test]
        public void UpdateActiveHierarchyPath()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            hsm.Update();

            Received.InOrder(() =>
            {
                state1.Update();
                state2.Update();
                state3.Update();
            });
        }

        [Test]
        public void ExitActiveHierarchyPath()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            hsm.Stop();

            Received.InOrder(() =>
            {
                state3.Exit();
                state2.Exit();
                state1.Exit();
            });
        }

        [Test]
        public void ReturnCorrespondingActiveHierarchyPathEvenWhenStateObjectsAreEntering()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            state1.When(state => state.Enter())
                .Do(callBackInfo =>
                {
                    var activeHierarchyPath = hsm.GetActiveHierarchyPath();

                    Assert.IsTrue(
                        activeHierarchyPath.Contains(1) &&
                        activeHierarchyPath.Contains(2) &&
                        activeHierarchyPath.Contains(3)
                        );
                });

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();
        }

        [Test]
        public void ReturnCorrespondingActiveHierarchyPathEvenWhenStateObjectsAreExiting()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            state1.When(state => state.Exit())
                .Do(callBackInfo =>
                {
                    var activeHierarchyPath = hsm.GetActiveHierarchyPath();

                    Assert.IsTrue(
                        activeHierarchyPath.Contains(1) &&
                        activeHierarchyPath.Contains(2) &&
                        activeHierarchyPath.Contains(3)
                        );
                });

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            hsm.Stop();
        }

        [Test]
        public void ThrowAnAggregateExceptionOfAllExceptionsThrownInEnterMethodsOfObjectStatesWhenStartingWithNoSideEffects()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            var exception1 = new InvalidOperationException();
            var exception2 = new InvalidInitialStateException();

            state1.When(state => state.Enter()).Do(callback => throw exception1);
            state3.When(state => state.Enter()).Do(callback => throw exception2);

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            var aggregateException = Assert.Throws<AggregateException>(() => hsm.Start());

            Assert.IsTrue(aggregateException.InnerExceptions.Contains(exception1) && 
                aggregateException.InnerExceptions.Contains(exception2));

            Assert.IsTrue(hsm.IsStarted);

            state2.Received(1).Enter();
        }

        [Test]
        public void ThrowAnAggregateExceptionOfAllExceptionsThrownInEnterMethodsOfObjectStatesWhenStopingWithNoSideEffects()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            var exception1 = new InvalidOperationException();
            var exception2 = new InvalidInitialStateException();

            state1.When(state => state.Exit()).Do(callback => throw exception1);
            state3.When(state => state.Exit()).Do(callback => throw exception2);

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            var aggregateException = Assert.Throws<AggregateException>(() => hsm.Stop());

            Assert.IsTrue(aggregateException.InnerExceptions.Contains(exception1) &&
                aggregateException.InnerExceptions.Contains(exception2));

            Assert.IsFalse(hsm.IsStarted);

            state2.Received(1).Exit();
        }

        [Test]
        public void ThrowsCorrectlyAnExceptionThrownWhenUpdatingAndCutsTheUpdateFlow()
        {
            HSM<int, int> hsm = new HSM<int, int>();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();
            IState state3 = Substitute.For<IState>();

            var exception1 = new InvalidOperationException();
            var exception2 = new InvalidInitialStateException();

            state2.When(state => state.Update()).Do(callback => throw new StateIdAlreadyAddedException());

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);

            hsm.SetImmediateSubstateRelation(1, 2);
            hsm.SetImmediateSubstateRelation(2, 3);

            hsm.SetInitialStateTo(1, 2);
            hsm.SetInitialStateTo(2, 3);

            hsm.InitialState = 1;

            hsm.Start();

            Assert.Throws<StateIdAlreadyAddedException>(hsm.Update);

            state1.Received(1).Update();
            state3.DidNotReceive().Update();
        }*/
    }
}