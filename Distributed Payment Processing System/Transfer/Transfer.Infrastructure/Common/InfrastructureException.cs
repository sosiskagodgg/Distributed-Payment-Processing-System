namespace Transfer.Infrastructure.Common
{
    public class InfrastructureException : Exception
    {
        public InfrastructureException(string message) : base(message) { }
        public InfrastructureException(string message,Exception exception) : base(message,exception) { }
    }
}
