namespace Hhreg.Business.Exceptions;

public class HhregException : Exception
{
    public HhregException() : base() { }
    public HhregException(string message) : base(message) { }
    public HhregException(string message, Exception inner) : base(message, inner) { }
}