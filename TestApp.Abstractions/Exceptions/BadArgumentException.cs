namespace TestApp.Abstractions
{
    public sealed class BadArgumentException : Exception
    {
        public BadArgumentException(string message) : base(message)
        {
        }
    }
}
