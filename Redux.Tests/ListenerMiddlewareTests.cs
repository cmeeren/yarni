#pragma warning disable 1998
// ReSharper disable ConvertToConstant.Local

using System;

using NUnit.Framework;

namespace Redux.Tests
{
    [TestFixture]
    public class ListenerMiddlewareTests
    {
        [Test]
        public void When_InitializedWithMultipleListeners_Should_PassActionToAllListeners()
        {
            // Arrange
            var dispatchedAction = new object();
            object listener1Action = null;
            object listener2Action = null;
            var listener1 = new AsyncListener<object>(async (action, state, dispatcher) => listener1Action = action);
            var listener2 = new AsyncListener<object>(async (action, state, dispatcher) => listener2Action = action);
            var listenerMiddleware = new ListenerMiddleware<object>(listener1, listener2);
            var store = new Store<object>((state, action) => state, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(dispatchedAction);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(listener1Action, Is.SameAs(dispatchedAction));
                Assert.That(listener2Action, Is.SameAs(dispatchedAction));
            });
        }

        [Test]
        public void ListenerMiddleware_Should_CallNextBeforeCallingListener()
        {
            // Arrange
            var i = 0;
            var reducerCalledOrder = 0;
            var listenerCalledOrder = 0;
            var listener = new AsyncListener<object>(async (action, state, dispatcher) => listenerCalledOrder = ++i);
            var listenerMiddleware = new ListenerMiddleware<object>(listener);
            var store = new Store<object>((state, action) => reducerCalledOrder = ++i, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(reducerCalledOrder, Is.EqualTo(1));
                Assert.That(listenerCalledOrder, Is.EqualTo(2));
            });
        }

        [Test]
        public void Listener_Should_GetStateBeforeAction()
        {
            // Arrange
            var oldState = new object();
            object newState = null;
            object listenerState = null;
            var listener = new AsyncListener<object>(async (action, state, dispatcher) => listenerState = state);
            var listenerMiddleware = new ListenerMiddleware<object>(listener);
            var store = new Store<object>((state, action) => newState, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(listenerState, Is.SameAs(newState));
        }

        [Test]
        public void Listener_Should_GetWorkingDispatcher()
        {
            // Arrange
            var dispatchedAction1 = new object();
            var dispatchedAction2 = new object();
            var listener = new AsyncListener<object>(async (action, state, dispatcher) =>
            {
                if (action == dispatchedAction1) dispatcher(dispatchedAction2);
            });
            var listenerMiddleware = new ListenerMiddleware<object>(listener);
            var store = new Store<object>((state, action) => action, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(dispatchedAction1);

            // Assert
            Assert.That(store.State, Is.SameAs(dispatchedAction2));
        }

        [Test]
        public void When_ExceptionRaisedInListener_Should_NotBubbleUp()
        {
            // Arrange
            var listener = new AsyncListener<object>(async (action, state, dispatcher) => throw new Exception());
            var listenerMiddleware = new ListenerMiddleware<object>(listener);
            var store = new Store<object>((state, action) => action, null, listenerMiddleware.CreateMiddleware);

            // Act/Assert
            Assert.That(() => store.Dispatch(null), Throws.Nothing);
        }
    }
}