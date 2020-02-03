using NSubstitute;
using NUnit.Framework;
using Paps.FSM;
using Paps.FSM.HSM;
using System.Linq;
using System;

namespace Tests
{
    public class HSMWithClassesShould
    {
        [Test]
        public void Add_States()
        {
            var hsm = new HSM<string, string>();

            string stateId = "1";

            IState state = Substitute.For<IState>();

            hsm.AddState(stateId, state);

            Assert.IsTrue(hsm.ContainsState(stateId));
            Assert.AreEqual(hsm.GetStateById(stateId), state);
        }

        [Test]
        public void Remove_States()
        {
            var hsm = new HSM<string, string>();

            string stateId = "1";

            IState state = Substitute.For<IState>();

            hsm.AddState(stateId, state);

            hsm.RemoveState(stateId);

            Assert.IsFalse(hsm.ContainsState(stateId));
        }
    }
}