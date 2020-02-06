using NSubstitute;
using NUnit.Framework;
using Paps.FSM;
using Paps.FSM.HSM;
using System.Linq;
using System;
using System.Collections;

namespace Tests
{
    public class HSMWithStructsShould
    {
        private static void AssertDoesNotContains(object notExpected, ICollection collection)
        {
            try
            {
                Assert.Contains(notExpected, collection);
            }
            catch(AssertionException)
            {
                return;
            }

            throw new AssertionException("expected collection not to contain " + notExpected);
        }

        [Test]
        public void Add_States()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

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
            var hsm = new HSM<int, int>();

            int stateId = 1;

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj1);

            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddState(stateId, stateObj2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Null_State_Object()
        {
            var hsm = new HSM<int, int>();

            int stateId = 1;

            Assert.Throws<ArgumentNullException>(() => hsm.AddState(stateId, null));
        }

        [Test]
        public void Permit_Add_The_Same_State_Object_Twice()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            Assert.DoesNotThrow(() => hsm.AddState(stateId2, stateObj));
        }

        [Test]
        public void Remove_States()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

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
            var hsm = new HSM<int, int>();

            int stateId = 1;

            Assert.DoesNotThrow(() => hsm.RemoveState(stateId));
        }

        [Test]
        public void Establish_Substate_Relations()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;
            int stateId4 = 4;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);
            hsm.AddState(stateId4, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);
            hsm.EstablishSubstateRelation(stateId2, stateId3);

            hsm.EstablishSubstateRelation(stateId1, stateId4);

            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId2));
            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId4));
            Assert.IsFalse(hsm.AreImmediateParentAndChild(stateId1, stateId3));
            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId2, stateId3));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_A_Substate_Relation_Between_State_Ids_That_Were_Not_Added()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            Assert.Throws<StateIdNotAddedException>(() => hsm.EstablishSubstateRelation(stateId1, stateId2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.EstablishSubstateRelation(stateId2, stateId1));

            hsm.AddState(stateId1, stateObj);

            Assert.Throws<StateIdNotAddedException>(() => hsm.EstablishSubstateRelation(stateId1, stateId2));
            Assert.Throws<StateIdNotAddedException>(() => hsm.EstablishSubstateRelation(stateId2, stateId1));

            hsm.AddState(stateId2, stateObj);

            Assert.DoesNotThrow(() => hsm.EstablishSubstateRelation(stateId1, stateId2));
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Establish_A_Substate_Relation_Twice()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);
            
            Assert.DoesNotThrow(() => hsm.EstablishSubstateRelation(stateId1, stateId2));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_With_The_Same_Id_In_Both_Parameters()
        {
            var hsm = new HSM<int, int>();

            int stateId = 1;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId, stateObj);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.EstablishSubstateRelation(stateId, stateId));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_And_Child_State_Already_Has_A_Parent()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.EstablishSubstateRelation(stateId2, stateId3);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.EstablishSubstateRelation(stateId1, stateId3));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Establish_A_Substate_Relation_Between_Parent_And_Grandfather()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            Assert.Throws<InvalidSubstateRelationException>(() => hsm.EstablishSubstateRelation(stateId2, stateId1));
        }

        [Test]
        public void Break_Substate_Relations()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            hsm.BreakSubstateRelation(stateId1, stateId2);

            Assert.That(hsm.AreImmediateParentAndChild(stateId1, stateId2) == false, "Are not immediate relatives");
        }

        [Test]
        public void Return_False_On_AreImmediateParentAndChild_If_States_Are_Grandfather_And_Grandson()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);
            hsm.EstablishSubstateRelation(stateId2, stateId3);

            Assert.IsFalse(hsm.AreImmediateParentAndChild(stateId1, stateId3));
        }

        [Test]
        public void Return_True_On_AreImmediateParentAndChild_If_States_Are_Parent_And_Child()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            Assert.IsTrue(hsm.AreImmediateParentAndChild(stateId1, stateId2));
        }

        [Test]
        public void Return_Orphan_States_As_Roots()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId3, roots);
            AssertDoesNotContains(stateId2, roots);
        }

        [Test]
        public void Return_Null_If_There_Is_No_Orphan_States_When_Asked_For_Roots()
        {
            var hsm = new HSM<int, int>();

            Assert.IsNull(hsm.GetRoots());
        }

        [Test]
        public void Return_Parent_Of_State()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            var parent = hsm.GetParentOf(stateId2);

            Assert.AreEqual(stateId1, parent);
        }

        [Test]
        public void Return_Child_Id_If_Child_Has_No_Parent_When_Asked_For_Parent_Of_State()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var parent = hsm.GetParentOf(stateId1);

            Assert.AreEqual(stateId1, parent);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_The_Parent_Of_A_State_That_Was_Not_Added()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetParentOf(stateId1));
        }

        [Test]
        public void Return_State_Object_By_Id()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var returnedStateObject = hsm.GetStateById(stateId1);

            Assert.AreEqual(stateObj, returnedStateObject);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_The_State_Object_Of_A_State_That_Was_Not_Added()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetStateById(stateId1));
        }

        [Test]
        public void Return_Only_Immediate_Childs_Of_State()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;
            int stateId4 = 4;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);
            hsm.AddState(stateId4, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);
            hsm.EstablishSubstateRelation(stateId1, stateId3);

            hsm.EstablishSubstateRelation(stateId2, stateId4);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.Contains(stateId2, childs);
            Assert.Contains(stateId3, childs);
            AssertDoesNotContains(stateId4, childs);
        }

        [Test]
        public void Return_Null_If_State_Does_Not_Has_Childs_When_Asked_For_Immediate_Childs()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.IsNull(childs);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Ask_For_Childs_Of_A_State_That_Was_Not_Added()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            Assert.Throws<StateIdNotAddedException>(() => hsm.GetImmediateChildsOf(stateId1));
        }

        [Test]
        public void Return_States()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            var states = hsm.GetStates();

            Assert.Contains(stateId1, states);
            Assert.Contains(stateId2, states);
        }

        [Test]
        public void Set_Initial_Child_State_To_A_State()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.SetInitialStateTo(stateId1, stateId2);

            Assert.AreEqual(stateId2, hsm.GetInitialStateOf(stateId1));
        }

        [Test]
        public void Return_Default_Type_Value_If_Initial_State_Is_Not_Set()
        {
            var hsm = new HSM<int, int>();

            Assert.That(hsm.InitialState == default, "Default initial state is default type value");
        }

        [Test]
        public void Return_Default_Type_Value_If_Initial_State_Is_Not_Set_On_A_State()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);

            Assert.That(hsm.GetInitialStateOf(stateId1) == default, "Default initial state of another state is default type value");
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Set_Initial_State_Of_A_State_With_An_Id_That_Was_Not_Added()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            Assert.Throws<StateIdNotAddedException>(() => hsm.SetInitialStateTo(stateId1, stateId2));
        }

        [Test]
        public void Convert_Into_Root_A_Child_State_After_Break_Substate_Relation()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            hsm.BreakSubstateRelation(stateId1, stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId2, roots);
        }

        [Test]
        public void Return_Child_State_Has_No_Parent_After_Its_Substate_Relation_With_Its_Previous_Parent_Was_Broken()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            hsm.BreakSubstateRelation(stateId1, stateId2);

            Assert.That(hsm.GetParentOf(stateId2) == stateId2, "Child has no parent after break substate relation");
        }

        [Test]
        public void Return_Parent_Has_No_Child_If_Their_Substate_Relation_Were_Broken()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);

            hsm.BreakSubstateRelation(stateId1, stateId2);

            var childs = hsm.GetImmediateChildsOf(stateId1);

            Assert.IsNull(childs);
        }

        [Test]
        public void Break_All_Substate_Relations_Related_To_A_State_When_It_Is_Removed()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;
            int stateId3 = 3;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);
            hsm.AddState(stateId3, stateObj);

            hsm.EstablishSubstateRelation(stateId1, stateId2);
            hsm.EstablishSubstateRelation(stateId2, stateId3);

            hsm.RemoveState(stateId2);

            var roots = hsm.GetRoots();

            Assert.Contains(stateId1, roots);
            Assert.Contains(stateId3, roots);
            AssertDoesNotContains(stateId2, roots);
        }
    }
}
