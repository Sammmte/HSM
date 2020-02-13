using NSubstitute;
using NUnit.Framework;
using Paps.StateMachines;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.WithClasses
{
    public class HierarchicalStateMachineShould
    {
        private static HierarchicalStateMachine<string, string> NewStateMachine()
        {
            return new HierarchicalStateMachine<string, string>();
        }

        private static Transition<string, string> NewTransition(string stateFrom, string trigger, string stateTo)
        {
            return new Transition<string, string>(stateFrom, trigger, stateTo);
        }

        [Test]
        public void Add_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);

            Assert.That(hsm.ContainsState(stateId1), "Contains state " + stateId1);
            Assert.AreEqual(hsm.GetStateById(stateId1), stateObj1);

            hsm.AddState(stateId2, stateObj2);

            Assert.That(hsm.ContainsState(stateId2), "Contains state " + stateId2);
            Assert.AreEqual(hsm.GetStateById(stateId2), stateObj2);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_The_Same_State_Id_Twice()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj1);

            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddState(stateId, stateObj2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Null_State_Object()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            Assert.Throws<ArgumentNullException>(() => hsm.AddState(stateId, null));
        }

        [Test]
        public void Permit_Add_The_Same_State_Object_Twice()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            Assert.DoesNotThrow(() => hsm.AddState(stateId2, stateObj));
        }

        [Test]
        public void Remove_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.RemoveState(stateId1);

            Assert.That(hsm.ContainsState(stateId1) == false, "Does not contains state " + stateId1);
            Assert.That(hsm.ContainsState(stateId2), "Contains state " + stateId2);

            hsm.RemoveState(stateId2);

            Assert.That(hsm.ContainsState(stateId2) == false, "Does not contains state " + stateId2);
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Remove_State_Ids_That_Were_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            Assert.DoesNotThrow(() => hsm.RemoveState(stateId));
        }

        [Test]
        public void Set_Childs_To_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);
            hsm.AddState(stateId4, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.AddChildTo(stateId1, stateId4);

            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId2));
            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId4));
            Assert.IsFalse(hsm.AreImmediateParentAndChild(stateId1, stateId3));
            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId2, stateId3));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_A_Substate_Relation_Between_State_Ids_That_Were_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            Assert.Throws<StateIdNotAddedException>(() => hsm.AddChildTo(stateId1, stateId2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.AddChildTo(stateId2, stateId1));

            hsm.AddState(stateId1, stateObj);

            Assert.Throws<StateIdNotAddedException>(() => hsm.AddChildTo(stateId1, stateId2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.AddChildTo(stateId2, stateId1));

            hsm.AddState(stateId2, stateObj);

            Assert.DoesNotThrow(() => hsm.AddChildTo(stateId1, stateId2));
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Establish_A_Substate_Relation_Twice()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            Assert.DoesNotThrow(() => hsm.AddChildTo(stateId1, stateId2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_With_The_Same_Id_In_Both_Parameters()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj);

            Assert.Throws<CannotAddChildException>(() => hsm.AddChildTo(stateId, stateId));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_And_Child_State_Already_Has_A_Parent()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId2, stateId3);

            Assert.Throws<CannotAddChildException>(() => hsm.AddChildTo(stateId1, stateId3));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_Between_Parent_And_Grandfather()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            Assert.Throws<CannotAddChildException>(() => hsm.AddChildTo(stateId2, stateId1));
        }

        [Test]
        public void Break_Substate_Relations()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.RemoveChildFromParent(stateId2);

            Assert.That(hsm.AreImmediateParentAndChild(stateId1, stateId2) == false, "Are not immediate relatives");
        }

        [Test]
        public void Return_False_On_AreImmediateParentAndChild_If_States_Are_Grandfather_And_Grandson()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            Assert.IsFalse(hsm.AreImmediateParentAndChild(stateId1, stateId3));
        }

        [Test]
        public void Return_True_On_AreImmediateParentAndChild_If_States_Are_Parent_And_Child()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId2));
        }

        [Test]
        public void Return_Orphan_States_As_Roots()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId3, roots);
            AssertExtensions.DoesNotContains(stateId2, roots);
        }

        [Test]
        public void Return_Null_If_There_Is_No_Orphan_States_When_Asked_For_Roots()
        {
            var hsm = NewStateMachine();

            Assert.IsNull(hsm.GetRoots());
        }

        [Test]
        public void Return_Parent_Of_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            var parent = hsm.GetParentOf(stateId2);

            Assert.AreEqual(stateId1, parent);
        }

        [Test]
        public void Return_Child_Id_If_Child_Has_No_Parent_When_Asked_For_Parent_Of_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var parent = hsm.GetParentOf(stateId1);

            Assert.AreEqual(stateId1, parent);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_The_Parent_Of_A_State_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetParentOf(stateId1));
        }

        [Test]
        public void Return_State_Object_By_Id()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var returnedStateObject = hsm.GetStateById(stateId1);

            Assert.AreEqual(stateObj, returnedStateObject);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_The_State_Object_Of_A_State_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetStateById(stateId1));
        }

        [Test]
        public void Return_Only_Immediate_Childs_Of_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);
            hsm.AddState(stateId4, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId1, stateId3);

            hsm.AddChildTo(stateId2, stateId4);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.Contains(stateId2, childs);
            Assert.Contains(stateId3, childs);
            AssertExtensions.DoesNotContains(stateId4, childs);
        }

        [Test]
        public void Return_Null_If_State_Does_Not_Has_Childs_When_Asked_For_Immediate_Childs()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.IsNull(childs);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_Childs_Of_A_State_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetImmediateChildsOf(stateId1));
        }

        [Test]
        public void Return_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            var states = hsm.GetStates();

            Assert.Contains(stateId1, states);
            Assert.Contains(stateId2, states);
        }

        [Test]
        public void Return_Null_If_User_Asks_For_All_States_And_There_Is_None()
        {
            var hsm = NewStateMachine();

            var states = hsm.GetStates();

            Assert.IsNull(states);
        }

        [Test]
        public void Set_Initial_Child_State_To_A_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId1, stateId3);

            hsm.SetInitialStateTo(stateId1, stateId3);

            Assert.AreEqual(stateId3, hsm.GetInitialStateOf(stateId1));
        }

        [Test]
        public void Return_Default_Type_Value_If_Initial_State_Is_Not_Set()
        {
            var hsm = NewStateMachine();

            Assert.That(hsm.InitialState == default, "Default initial state is default type value");
        }

        [Test]
        public void Return_Default_Type_Value_If_Initial_State_Is_Not_Set_On_A_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            Assert.That(hsm.GetInitialStateOf(stateId1) == default, "Default initial state of another state is default type value");
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Initial_State_Of_A_State_With_An_Id_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetInitialStateTo(stateId1, stateId2));
        }

        [Test]
        public void Convert_Into_Root_A_Child_State_After_Break_Substate_Relation()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.RemoveChildFromParent(stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId2, roots);
        }

        [Test]
        public void Return_Child_State_Has_No_Parent_After_Its_Substate_Relation_With_Its_Previous_Parent_Was_Broken()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.RemoveChildFromParent(stateId2);

            Assert.That(hsm.GetParentOf(stateId2) == stateId2, "Child has no parent after break substate relation");
        }

        [Test]
        public void Return_Parent_Has_No_Child_If_Their_Substate_Relation_Were_Broken()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.RemoveChildFromParent(stateId2);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.IsNull(childs);
        }

        [Test]
        public void Remove_All_Child_From_A_State_When_It_Is_Removed()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.RemoveState(stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId3, roots);
            AssertExtensions.DoesNotContains(stateId2, roots);
        }

        [Test]
        public void Start_And_Enter_Initial_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            Assert.DoesNotThrow(() => hsm.Start());
            Assert.That(hsm.IsStarted, "HSM is started");

            stateObj.Received(hsm.GetActiveHierarchyPath().Count()).Enter();
        }

        [Test]
        public void Enter_States_From_Root_To_Leaf()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            Received.InOrder(() => {
                stateObj1.Enter();
                stateObj2.Enter();
                stateObj3.Enter();
            });
        }

        [Test]
        public void Return_Active_Hierarchy_Path()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            var activeHierarchyPath = hsm.GetActiveHierarchyPath();

            AssertExtensions.Contains(stateId1, activeHierarchyPath);
            AssertExtensions.Contains(stateId2, activeHierarchyPath);
            AssertExtensions.Contains(stateId3, activeHierarchyPath);
        }

        [Test]
        public void Stop_And_Exit_States_In_The_Active_Hierarchy_Path()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            int activeHierarchyPathCount = hsm.GetActiveHierarchyPath().Count();

            hsm.Stop();

            Assert.That(hsm.IsStarted == false, "Is stopped");

            stateObj.Received(activeHierarchyPathCount).Exit();
        }

        [Test]
        public void Exit_States_From_Leaf_To_Root()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            hsm.Stop();

            Received.InOrder(() => {
                stateObj3.Exit();
                stateObj2.Exit();
                stateObj1.Exit();
            });
        }

        [Test]
        public void Update_States_In_The_Active_Hierarchy_Path()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            hsm.Update();

            stateObj.Received(hsm.GetActiveHierarchyPath().Count()).Update();
        }

        [Test]
        public void Update_States_From_Root_To_Leaf()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);
            hsm.SetInitialStateTo(stateId2, stateId3);

            hsm.Start();

            hsm.Update();

            Received.InOrder(() => {
                stateObj1.Update();
                stateObj2.Update();
                stateObj3.Update();
            });
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Update_And_State_Machine_Was_Not_Started()
        {
            var hsm = NewStateMachine();

            Assert.Throws<StateMachineNotStartedException>(hsm.Update);
        }

        [Test]
        public void Set_Initial_State_Automatically_When_The_First_State_Is_Added()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj);

            Assert.AreEqual(stateId, hsm.InitialState);
        }

        [Test]
        public void Reset_To_Default_Type_Value_After_The_Last_State_Is_Removed()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj);

            hsm.RemoveState(stateId);

            Assert.AreEqual(default(string), hsm.InitialState);
        }

        [Test]
        public void Set_Initial_State_Automatically_When_The_First_Child_State_Is_Added_To_A_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            Assert.AreEqual(stateId2, hsm.GetInitialStateOf(stateId1));
        }

        [Test]
        public void Reset_To_Default_Type_Value_After_The_Last_Child_State_Is_Remove_From_A_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.RemoveChildFromParent(stateId2);

            Assert.AreEqual(default(string), hsm.GetInitialStateOf(stateId1));
        }

        [Test]
        public void Return_If_Is_In_A_Specific_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.InitialState = stateId1;

            hsm.SetInitialStateTo(stateId1, stateId2);

            hsm.Start();

            Assert.That(hsm.IsInState(stateId1), "Is in " + stateId1);
            Assert.That(hsm.IsInState(stateId2), "Is in " + stateId2);
            Assert.That(hsm.IsInState(stateId3) == false, "Is not in " + stateId3);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Start_The_State_Machine_When_It_Is_Already_Started()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            hsm.Start();

            Assert.Throws<StateMachineStartedException>(() => hsm.Start());
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Start_The_State_Machine_With_No_State_Added()
        {
            var hsm = NewStateMachine();

            Assert.Throws<EmptyStateMachineException>(() => hsm.Start());
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Stop_The_State_Machine_Without_Been_Started()
        {
            var hsm = NewStateMachine();

            Assert.DoesNotThrow(() => hsm.Stop());
        }

        [Test]
        public void Add_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            Assert.That(hsm.ContainsTransition(transition), "Contains transition");
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Add_The_Same_Transition_Twice()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            Assert.DoesNotThrow(() => hsm.AddTransition(transition));
            Assert.That(hsm.TransitionCount == 1, "Only has 1 transition");
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_Transition_With_State_Ids_That_Were_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            Assert.Throws<StateIdNotAddedException>(() => hsm.AddTransition(transition));

            hsm.AddState(stateId1, stateObj);

            Assert.Throws<StateIdNotAddedException>(() => hsm.AddTransition(transition));

            hsm.RemoveState(stateId1);

            hsm.AddState(stateId2, stateObj);

            Assert.Throws<StateIdNotAddedException>(() => hsm.AddTransition(transition));
        }

        [Test]
        public void Remove_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.RemoveTransition(transition);

            Assert.That(hsm.ContainsTransition(transition) == false, "Does not contains transition");
        }

        [Test]
        public void Return_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId1, trigger, stateId2);
            var transition2 = NewTransition(stateId2, trigger, stateId1);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            var transitions = hsm.GetTransitions();

            Assert.Contains(transition1, transitions);
            Assert.Contains(transition2, transitions);
        }

        [Test]
        public void Return_Null_If_User_Asks_For_All_Transitions_And_There_Is_None()
        {
            var hsm = NewStateMachine();

            Assert.IsNull(hsm.GetTransitions());
        }

        [Test]
        public void Add_Guard_Conditions_To_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            Assert.That(hsm.ContainsGuardConditionOn(transition, guardCondition), "Transition contains guard condition");
        }

        [Test]
        public void Remove_Guard_Conditions_From_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            hsm.RemoveGuardConditionFrom(transition, guardCondition);

            Assert.That(hsm.ContainsGuardConditionOn(transition, guardCondition) == false, "Transition does not contains guard condition");
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Guard_Condition_To_State_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            Assert.Throws<TransitionNotAddedException>(() => hsm.AddGuardConditionTo(transition, guardCondition));
        }

        [Test]
        public void Throw_An_Exception_If_User_Asks_If_State_Machine_Contains_Guard_Condition_On_A_Transition_That_Was_Not_Added()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            Assert.Throws<TransitionNotAddedException>(() => hsm.ContainsGuardConditionOn(transition, guardCondition));
        }

        [Test]
        public void Return_Transitions_Of_A_Specific_Transition()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId1, trigger, stateId2);
            var transition2 = NewTransition(stateId2, trigger, stateId1);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();
            var guardCondition3 = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            hsm.AddGuardConditionTo(transition1, guardCondition1);
            hsm.AddGuardConditionTo(transition1, guardCondition2);

            hsm.AddGuardConditionTo(transition2, guardCondition3);

            var guardConditions = hsm.GetGuardConditionsOf(transition1);

            Assert.Contains(guardCondition1, guardConditions);
            Assert.Contains(guardCondition2, guardConditions);
            AssertExtensions.DoesNotContains(guardCondition3, guardConditions);
        }

        [Test]
        public void Transition_With_Plain_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddTransition(transition);

            hsm.Start();

            hsm.Trigger(trigger);

            Received.InOrder(() =>
            {
                stateObj1.Received(1).Exit();
                stateObj2.Received(1).Enter();
            });

            Assert.That(hsm.IsInState(stateId2), "Is in state " + stateId2);

            var activeHierarchyPath = hsm.GetActiveHierarchyPath();

            AssertExtensions.Contains(stateId2, activeHierarchyPath);
            AssertExtensions.DoesNotContains(stateId1, activeHierarchyPath);
        }

        [Test]
        public void Transition_With_Hierarchical_States()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";
            var stateId5 = "5";
            var stateId6 = "6";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();
            var stateObj4 = Substitute.For<IState>();
            var stateObj5 = Substitute.For<IState>();
            var stateObj6 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId3, trigger, stateId5);

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);
            hsm.AddState(stateId4, stateObj4);
            hsm.AddState(stateId5, stateObj5);
            hsm.AddState(stateId6, stateObj6);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.AddChildTo(stateId2, stateId3);
            hsm.AddChildTo(stateId2, stateId5);

            hsm.AddChildTo(stateId3, stateId4);

            hsm.AddChildTo(stateId5, stateId6);

            hsm.AddTransition(transition);

            hsm.Start();

            hsm.Trigger(trigger);

            stateObj1.DidNotReceive().Exit();
            stateObj2.DidNotReceive().Exit();

            Received.InOrder(() =>
            {
                stateObj4.Exit();
                stateObj3.Exit();

                stateObj5.Enter();
                stateObj6.Enter();
            });

            Assert.That(hsm.IsInState(stateId1), "Is in state " + stateId1);
            Assert.That(hsm.IsInState(stateId2), "Is in state " + stateId2);
            Assert.That(hsm.IsInState(stateId5), "Is in state " + stateId5);
            Assert.That(hsm.IsInState(stateId6), "Is in state " + stateId6);

            var activeHierarchyPath = hsm.GetActiveHierarchyPath();

            AssertExtensions.Contains(stateId1, activeHierarchyPath);
            AssertExtensions.Contains(stateId2, activeHierarchyPath);
            AssertExtensions.Contains(stateId5, activeHierarchyPath);
            AssertExtensions.Contains(stateId6, activeHierarchyPath);
        }

        [Test]
        public void Transition_Queued()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();
            var stateObj4 = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId2, trigger, stateId3);
            var transition2 = NewTransition(stateId3, trigger, stateId2);

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);
            hsm.AddState(stateId4, stateObj4);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId1, stateId3);

            hsm.AddChildTo(stateId2, stateId4);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            hsm.Start();

            bool shouldTrigger = true;

            stateObj2.When(state => state.Exit()).Do(callback =>
            {
                if (shouldTrigger)
                {
                    hsm.Trigger(trigger);
                    shouldTrigger = false;
                }
            });

            hsm.Trigger(trigger);

            Assert.That(hsm.IsInState(stateId2), "State machine has transitioned 2 times");

            Received.InOrder(() =>
            {
                stateObj1.Enter();
                stateObj2.Enter();
                stateObj4.Enter();
                stateObj4.Exit();
                stateObj2.Exit();
                stateObj3.Enter();
                stateObj3.Exit();
                stateObj2.Enter();
                stateObj4.Enter();
            });
        }

        [Test]
        public void Do_Not_Transition_If_Switching_To_Target_State_Is_Invalid()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();
            var stateObj3 = Substitute.For<IState>();
            var stateObj4 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId4, trigger, stateId3);

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj3);
            hsm.AddState(stateId4, stateObj4);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId1, stateId3);

            hsm.AddChildTo(stateId2, stateId4);

            hsm.AddTransition(transition);

            hsm.Start();

            hsm.Trigger(trigger);

            Assert.That(hsm.IsInState(stateId4), "Did not transitioned");
        }

        [Test]
        public void Call_Guard_Conditions_When_Evaluating_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            hsm.Start();

            hsm.Trigger(trigger);

            guardCondition.Received(1).IsValid();
        }

        [Test]
        public void Deny_Transition_If_Any_Guard_Condition_Returned_False()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();

            guardCondition1.IsValid().Returns(false);
            guardCondition2.IsValid().Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition1);
            hsm.AddGuardConditionTo(transition, guardCondition2);

            hsm.Start();

            hsm.Trigger(trigger);

            guardCondition1.Received(1).IsValid();
            guardCondition2.DidNotReceive().IsValid();

            Assert.That(hsm.IsInState(stateId1), "Did not transitioned");
        }

        [Test]
        public void Permit_Transition_If_All_Guard_Conditions_Returned_True()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();

            guardCondition1.IsValid().Returns(true);
            guardCondition2.IsValid().Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition1);
            hsm.AddGuardConditionTo(transition, guardCondition2);

            hsm.Start();

            hsm.Trigger(trigger);

            guardCondition1.Received(1).IsValid();
            guardCondition2.Received(1).IsValid();

            Assert.That(hsm.IsInState(stateId2), "Did transitioned");
        }

        [Test]
        public void Choose_Valid_Transition_For_Switching_If_There_Is_More_Than_One_With_Same_Source_And_Trigger_With_Different_Target()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId1, trigger, stateId2);
            var transition2 = NewTransition(stateId1, trigger, stateId3);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();

            guardCondition1.IsValid().Returns(false);
            guardCondition2.IsValid().Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            hsm.AddGuardConditionTo(transition1, guardCondition1);
            hsm.AddGuardConditionTo(transition2, guardCondition2);

            hsm.Start();

            hsm.Trigger(trigger);

            guardCondition1.Received(1).IsValid();
            guardCondition2.Received(1).IsValid();

            Assert.That(hsm.IsInState(stateId3), "Transitioned to " + stateId3);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Trigger_When_State_Machine_Is_Not_Started()
        {
            var hsm = NewStateMachine();

            var trigger = "0";

            Assert.Throws<StateMachineNotStartedException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Throw_An_Exception_If_There_Is_More_Than_One_Valid_Transition_For_The_Same_Target_And_Trigger()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId1, trigger, stateId2);
            var transition2 = NewTransition(stateId1, trigger, stateId3);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();

            guardCondition1.IsValid().Returns(true);
            guardCondition2.IsValid().Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            hsm.AddGuardConditionTo(transition1, guardCondition1);
            hsm.AddGuardConditionTo(transition2, guardCondition2);

            hsm.Start();

            Assert.Throws<MultipleValidTransitionsFromSameStateException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Call_On_Before_Active_Hierarchy_Path_Changes_Event_Before_Previous_States_Exit()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var eventHandler = Substitute.For<Action>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddTransition(transition);

            hsm.OnBeforeActiveHierarchyPathChanges += eventHandler;

            hsm.Start();

            hsm.Trigger(trigger);

            Received.InOrder(() =>
            {
                eventHandler.Invoke();
                stateObj1.Exit();
            });
        }

        [Test]
        public void Call_On_Active_Hierarchy_Path_Changed_Event_After_Previous_States_Exit_And_Before_Next_States_Enter()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var eventHandler = Substitute.For<Action>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddTransition(transition);

            hsm.OnActiveHierarchyPathChanged += eventHandler;

            hsm.Start();

            hsm.Trigger(trigger);

            Received.InOrder(() =>
            {
                stateObj1.Exit();
                eventHandler.Invoke();
                stateObj2.Enter();
            });
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Transitions_While_Evaluating_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition1 = NewTransition(stateId1, trigger, stateId2);
            var transition2 = NewTransition(stateId2, trigger, stateId1);

            var guardCondition = Substitute.For<IGuardCondition>();

            guardCondition.When(g => g.IsValid()).Do(callback => hsm.AddTransition(transition2));

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition1);

            hsm.AddGuardConditionTo(transition1, guardCondition);

            hsm.Start();

            Assert.Throws<StateMachineEvaluatingTransitionsException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_Guard_Conditions_While_Evaluating_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition1 = Substitute.For<IGuardCondition>();
            var guardCondition2 = Substitute.For<IGuardCondition>();

            guardCondition1.When(g => g.IsValid()).Do(callback => hsm.AddGuardConditionTo(transition, guardCondition2));

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition1);

            hsm.Start();

            Assert.Throws<StateMachineEvaluatingTransitionsException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Remove_States_While_Evaluating_Transitions()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            guardCondition.When(g => g.IsValid()).Do(callback => hsm.RemoveState(stateId1));

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            hsm.Start();

            Assert.Throws<StateMachineEvaluatingTransitionsException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Trigger_While_Stopping()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            stateObj.When(state => state.Exit()).Do(callback => hsm.Trigger(trigger));

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.Start();

            Assert.Throws<StateMachineStoppingException>(() => hsm.Stop());
        }

        [Test]
        public void Reenter_State_If_Source_Is_Equals_To_Target()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId1);

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.AddTransition(transition);

            hsm.Start();

            hsm.Trigger(trigger);

            stateObj1.Received(2).Enter();
            stateObj1.Received(1).Exit();

            stateObj2.Received(2).Enter();
            stateObj2.Received(1).Exit();
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Remove_A_State_That_Is_In_The_Active_Hierarchy_Path()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.Start();

            Assert.Throws<InvalidOperationException>(() => hsm.RemoveState(stateId1));
            Assert.Throws<InvalidOperationException>(() => hsm.RemoveState(stateId2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Remove_The_Next_State_Or_An_Initial_Child_State_At_Any_Level_While_In_Transition()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";
            var stateId4 = "4";
            var stateId5 = "5";

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            stateObj1.When(state => state.Exit()).Do(callback => hsm.RemoveState(stateId4));

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);
            hsm.AddState(stateId3, stateObj2);
            hsm.AddState(stateId4, stateObj2);
            hsm.AddState(stateId5, stateObj2);

            hsm.AddChildTo(stateId2, stateId3);
            hsm.AddChildTo(stateId3, stateId4);

            hsm.AddChildTo(stateId3, stateId5);

            hsm.AddTransition(transition);

            hsm.Start();

            Assert.Throws<ProtectedStateException>(() => hsm.Trigger(trigger));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Remove_An_Active_Child_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.Start();

            Assert.Throws<InvalidOperationException>(() => hsm.RemoveChildFromParent(stateId2));
            Assert.That(hsm.AreImmediateParentAndChild(stateId1, stateId2), "Still related");
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_An_Active_State_As_Child_To_Inactive_One()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.Start();

            Assert.Throws<CannotAddChildException>(() => hsm.AddChildTo(stateId2, stateId1));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Child_State_To_An_Active_Parent_That_Has_No_Childs()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.Start();

            Assert.Throws<CannotAddChildException>(() => hsm.AddChildTo(stateId1, stateId2));
        }

        [Test]
        public void Permit_Add_Child_State_If_Parent_Is_Active_And_Has_At_Least_One_Child()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.Start();

            Assert.DoesNotThrow(() => hsm.AddChildTo(stateId1, stateId3));
        }

        [Test]
        public void Permit_Remove_Inactive_Child_Event_If_Its_Parent_Is_Active()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId1, stateId3);

            hsm.Start();

            Assert.DoesNotThrow(() => hsm.RemoveChildFromParent(stateId3));
        }

        [Test]
        public void Remove_All_Transitions_Related_To_A_Removed_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.RemoveState(stateId2);

            Assert.That(hsm.ContainsTransition(transition) == false, "Does not contains transition related to a removed state");
        }

        [Test]
        public void Remove_All_Guard_Conditions_Related_To_A_Removed_Transition()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            hsm.RemoveTransition(transition);

            Assert.Throws<TransitionNotAddedException>(() => hsm.ContainsGuardConditionOn(transition, guardCondition));

            hsm.AddTransition(transition);

            Assert.DoesNotThrow(() => hsm.AddGuardConditionTo(transition, guardCondition));

            Assert.That(hsm.GetGuardConditionsOf(transition).Count() == 1, "Contains just one guard condition");
        }

        [Test]
        public void Remove_All_Transitions_And_Guard_Conditions_Of_A_Removed_State()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";

            var stateObj = Substitute.For<IState>();

            var trigger = "0";

            var transition = NewTransition(stateId1, trigger, stateId2);

            var guardCondition = Substitute.For<IGuardCondition>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddTransition(transition);

            hsm.AddGuardConditionTo(transition, guardCondition);

            hsm.RemoveState(stateId1);

            Assert.That(hsm.ContainsTransition(transition) == false, "Does not contains transition");

            hsm.AddState(stateId1, stateObj);
            hsm.AddTransition(transition);

            Assert.DoesNotThrow(() => hsm.AddGuardConditionTo(transition, guardCondition));

            Assert.That(hsm.GetGuardConditionsOf(transition).Count() == 1, "Contains just one guard condition");
        }

        [Test]
        public void Subscribe_Event_Handlers_To_States()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            var eventHandler = Substitute.For<IStateEventHandler>();

            hsm.AddState(stateId, stateObj);

            hsm.SubscribeEventHandlerTo(stateId, eventHandler);

            Assert.That(hsm.HasEventHandlerOn(stateId, eventHandler), "State " + stateId + " has event handler");
        }

        [Test]
        public void Unsubscribe_Event_Handlers_From_States()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            var eventHandler = Substitute.For<IStateEventHandler>();

            hsm.AddState(stateId, stateObj);

            hsm.SubscribeEventHandlerTo(stateId, eventHandler);

            hsm.UnsubscribeEventHandlerFrom(stateId, eventHandler);

            Assert.That(hsm.HasEventHandlerOn(stateId, eventHandler) == false, "State " + stateId + " does not has event handler");
        }

        [Test]
        public void Return_Event_Handlers_Of_A_Specific_State()
        {
            var hsm = NewStateMachine();

            var stateId = "1";

            var stateObj = Substitute.For<IState>();

            var eventHandler1 = Substitute.For<IStateEventHandler>();
            var eventHandler2 = Substitute.For<IStateEventHandler>();
            var eventHandler3 = Substitute.For<IStateEventHandler>();

            hsm.AddState(stateId, stateObj);

            hsm.SubscribeEventHandlerTo(stateId, eventHandler1);
            hsm.SubscribeEventHandlerTo(stateId, eventHandler2);

            var eventHandlers = hsm.GetEventHandlersOf(stateId);

            Assert.Contains(eventHandler1, eventHandlers);
            Assert.Contains(eventHandler2, eventHandlers);
            AssertExtensions.DoesNotContains(eventHandler3, eventHandlers);
        }

        [Test]
        public void Send_Event_From_Leaf_To_Root()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            var eventHandler1 = Substitute.For<IStateEventHandler>();
            var eventHandler2 = Substitute.For<IStateEventHandler>();
            var eventHandler3 = Substitute.For<IStateEventHandler>();

            var eventObj = Substitute.For<IEvent>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.SubscribeEventHandlerTo(stateId1, eventHandler1);
            hsm.SubscribeEventHandlerTo(stateId2, eventHandler2);
            hsm.SubscribeEventHandlerTo(stateId3, eventHandler3);

            hsm.Start();

            hsm.SendEvent(eventObj);

            Received.InOrder(() =>
            {
                eventHandler3.HandleEvent(eventObj);
                eventHandler2.HandleEvent(eventObj);
                eventHandler1.HandleEvent(eventObj);
            });
        }

        [Test]
        public void Return_True_On_Send_Event_If_Any_Event_Handler_Returns_True()
        {
            var hsm = NewStateMachine();

            var stateId1 = "1";
            var stateId2 = "2";
            var stateId3 = "3";

            var stateObj = Substitute.For<IState>();

            var eventHandler1 = Substitute.For<IStateEventHandler>();
            var eventHandler2 = Substitute.For<IStateEventHandler>();
            var eventHandler3 = Substitute.For<IStateEventHandler>();

            var eventObj = Substitute.For<IEvent>();

            eventHandler2.HandleEvent(eventObj).Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.AddChildTo(stateId1, stateId2);
            hsm.AddChildTo(stateId2, stateId3);

            hsm.SubscribeEventHandlerTo(stateId1, eventHandler1);
            hsm.SubscribeEventHandlerTo(stateId2, eventHandler2);
            hsm.SubscribeEventHandlerTo(stateId3, eventHandler3);

            hsm.Start();

            hsm.SendEvent(eventObj);

            eventHandler3.Received(1).HandleEvent(eventObj);
            eventHandler2.Received(1).HandleEvent(eventObj);
            eventHandler1.DidNotReceive().HandleEvent(eventObj);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Send_An_Event_And_State_Machine_Is_Not_Started()
        {
            var hsm = NewStateMachine();

            var eventObj = Substitute.For<IEvent>();

            Assert.Throws<StateMachineNotStartedException>(() => hsm.SendEvent(eventObj));
        }
    }
}
