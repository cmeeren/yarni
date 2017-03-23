#pragma warning disable 1998
// ReSharper disable ConvertToConstant.Local

namespace Yarni.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class ListenerMiddlewareTests
    {
        [Test]
        public void When_InitializedWithMultipleListeners_Should_PassActionToAllListeners()
        {
            // Arrange
            var dispatchedAction = new object();
            object actionReceivedByListener1 = null;
            object actionReceivedByListener2 = null;

            var listener1 = new Listener<object>((action, state, dispatcher) => actionReceivedByListener1 = action);
            var listener2 = new Listener<object>((action, state, dispatcher) => actionReceivedByListener2 = action);
            var listenerMiddleware = new ListenerMiddleware<object>();
            listenerMiddleware.ActionReceived += listener1;
            listenerMiddleware.ActionReceived += listener2;
            var store = new Store<object>(Reducers.Passthrough, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(dispatchedAction);

            // Assert
            Assert.That(actionReceivedByListener1, Is.SameAs(dispatchedAction));
            Assert.That(actionReceivedByListener2, Is.SameAs(dispatchedAction));
        }

        [Test]
        public void ListenerMiddleware_Should_CallNextBeforeCallingListener()
        {
            // Arrange
            var i = 0;
            var reducerCalledPosition = 0;
            var listenerCalledPosition = 0;

            var listener = new Listener<object>((action, state, dispatcher) => listenerCalledPosition = ++i);
            var listenerMiddleware = new ListenerMiddleware<object>();
            listenerMiddleware.ActionReceived += listener;
            var store = new Store<object>((state, action) => reducerCalledPosition = ++i, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(reducerCalledPosition, Is.EqualTo(1));
            Assert.That(listenerCalledPosition, Is.EqualTo(2));
        }

        [Test]
        public void Listener_Should_GetStateBeforeAction()
        {
            // Arrange
            var initialState = new object();
            var newState = new object();
            object stateReceivedByListener = null;

            var listener = new Listener<object>((action, state, dispatcher) => stateReceivedByListener = state);
            var listenerMiddleware = new ListenerMiddleware<object>();
            listenerMiddleware.ActionReceived += listener;
            var store = new Store<object>(Reducers.Return(newState), initialState, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(stateReceivedByListener, Is.SameAs(initialState));
        }

        [Test]
        public void Listener_Should_GetWorkingDispatcher()
        {
            // Arrange
            var originalDispatchedAction = new object();
            var actionDispatchedFromListener = new object();
            object actionReceivedByReducer = null;

            var listener = new Listener<object>(
                (action, state, dispatcher) =>
                {
                    if (action == originalDispatchedAction) dispatcher(actionDispatchedFromListener);
                });
            var listenerMiddleware = new ListenerMiddleware<object>();
            listenerMiddleware.ActionReceived += listener;
            var store = new Store<object>((state, action) => actionReceivedByReducer = action, null, listenerMiddleware.CreateMiddleware);

            // Act
            store.Dispatch(originalDispatchedAction);

            // Assert
            Assert.That(actionReceivedByReducer, Is.SameAs(actionDispatchedFromListener));
        }
    }
}