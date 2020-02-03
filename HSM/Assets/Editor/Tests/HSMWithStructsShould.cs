using NSubstitute;
using NUnit.Framework;
using Paps.FSM;
using Paps.FSM.HSM;
using System.Linq;
using System;

namespace Tests
{
    public class HSMWithStructsShould
    {
        [Test]
        public void Add_States()
        {
            var hsm = new HSM<int, int>();

            int stateId1 = 1;
            int stateId2 = 2;

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);

            Assert.IsTrue(hsm.ContainsState(stateId1));
            Assert.AreEqual(hsm.GetStateById(stateId1), stateObj1);

            hsm.AddState(stateId2, stateObj2);

            Assert.IsTrue(hsm.ContainsState(stateId2));
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

            Assert.IsFalse(hsm.ContainsState(stateId1));
            Assert.IsTrue(hsm.ContainsState(stateId2));

            hsm.RemoveState(stateId2);

            Assert.IsFalse(hsm.ContainsState(stateId2));
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
    }
}