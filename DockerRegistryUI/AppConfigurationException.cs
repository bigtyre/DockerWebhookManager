using System.Runtime.Serialization;

namespace BigTyre;
class AppConfigurationException : Exception
{
    public AppConfigurationException()
    {
    }

    public AppConfigurationException(string? message) : base(message)
    {
    }

    public AppConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected AppConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
