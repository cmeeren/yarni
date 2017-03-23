namespace Yarni.Tests
{
    /// <summary>Provides test helper reducers.</summary>
    public static class Reducers
    {
        public static Reducer<object> Passthrough => (state, action) => state;

        public static Reducer<object> Return(object newState) => (state, action) => newState;
    }
}