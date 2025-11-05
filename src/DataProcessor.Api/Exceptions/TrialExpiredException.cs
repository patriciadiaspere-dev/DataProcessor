namespace DataProcessor.Api.Exceptions;

public class TrialExpiredException : Exception
{
    public TrialExpiredException(string message) : base(message)
    {
    }
}
