// ReSharper disable ConvertToConstant.Local

using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Yarni.Tests
{
    [TestFixture]
    public class StoreTests
    {
        [Test]
        public void When_PrimitiveStoreInitializedWithoutState_Should_HaveDefaultState()
        {
            // Act
            var store = new Store<bool>((state, action) => true);

            // Assert
            Assert.That(store.State, Is.EqualTo(default(bool)));
        }

        [Test]
        public void When_ObjectStoreInitializedWithoutState_Should_HaveDefaultState()
        {
            // Act
            var store = new Store<object>((state, action) => new object());

            // Assert
            Assert.That(store.State, Is.Null);
        }

        [Test]
        public void When_PrimitiveStoreInitializedWithInitialState_Should_HaveCorrectInitialState()
        {
            // Arrange
            var initialState = true;

            // Act
            var store = new Store<bool>((state, action) => false, initialState);

            // Assert
            Assert.That(store.State, Is.EqualTo(initialState));
        }

        [Test]
        public void When_ObjectStoreInitializedWithInitialState_Should_HaveCorrectInitialState()
        {
            // Act
            var initialState = new object();
            var store = new Store<object>((state, action) => null, initialState);

            // Assert
            Assert.That(store.State, Is.SameAs(initialState));
        }

        [Test]
        public void When_ActionDispatched_Should_ReduceState()
        {
            // Arrange
            var store = new Store<bool>((state, action) => !state, true);
            Assert.That(store.State, Is.EqualTo(true)); // Sanity check

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(store.State, Is.EqualTo(false));
        }

        [Test]
        public void When_StateChanged_Should_NotifySubscribers()
        {
            // Arrange
            var eventWasRaised = false;
            var store = new Store<bool>((state, action) => !state, true);
            store.StateChanged += state => eventWasRaised = true;

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
            object passedState = null;
            var store = new Store<object>((state, action) => newState);
            store.StateChanged += state => passedState = state;

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(passedState, Is.SameAs(newState));
        }

        [Test]
        public void When_StoreInitializedWithSingleMiddleware_Should_PassActionToMiddleware()
        {
            // Arrange
            var dispatchedAction = new object();
            object middlewareAction = null;
            var middleware = new Middleware<object>(store1 => (dispatcher => (action => middlewareAction = action)));
            var store = new Store<object>((state, action) => state, true, middleware);

            // Act
            store.Dispatch(dispatchedAction);

            // Assert
            Assert.That(middlewareAction, Is.SameAs(dispatchedAction));
        }

        [Test]
        public void When_StoreInitializedWithMultipleMiddlewares_Should_CallMiddlewaresAndReducerInCorrectOrder()
        {
            // Arrange
            var i = 0;
            var middleware1Order = 0;
            var middleware2Order = 0;
            var reducerOrder = 0;
            var middleware1 = new Middleware<object>(store1 => (next => (action =>
            {
                middleware1Order = ++i;
                next(action);
            })));
            var middleware2 = new Middleware<object>(store2 => (next => (action =>
            {
                middleware2Order = ++i;
                next(action);
            })));
            var store = new Store<object>((state, action) => reducerOrder = ++i, true, middleware2, middleware1);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(middleware1Order, Is.EqualTo(1));
                Assert.That(middleware2Order, Is.EqualTo(2));
                Assert.That(reducerOrder, Is.EqualTo(3));
            });
        }

        [Test]
        public void When_MiddlewareCallsNextDispatcher_Should_CallReducer()
        {
            // Arrange
            var reducerWasCalled = false;
            var middleware = new Middleware<object>(store1 => (next => (action => next(action))));
            var store = new Store<object>((state, action) => reducerWasCalled = true, true, middleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(reducerWasCalled);
        }

        [Test]
        public void When_MiddlewareDoesNotCallNextDispatcher_Should_NotCallReducer()
        {
            // Arrange
            var reducerWasCalled = false;
            var middleware = new Middleware<object>(store1 => next => action => { });
            var store = new Store<object>((state, action) => reducerWasCalled = true, true, middleware);

            // Act
            store.Dispatch(null);

            // Assert
            Assert.That(reducerWasCalled, Is.False);
        }

        [Test]
        public void When_EventHandlerIsAdded_Should_InvokeNewEventHandler()
        {
            // Arrange
            var eventHandler1CalledTimes = 0;
            var eventHandler2CalledTimes = 0;
            var eventHandler1 = new StateChangedHandler<object>(state => eventHandler1CalledTimes++);
            var eventHandler2 = new StateChangedHandler<object>(state => eventHandler2CalledTimes++);
            var store = new Store<object>((state, action) => state);

            // Act
            store.StateChanged += eventHandler1;
            store.StateChanged += eventHandler2;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(eventHandler1CalledTimes, Is.EqualTo(1));
                Assert.That(eventHandler2CalledTimes, Is.EqualTo(1));
            });
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