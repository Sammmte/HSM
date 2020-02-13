using NSubstitute;
using NUnit.Framework;
using Paps.StateMachines;
using Paps.StateMachines.Extensions;
using System;
using System.Linq;
using System.Threading;

namespace Tests.WithStructs
{
    public class StateMachineExtensionsShould
    {
        private static HierarchicalStateMachine<int, int> NewStateMachine()
        {
            return new HierarchicalStateMachine<int, int>();
        }

        [Test]
        public void Get_State()
        {
            var state1 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);

            var shouldBeState1 = hsm.GetState<IState, int, int>();

            Assert.AreEqual(state1, shouldBeState1);
        }

        [Test]
        public void Get_States()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            var states = hsm.GetStates<IState, int, int>();

            Assert.IsTrue(states.Contains(state1) && states.Contains(state2));
        }

        [Test]
        public void Return_Corresponding_Value_When_Asked_If_Contains_By_Reference()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            Assert.IsTrue(hsm.ContainsStateByReference(state1));
        }
        
        [Test]
        public void Add_Timer_State()
        {
            var stateAfter = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(2, stateAfter);
            hsm.AddTimerState(1, 1000, stateId => hsm.Trigger(0));
            hsm.AddTransition(new Transition<int, int>(1, 0, 2));

            hsm.InitialState = 1;

            hsm.Start();

            Thread.Sleep(1200);

            hsm.Update();

            stateAfter.Received().Enter();
        }

        [Test]
        public void Add_Empty()
        {
            var stateAfter = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(2, stateAfter);
            hsm.AddEmpty(1);
            hsm.AddTransition(new Transition<int, int>(1, 0, 2));

            hsm.InitialState = 1;

            hsm.Start();

            hsm.Trigger(0);

            stateAfter.Received().Enter();
        }

        [Test]
        public void Add_With_Events()
        {
            var stateAfter = Substitute.For<IState>();

            var hsm = NewStateMachine();

            Action enterEvent = Substitute.For<Action>();
            Action updateEvent = Substitute.For<Action>();
            Action exitEvent = Substitute.For<Action>();

            hsm.AddState(2, stateAfter);
            hsm.AddWithEvents(1, enterEvent, updateEvent, exitEvent);
            hsm.AddTransition(new Transition<int, int>(1, 0, 2));

            hsm.InitialState = 1;

            hsm.Start();

            enterEvent.Received().Invoke();

            hsm.Update();

            updateEvent.Received().Invoke();

            hsm.Trigger(0);

            stateAfter.Received().Enter();

            exitEvent.Received().Invoke();
        }

        [Test]
        public void Add_Transition_From_Any_State()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();
            var state3 = Substitute.For<IState>();
            var state4 = Substitute.For<IState>();
            var stateTarget = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);
            hsm.AddState(5, stateTarget);
            hsm.FromAny(0, 5);

            Assert.IsTrue(
                hsm.ContainsTransition(new Transition<int, int>(1, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(4, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(5, 0, 5))
                        );
        }

        [Test]
        public void Add_Transition_From_Any_State_Except_Target()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();
            var state3 = Substitute.For<IState>();
            var state4 = Substitute.For<IState>();
            var stateTarget = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, state3);
            hsm.AddState(4, state4);
            hsm.AddState(5, stateTarget);
            hsm.FromAnyExceptTarget(0, 5);

            Assert.IsTrue(
                hsm.ContainsTransition(new Transition<int, int>(1, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 0, 5)) &&
                hsm.ContainsTransition(new Transition<int, int>(4, 0, 5)));

            Assert.IsFalse(hsm.ContainsTransition(new Transition<int, int>(5, 0, 5)));
        }

        [Test]
        public void Return_Transitions_With_Specific_Trigger()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();
            var stateTarget = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddState(3, stateTarget);
            hsm.FromAny(0, 3);
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));

            var transitions = hsm.GetTransitionsWithTrigger(0);

            Assert.IsTrue(HasAnyWithTrigger(transitions, 0));
            Assert.IsFalse(HasAnyWithTrigger(transitions, 1));

            bool HasAnyWithTrigger<TState, TTrigger>(Transition<TState, TTrigger>[] transitionArray, TTrigger trigger)
            {
                foreach (var transition in transitionArray)
                {
                    if (transition.Trigger.Equals(trigger))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Test]
        public void Return_Transitions_With_Specific_State_From()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(2, 1, 1));

            var transitions = hsm.GetTransitionsWithStateFrom(1);

            Assert.IsTrue(HasAnyWithStateFrom(transitions, 1));
            Assert.IsFalse(HasAnyWithStateFrom(transitions, 2));

            bool HasAnyWithStateFrom<TState, TTrigger>(Transition<TState, TTrigger>[] transitionArray, TState stateFrom)
            {
                foreach (var transition in transitionArray)
                {
                    if (transition.StateFrom.Equals(stateFrom))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Test]
        public void Return_Transitions_With_Specific_State_To()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 2));

            var transitions = hsm.GetTransitionsWithStateTo(1);

            Assert.IsTrue(HasAnyWithStateTo(transitions, 1));
            Assert.IsFalse(HasAnyWithStateTo(transitions, 2));

            bool HasAnyWithStateTo<TState, TTrigger>(Transition<TState, TTrigger>[] transitionArray, TState stateTo)
            {
                foreach (var transition in transitionArray)
                {
                    if (transition.StateTo.Equals(stateTo))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Test]
        public void Return_Transition_With_Related_State()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(2, 0, 2));

            var transitions = hsm.GetTransitionsRelatedTo(1);

            Assert.IsTrue(HasAnyWithState(transitions, 1));
            Assert.IsFalse(HasAnyWithState(transitions, 2));

            bool HasAnyWithState<TState, TTrigger>(Transition<TState, TTrigger>[] transitionArray, TState stateId)
            {
                foreach (var transition in transitionArray)
                {
                    if (transition.StateTo.Equals(stateId) || transition.StateFrom.Equals(stateId))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Test]
        public void Return_If_Has_Transition_With_Specific_State_From()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(2, 1, 1));

            Assert.IsTrue(hsm.ContainsTransitionWithStateFrom(1));
            Assert.IsFalse(hsm.ContainsTransitionWithStateFrom(3));
        }

        [Test]
        public void Return_If_Has_Transition_With_Specific_State_To()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 2));

            Assert.IsTrue(hsm.ContainsTransitionWithStateTo(1));
            Assert.IsFalse(hsm.ContainsTransitionWithStateTo(3));
        }

        [Test]
        public void Return_If_Has_Transition_With_Specific_Trigger()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 2));

            Assert.IsTrue(hsm.ContainsTransitionWithTrigger(1));
            Assert.IsFalse(hsm.ContainsTransitionWithTrigger(2));
        }

        [Test]
        public void Return_If_Has_Transition_Related_To_Specific_State()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);
            hsm.AddTransition(new Transition<int, int>(1, 0, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 1));
            hsm.AddTransition(new Transition<int, int>(1, 1, 2));

            Assert.IsTrue(hsm.ContainsTransitionRelatedTo(1));
            Assert.IsTrue(hsm.ContainsTransitionRelatedTo(2));
            Assert.IsFalse(hsm.ContainsTransitionRelatedTo(0));
        }

        [Test]
        public void Remove_All_Transitions()
        {
            var hsm = NewStateMachine();

            hsm.AddEmpty(1);
            hsm.AddEmpty(2);
            hsm.AddEmpty(3);
            hsm.FromAny(0, 1);
            hsm.FromAny(0, 2);
            hsm.FromAny(0, 3);

            hsm.RemoveAllTransitions();

            Assert.IsTrue(hsm.TransitionCount == 0);
        }

        [Test]
        public void Remove_All_Transitions_Related_To_Specific_State()
        {
            var hsm = NewStateMachine();

            hsm.AddEmpty(1);
            hsm.AddEmpty(2);
            hsm.AddEmpty(3);
            hsm.FromAny(0, 1);
            hsm.FromAny(0, 2);
            hsm.FromAny(0, 3);

            hsm.RemoveAllTransitionsRelatedTo(1);

            Assert.IsFalse(hsm.ContainsTransitionRelatedTo(1));
            Assert.IsTrue(hsm.ContainsTransitionRelatedTo(2));
            Assert.IsTrue(hsm.ContainsTransitionRelatedTo(3));
        }

        [Test]
        public void Return_Corresponding_Value_When_User_Asks_If_Contains_All_States()
        {
            var hsm = NewStateMachine();

            hsm.AddEmpty(1);
            hsm.AddEmpty(2);
            hsm.AddEmpty(3);

            Assert.IsTrue(hsm.ContainsAll(1, 2, 3));
            Assert.IsFalse(hsm.ContainsAll(1, 2, 3, 4));
        }

        [Test]
        public void Add_Multiple_States()
        {
            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            var hsm = NewStateMachine();

            hsm.AddStates((1, state1), (2, state2));
        }

        [Test]
        public void Throw_An_Exception_If_A_State_Is_Already_Added_When_Adding_Multiple_States_Without_Side_Effects()
        {
            var hsm = NewStateMachine();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddState(2, state2);

            Assert.Throws<StateIdAlreadyAddedException>(() => hsm.AddStates((1, state1), (2, state2)));
            Assert.IsFalse(hsm.ContainsState(1));
        }

        [Test]
        public void Add_Multiple_Empty_States()
        {
            var hsm = NewStateMachine();

            hsm.AddEmptyStates(1, 2, 3, 4);

            Assert.IsTrue(hsm.ContainsAll(1, 2, 3, 4));
        }

        [Test]
        public void Configure_With_States_As_Triggers_With_No_Reentrant()
        {
            var hsm = NewStateMachine();

            hsm.AddEmptyStates(1, 2, 3);

            hsm.ConfigureWithStatesAsTriggersWithNoReentrant();

            Assert.IsTrue(
                hsm.ContainsTransition(new Transition<int, int>(1, 2, 2)) &&
                hsm.ContainsTransition(new Transition<int, int>(1, 3, 3)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 1, 1)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 3, 3)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 1, 1)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 2, 2))
                );
        }

        [Test]
        public void Configure_With_States_As_Triggers()
        {
            var hsm = NewStateMachine();

            hsm.AddEmptyStates(1, 2, 3);

            hsm.ConfigureWithStatesAsTriggers();

            Assert.IsTrue(
                hsm.ContainsTransition(new Transition<int, int>(1, 1, 1)) &&
                hsm.ContainsTransition(new Transition<int, int>(1, 2, 2)) &&
                hsm.ContainsTransition(new Transition<int, int>(1, 3, 3)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 2, 2)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 1, 1)) &&
                hsm.ContainsTransition(new Transition<int, int>(2, 3, 3)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 3, 3)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 1, 1)) &&
                hsm.ContainsTransition(new Transition<int, int>(3, 2, 2))
                );
        }

        [Test]
        public void Add_Composite_States()
        {
            var hsm = NewStateMachine();

            IState state1 = Substitute.For<IState>();
            IState state2 = Substitute.For<IState>();

            hsm.AddComposite(1, state1, state2);

            Assert.IsTrue(hsm.ContainsState(1));

            hsm.InitialState = 1;

            hsm.Start();

            state1.Received(1).Enter();
            state2.Received(1).Enter();

            hsm.Update();

            state1.Received(1).Update();
            state2.Received(1).Update();

            hsm.Stop();

            state1.Received(1).Exit();
            state2.Received(1).Exit();
        }

        [Test]
        public void Add_Multiple_Event_Handlers()
        {
            var hsm = NewStateMachine();

            IStateEventHandler eventHandler1 = Substitute.For<IStateEventHandler>();
            IStateEventHandler eventHandler2 = Substitute.For<IStateEventHandler>();

            hsm.AddEmpty(1);

            hsm.SubscribeEventHandlersTo(1, eventHandler1, eventHandler2);

            Assert.IsTrue(hsm.HasEventHandlerOn(1, eventHandler1));
            Assert.IsTrue(hsm.HasEventHandlerOn(1, eventHandler2));
        }

        [Test]
        public void Add_And_Remove_Event_Handlers_From_Delegates()
        {
            var hsm = NewStateMachine();

            hsm.AddEmpty(1);

            Func<IEvent, bool> method1 = Substitute.For<Func<IEvent, bool>>();
            Func<IEvent, bool> method2 = Substitute.For<Func<IEvent, bool>>();

            IEvent stateEvent = Substitute.For<IEvent>();

            method1.Invoke(stateEvent).Returns(false);
            method2.Invoke(stateEvent).Returns(true);

            hsm.SubscribeEventHandlerTo(1, method1);
            hsm.SubscribeEventHandlerTo(1, method2);

            Assert.IsTrue(hsm.HasEventHandler(1, method1));
            Assert.IsTrue(hsm.HasEventHandler(1, method2));

            hsm.InitialState = 1;

            hsm.Start();

            Assert.IsTrue(hsm.SendEvent(stateEvent));

            method1.Received(1).Invoke(stateEvent);
            method2.Received(1).Invoke(stateEvent);

            hsm.UnsubscribeEventHandlerFrom(1, method1);
            hsm.UnsubscribeEventHandlerFrom(1, method2);

            Assert.IsFalse(hsm.HasEventHandler(1, method1));
            Assert.IsFalse(hsm.HasEventHandler(1, method2));
        }

        [Test]
        public void Add_Multiple_Event_Handlers_From_Methods()
        {
            var hsm = NewStateMachine();

            hsm.AddEmpty(1);

            Func<IEvent, bool> method1 = Substitute.For<Func<IEvent, bool>>();
            Func<IEvent, bool> method2 = Substitute.For<Func<IEvent, bool>>();

            hsm.SubscribeEventHandlersTo(1, method1, method2);

            Assert.IsTrue(hsm.HasEventHandler(1, method1));
            Assert.IsTrue(hsm.HasEventHandler(1, method2));
        }

        [Test]
        public void Iterate_Over_Transitions()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();
            var state3 = Substitute.For<IState>();
            var state4 = Substitute.For<IState>();

            var transition1 = new Transition<int, int>(1, 2, 3);
            var transition2 = new Transition<int, int>(4, 5, 6);

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(3, state2);

            hsm.AddState(4, state3);
            hsm.AddState(6, state4);

            hsm.AddTransition(transition1);
            hsm.AddTransition(transition2);

            Transition<int, int> item1 = default;
            Transition<int, int> item2 = default;

            int cont = 1;

            hsm.ForeachTransition(
                transition =>
                {
                    if (cont == 1)
                    {
                        item1 = transition;
                    }
                    else
                    {
                        item2 = transition;
                    }

                    cont++;

                    return false;
                }
                );

            Assert.IsTrue(item1.Equals(transition1) && item2.Equals(transition2));
        }

        [Test]
        public void Iterate_Over_States()
        {
            var state1 = Substitute.For<IState>();
            var state2 = Substitute.For<IState>();

            IState item1 = null;
            IState item2 = null;

            var hsm = NewStateMachine();

            hsm.AddState(1, state1);
            hsm.AddState(2, state2);

            int cont = 1;

            hsm.ForeachState(
                state =>
                {
                    if (cont == 1)
                    {
                        item1 = hsm.GetStateById(state);
                    }
                    else
                    {
                        item2 = hsm.GetStateById(state);
                    }

                    cont++;

                    return false;
                }
                );

            Assert.IsTrue(hsm.GetStateById(1) == item1 && hsm.GetStateById(2) == item2);
        }
    }
}
