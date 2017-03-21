using JetBrains.Annotations;

namespace Redux
{
    /// <inheritdoc />
    public class Store<TState> : IStore<TState>
    {
        [CanBeNull] private StateChangedHandler<TState> stateChangedHandler;

        /// <inheritdoc />
        public event StateChangedHandler<TState> StateChanged
        {
            add
            {
                stateChangedHandler += value;
                value.Invoke(State);
            }
            // ReSharper disable once DelegateSubtraction (OK since we're removing a single value)
            remove => stateChangedHandler -= value;
        }

        private readonly object syncRoot = new object();

        private readonly Reducer<TState> reducer;
        private readonly Dispatcher dispatcher;

        public Store(Reducer<TState> reducer, TState initialState = default(TState), params Middleware<TState>[] middlewares)
        {
            this.reducer = reducer;
            dispatcher = ApplyMiddlewares(middlewares);
            State = initialState;

            // TODO: Doesn't seem to be needed - can anything be subscribed before the constructor has completed?
            stateChangedHandler?.Invoke(State);
        }

        /// <inheritdoc />
        [CanBeNull]
        public TState State { get; private set; }

        /// <inheritdoc />
        public void Dispatch([CanBeNull] object action)
        {
            dispatcher(action);
        }

        private Dispatcher ApplyMiddlewares(params Middleware<TState>[] middlewares)
        {
            Dispatcher dispatcher = DispatchToReducer;
            foreach (Middleware<TState> middleware in middlewares)
            {
                dispatcher = middleware(this)(dispatcher);
            }
            return dispatcher;
        }

        private void DispatchToReducer(object action)
        {
            lock (syncRoot)
            {
                State = reducer(State, action);
            }
            stateChangedHandler?.Invoke(State);
        }
    }
}