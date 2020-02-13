using NSubstitute;
using NUnit.Framework;
using Paps.StateMachines;
using Paps.StateMachines.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Paps.StateMachines.Extensions.BehaviouralStates;

namespace Tests.WithStructs
{
    public class StateMachineExtensionsForBehaviouralStatesShould
    {
        private class TestStateBehaviour : IStateBehaviour
        {
            public void OnEnter()
            {

            }

            public void OnExit()
            {

            }

            public void OnUpdate()
            {

            }
        }

        private static HierarchicalStateMachine<int, int> NewStateMachine()
        {
            return new HierarchicalStateMachine<int, int>();
        }

        [Test]
        public void Add_Behavioural_States()
        {
            var hsm = NewStateMachine();
            
            var behaviouralState1 = hsm.AddBehaviouralState(1);

            Assert.IsTrue(hsm.ContainsState(1));
            Assert.IsTrue(hsm.GetStateById(1) == behaviouralState1);
            
            IStateBehaviour stateBehaviour1 = Substitute.For<IStateBehaviour>();
            IStateBehaviour stateBehaviour2 = Substitute.For<IStateBehaviour>();

            var behaviouralState2 = hsm.AddBehaviouralState(2, stateBehaviour1, stateBehaviour2);

            Assert.IsTrue(hsm.ContainsState(2));
            Assert.IsTrue(hsm.GetStateById(2) == behaviouralState2);
            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour1));
            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour2));
            Assert.IsTrue(hsm.BehaviourCount() == 2);
            Assert.IsTrue(hsm.BehaviourCountOf(2) == 2);
        }

        [Test]
        public void Add_Behaviours_To_Behavioural_States_After_Creation()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1);

            hsm.AddBehaviourTo(1, stateBehaviour1);

            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour1));
            Assert.IsTrue(hsm.ContainsBehaviourOn(1, stateBehaviour1));
            Assert.IsTrue(hsm.BehaviourCount() == 1);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 1);

            var stateBehaviour2 = hsm.AddBehaviourTo<TestStateBehaviour, int, int>(1);

            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour2));
            Assert.IsTrue(hsm.ContainsBehaviourOn(1, stateBehaviour2));
            Assert.IsTrue(hsm.BehaviourCount() == 2);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 2);

            var stateBehaviour3 = Substitute.For<IStateBehaviour>();
            var stateBehaviour4 = Substitute.For<IStateBehaviour>();

            hsm.AddBehavioursTo(1, stateBehaviour3, stateBehaviour4);

            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour3));
            Assert.IsTrue(hsm.ContainsBehaviourOn(1, stateBehaviour3));
            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour4));
            Assert.IsTrue(hsm.ContainsBehaviourOn(1, stateBehaviour4));
            Assert.IsTrue(hsm.BehaviourCount() == 4);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 4);
        }

        [Test]
        public void Do_Nothing_If_User_Tries_To_Add_The_Same_Behaviour_On_The_Same_State_Twice()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1);

            hsm.AddBehaviourTo(1, stateBehaviour1);
            Assert.DoesNotThrow(() => hsm.AddBehaviourTo(1, stateBehaviour1));
            Assert.IsTrue(hsm.BehaviourCount() == 1);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 1);
        }

        [Test]
        public void Remove_Behaviours_From_Behavioural_States()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();
            var stateBehaviour2 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2);

            hsm.RemoveBehaviourFrom(1, stateBehaviour1);

            Assert.IsTrue(hsm.BehaviourCount() == 1);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 1);
            Assert.IsFalse(hsm.ContainsBehaviour(stateBehaviour1));
            Assert.IsFalse(hsm.ContainsBehaviourOn(1, stateBehaviour1));
            Assert.IsTrue(hsm.ContainsBehaviour(stateBehaviour2));
            Assert.IsTrue(hsm.ContainsBehaviourOn(1, stateBehaviour2));

            hsm.RemoveBehaviourFrom(1, stateBehaviour2);

            Assert.IsTrue(hsm.BehaviourCount() == 0);
            Assert.IsTrue(hsm.BehaviourCountOf(1) == 0);
            Assert.IsFalse(hsm.ContainsBehaviour(stateBehaviour2));
            Assert.IsFalse(hsm.ContainsBehaviourOn(1, stateBehaviour2));
        }

        [Test]
        public void Return_Behaviour_Of_Specific_Type_From_Any_State()
        {
            var hsm = NewStateMachine();

            var stateBehaviour = new TestStateBehaviour();

            hsm.AddBehaviouralState(1, stateBehaviour);

            Assert.AreEqual(stateBehaviour, hsm.GetBehaviour<TestStateBehaviour, int, int>());
        }
        
        [Test]
        public void Return_Behaviour_Of_Specific_Type_From_Specific_State()
        {
            var hsm = NewStateMachine();

            var stateBehaviour = new TestStateBehaviour();

            hsm.AddBehaviouralState(1, stateBehaviour);

            Assert.AreEqual(stateBehaviour, hsm.GetBehaviourOf<TestStateBehaviour, int, int>(1));
        }

        [Test]
        public void Return_Behaviours_Of_Specific_Type_From_All_Behavioural_States()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = new TestStateBehaviour();
            var stateBehaviour2 = new TestStateBehaviour();
            var stateBehaviour3 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2, stateBehaviour3);

            var behaviors = hsm.GetBehaviours<TestStateBehaviour, int, int>();

            Assert.IsTrue(behaviors.Length == 2);
            Assert.IsTrue(behaviors.Contains(stateBehaviour1));
            Assert.IsTrue(behaviors.Contains(stateBehaviour2));
        }

        [Test]
        public void Return_Behaviours_Of_Specific_Type_From_Specific_Behavioural_State()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = new TestStateBehaviour();
            var stateBehaviour2 = new TestStateBehaviour();
            var stateBehaviour3 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2, stateBehaviour3);

            var behaviors = hsm.GetBehavioursOf<TestStateBehaviour, int, int>(1);

            Assert.IsTrue(behaviors.Length == 2);
            Assert.IsTrue(behaviors.Contains(stateBehaviour1));
            Assert.IsTrue(behaviors.Contains(stateBehaviour2));
        }

        [Test]
        public void Iterate_Over_All_Behaviours()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();
            var stateBehaviour2 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2);
            
            var behaviours = new List<IStateBehaviour>();

            behaviours.Add(stateBehaviour1);
            behaviours.Add(stateBehaviour2);

            hsm.ForeachBehaviour(behaviour => 
            {
                behaviours.Remove(behaviour);
                return false;
            });

            Assert.IsTrue(behaviours.Count == 0);
        }

        [Test]
        public void Iterate_Over_All_Behaviours_Of_Specific_State()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();
            var stateBehaviour2 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2);

            var behaviours = new List<IStateBehaviour>();

            behaviours.Add(stateBehaviour1);
            behaviours.Add(stateBehaviour2);

            hsm.ForeachBehaviourOn(1, behaviour =>
            {
                Assert.IsFalse(behaviours.Count == 0);
                behaviours.Remove(behaviour);
                return false;
            });

            Assert.IsTrue(behaviours.Count == 0);
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Add_A_Null_Behaviour()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();

            Assert.Throws<ArgumentNullException>(() => hsm.AddBehaviouralState(1, stateBehaviour1, null));
            
            hsm.AddBehaviouralState(1);

            Assert.Throws<ArgumentNullException>(() => hsm.AddBehaviourTo(1, null));
            Assert.Throws<ArgumentNullException>(() => hsm.AddBehavioursTo(1, stateBehaviour1, null));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Remove_A_Null_Behaviour()
        {
            var hsm = NewStateMachine();

            Assert.Throws<ArgumentNullException>(() => hsm.RemoveBehaviourFrom(1, null));
        }

        [Test]
        public void Throw_An_Exception_If_User_Asks_If_Contains_Behaviour_With_A_Null_Behaviour()
        {
            var hsm = NewStateMachine();

            Assert.Throws<ArgumentNullException>(() => hsm.ContainsBehaviour(null));
            Assert.Throws<ArgumentNullException>(() => hsm.ContainsBehaviourOn(1, null));
        }

        [Test]
        public void Throw_An_Exception_If_User_Tries_To_Iterate_Over_Behaviours_With_A_Null_Delegate()
        {
            var hsm = NewStateMachine();

            Assert.Throws<ArgumentNullException>(() => hsm.ForeachBehaviour(null));
            Assert.Throws<ArgumentNullException>(() => hsm.ForeachBehaviourOn(1, null));
        }

        [Test]
        public void Enter_Update_And_Exit_Behaviours()
        {
            var hsm = NewStateMachine();

            var stateBehaviour1 = Substitute.For<IStateBehaviour>();
            var stateBehaviour2 = Substitute.For<IStateBehaviour>();

            hsm.AddBehaviouralState(1, stateBehaviour1, stateBehaviour2);

            hsm.InitialState = 1;

            hsm.Start();

            stateBehaviour1.Received(1).OnEnter();
            stateBehaviour2.Received(1).OnEnter();

            hsm.Update();

            stateBehaviour1.Received(1).OnUpdate();
            stateBehaviour2.Received(1).OnUpdate();

            hsm.Stop();

            stateBehaviour1.Received(1).OnExit();
            stateBehaviour2.Received(1).OnExit();
        }
    }
}