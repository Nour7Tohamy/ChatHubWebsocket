namespace Application.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message = "Forbidden") : base(message, 403) { }
}