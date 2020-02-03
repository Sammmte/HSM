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
    }
}