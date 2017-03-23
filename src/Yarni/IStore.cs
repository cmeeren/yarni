namespace Yarni
{
    using JetBrains.Annotations;

    /// <summary>
    ///     Represents a store that encapsulates a state tree and is used to dispatch actions to update the
    ///     state tree.
    /// </summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    public interface IStore<out TState>
    {
        /// <summary>Dispatches an action to the store.</summary>
        /// <param name="action">The action to dispatch.</param>
        void Dispatch([CanBeNull] object action);

        /// <summary>Gets the current state tree.</summary>
        TState State { get; }

        /// <summary>Occurs when the state tree has been updated. Delegates are immediately called upon subscription.</summary>
        event StateChangedHandler<TState> StateChanged;
    }
}