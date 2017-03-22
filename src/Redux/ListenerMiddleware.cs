namespace Yarni
{
    using System;

    public class ListenerMiddleware<TState>
    {
        private readonly AsyncListener<TState>[] listeners;

        public ListenerMiddleware(params AsyncListener<TState>[] listeners)
        {
            this.listeners = listeners;
        }

        public Func<Dispatcher, Dispatcher> CreateMiddleware(IStore<TState> store)
        {
            return next => action =>
            {
                TState preActionState = store.State;
                next(action);
                foreach (AsyncListener<TState> listener in this.listeners)
                {
                    listener(action, preActionState, store.Dispatch);
                }
            };
        }
    }
}