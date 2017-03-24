namespace Yarni
{
    using System;

    /// <summary>
    ///     Provides a middleware that raises an event when an action is received. The action is passed down the
    ///     middleware chain before raising the event. Event handlers are provided with the state before the action
    ///     was passed down.
    /// </summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    public class ListenerMiddleware<TState>
    {
        /// <summary>Occurs when the middleware receives an action.</summary>
        public event Listener<TState> ActionReceived;

        public Func<Dispatcher, Dispatcher> CreateMiddleware(IStore<TState> store)
        {
            return next => action =>
            {
                TState preActionState = store.State;
                next(action);
                this.ActionReceived?.Invoke(action, preActionState, store.Dispatch);
            };
        }
    }
}