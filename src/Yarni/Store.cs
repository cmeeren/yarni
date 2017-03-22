namespace Yarni
{
    using JetBrains.Annotations;

    /// <inheritdoc />
    public class Store<TState> : IStore<TState>
    {
        [CanBeNull] private StateChangedHandler<TState> stateChangedHandler;

        /// <inheritdoc />
        public event StateChangedHandler<TState> StateChanged
        {
            add
            {
                this.stateChangedHandler += value;
                value.Invoke(this.State);
            }
            // ReSharper disable once DelegateSubtraction (OK since we're removing a single value)
            remove => this.stateChangedHandler -= value;
        }

        private readonly object syncRoot = new object();

        private readonly Reducer<TState> reducer;
        private readonly Dispatcher dispatcher;

        public Store(Reducer<TState> reducer, TState initialState = default(TState), params Middleware<TState>[] middlewares)
        {
            this.reducer = reducer;
            this.dispatcher = this.ApplyMiddlewares(middlewares);
            this.State = initialState;
        }

        /// <inheritdoc />
        [CanBeNull]
        public TState State { get; private set; }

        /// <inheritdoc />
        public void Dispatch([CanBeNull] object action)
        {
            this.dispatcher(action);
        }

        private Dispatcher ApplyMiddlewares(params Middleware<TState>[] middlewares)
        {
            Dispatcher dispatcher = this.DispatchToReducer;
            foreach (Middleware<TState> middleware in middlewares)
            {
                dispatcher = middleware(this)(dispatcher);
            }
            return dispatcher;
        }

        private void DispatchToReducer(object action)
        {
            lock (this.syncRoot)
            {
                this.State = this.reducer(this.State, action);
            }
            this.stateChangedHandler?.Invoke(this.State);
        }
    }
}