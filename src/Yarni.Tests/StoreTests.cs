// ReSharper disable ConvertToConstant.Local

namespace Yarni.Tests
{
    using System.Linq;
    using System.Threading.Tasks;

    using NUnit.Framework;

    [TestFixture]
    public class StoreTests
    {
        [Test]
        public void When_StoreInitializedWithoutState_Should_HaveDefaultState()
        {
            // Act
            var store = new Store<object>(Reducers.Passthrough);

            // Assert
            Assert.That(store.State, Is.Null);
        }

        [Test]
        public void When_StoreInitializedWithInitialState_Should_HaveCorrectInitialState()
        {
            // Act
            var initialState = new object();
            var store = new Store<object>(Reducers.Passthrough, initialState);

            // Assert
            Assert.That(store.State, Is.SameAs(initialState));
        }

        [Test]
        public void When_ActionDispatched_Should_ReduceState()
        {
            // Arrange
            var expectedState = new object();
            var store = new Store<object>(Reducers.Return(expectedState));

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(store.State, Is.SameAs(expectedState));
        }

        [Test]
        public void When_StateChanged_Should_RaiseEvent()
        {
            // Arrange
            var eventWasRaised = false;
            var store = new Store<object>(Reducers.Passthrough);
            store.StateChanged += _ => eventWasRaised = true;

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(eventWasRaised);
        }

        [Test]
        public void When_StateChanged_Should_PassNewStateInEvent()
        {
            // Arrange
            var newState = new object();
            object eventState = null;
            var store = new Store<object>(Reducers.Return(newState));
            store.StateChanged += state => eventState = state;

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(eventState, Is.SameAs(newState));
        }

        [Test]
        public void When_StoreInitializedWithSingleMiddleware_Should_PassActionToMiddleware()
        {
            // Arrange
            var dispatchedAction = new object();
            object actionReceivedByMiddleware = null;

            var middleware = new Middleware<object>(store1 => dispatcher => action => actionReceivedByMiddleware = action);
            var store = new Store<object>(Reducers.Passthrough, null, middleware);

            // Act
            store.Dispatch(dispatchedAction);

            // Assert
            Assert.That(actionReceivedByMiddleware, Is.SameAs(dispatchedAction));
        }

        [Test]
        public void When_StoreInitializedWithMultipleMiddlewares_Should_CallMiddlewaresAndReducerInCorrectOrder()
        {
            // Arrange
            var i = 0;
            var middleware1CalledPosition = 0;
            var middleware2CalledPosition = 0;
            var reducerCalledPosition = 0;
            var middleware1 = new Middleware<object>(
                store1 => next => action =>
                {
                    middleware1CalledPosition = ++i;
                    next(action);
                });
            var middleware2 = new Middleware<object>(
                store2 => next => action =>
                {
                    middleware2CalledPosition = ++i;
                    next(action);
                });
            var store = new Store<object>((state, action) => reducerCalledPosition = ++i, null, middleware2, middleware1);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(middleware1CalledPosition, Is.EqualTo(1));
            Assert.That(middleware2CalledPosition, Is.EqualTo(2));
            Assert.That(reducerCalledPosition, Is.EqualTo(3));
        }

        [Test]
        public void When_MiddlewareCallsNextDispatcher_Expect_ActionReachesReducer()
        {
            // Arrange
            var reducerWasCalled = false;
            var middleware = new Middleware<object>(store1 => next => action => next(action));
            var store = new Store<object>((state, action) => reducerWasCalled = true, null, middleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(reducerWasCalled);
        }

        [Test]
        public void When_MiddlewareDoesNotCallNextDispatcher_Expect_ActionDoesNotReachReducer()
        {
            // Arrange
            var reducerWasCalled = false;
            var middleware = new Middleware<object>(store1 => next => action => { });
            var store = new Store<object>((state, action) => reducerWasCalled = true, null, middleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(reducerWasCalled, Is.False);
        }

        [Test]
        public void When_EventHandlerIsAdded_Should_ImmediatelyInvokeNewEventHandler()
        {
            // Arrange
            var eventHandlerCalledTimes = 0;
            var eventHandler = new StateChangedHandler<object>(state => eventHandlerCalledTimes++);
            var store = new Store<object>((state, action) => state);

            // Act
            store.StateChanged += eventHandler;

            // Assert
            Assert.That(eventHandlerCalledTimes, Is.EqualTo(1));
        }

        [Test]
        public async Task Expect_StoreIsThreadSafe()
        {
            // Arrange
            var store = new Store<int>((state, action) => state + 1, 0);

            // Act
            await Task.WhenAll(Enumerable.Range(0, 10000).Select(_ => Task.Run(() => store.Dispatch(null))));

            // Assert
            Assert.That(store.State, Is.EqualTo(10000));
        }
    }
}