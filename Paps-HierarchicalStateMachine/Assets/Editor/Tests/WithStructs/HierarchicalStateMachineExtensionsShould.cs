using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Paps.StateMachines;
using Paps.StateMachines.Extensions;
using NSubstitute;

namespace Tests.WithStructs
{
    public class HierarchicalStateMachineExtensionsShould
    {
        private static HierarchicalStateMachine<int, int> NewStateMachine()
        {
            return new HierarchicalStateMachine<int, int>();
        }

        private static Transition<int, int> NewTransition(int stateFrom, int trigger, int stateTo)
        {
            return new Transition<int, int>(stateFrom, trigger, stateTo);
        }

        [Test]
        public void Return_If_State_Is_Root()
        {
            var hsm = NewStateMachine();

            var stateId1 = 1;
            var stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            Assert.That(hsm.IsRoot(stateId1), "State " + stateId1 + " is root");
            Assert.That(hsm.IsRoot(stateId2) == false, "State " + stateId2 + " is not root");
        }

        [Test]
        public void Iterate_Over_States_In_The_Active_Hierarchy_Path()
        {
            var hsm = NewStateMachine();

            var stateId1 = 1;
            var stateId2 = 2;

            var stateObj1 = Substitute.For<IState>();
            var stateObj2 = Substitute.For<IState>();

            hsm.AddState(stateId1, stateObj1);
            hsm.AddState(stateId2, stateObj2);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.Start();

            hsm.ForeachInActiveHierarchyPath(stateId => 
            {
                hsm.GetStateById(stateId).Enter();
                return false;
            });

            stateObj1.Received(2).Enter();
            stateObj2.Received(2).Enter();
        }

        [Test]
        public void Stop_Iteration_Through_Active_Hierarchy_Path_States_If_Any_Delegate_Returns_True()
        {
            var hsm = NewStateMachine();

            var stateId1 = 1;
            var stateId2 = 2;

            var stateObj = Substitute.For<IState>();

            var method = Substitute.For<ReturnTrueToFinishIteration<int>>();

            method.Invoke(stateId1).Returns(true);

            hsm.AddState(stateId1, stateObj);
            hsm.AddState(stateId2, stateObj);

            hsm.AddChildTo(stateId1, stateId2);

            hsm.Start();

            hsm.ForeachInActiveHierarchyPath(method);

            method.Received(1).Invoke(stateId1);
            method.DidNotReceive().Invoke(stateId2);
        }
    }

}