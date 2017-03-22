﻿namespace Yarni
{
    using System;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>Represents a method that can be used as a handler for <see cref="E:IStore{TState}.StateChanged" />
    /// </summary>
    /// <typeparam name="TState">THe state tree type.</typeparam>
    /// <param name="state">The updated state tree.</param>
    public delegate void StateChangedHandler<TState>([CanBeNull] TState state);

    /// <summary>Represents a method that dispatches an action.</summary>
    /// <param name="action">The action to dispatch.</param>
    public delegate void Dispatcher([CanBeNull] object action);

    /// <summary>Represents a method that is used to create a middleware.</summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    /// <param name="store">The <see cref="T:Store{TState}" /> this middleware is to be used by.</param>
    /// <returns>
    ///     A function that, when called with a <see cref="Dispatcher" />, returns a new
    ///     <see cref="Dispatcher" /> that wraps the first one.
    /// </returns>
    public delegate Func<Dispatcher, Dispatcher> Middleware<TState>(IStore<TState> store);

    /// <summary>Represents a method that is used to update the state tree.</summary>
    /// <typeparam name="TState">The state tree type.</typeparam>
    /// <param name="previousState">The previous state tree.</param>
    /// <param name="action">The action to be applied to the state tree.</param>
    /// <returns>The updated state tree.</returns>
    [CanBeNull]
    public delegate TState Reducer<TState>([CanBeNull] TState previousState, [CanBeNull] object action);

    /// <summary>
    ///     Represents a method that can be used as a listener by <see cref="T:ListenerMiddleware{TState}" />.
    ///     Should preferably be async and not block. If it blocks, subsequent listeners will not be called until it
    ///     completes.
    /// </summary>
    /// <remarks>
    ///     This is semantically a top-level event handler, and all exceptions should be handled in the
    ///     listener. Unhandled exceptions will be silently ignored (the returned <see cref="T:Task" /> is never used).
    /// </remarks>
    /// <typeparam name="TState">The state tree type.</typeparam>
    /// <param name="action">The action passed to the listener.</param>
    /// <param name="state">The state tree before the action was passed to the store.</param>
    /// <param name="dispatch">The dispatcher that the listener can use to dispatch new actions to the store.</param>
    public delegate Task AsyncListener<TState>([CanBeNull] object action, [CanBeNull] TState state, Dispatcher dispatch);
}