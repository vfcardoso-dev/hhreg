namespace hhreg.business.exceptions;

public class HhregException : Exception
{
    public HhregException() : base() {}
    public HhregException(string message) : base(message) {}
    public HhregException(string message, Exception inner) : base(message, inner) {}
}