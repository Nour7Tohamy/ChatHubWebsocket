namespace Application.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with id [{key}] was not found.", 404) { }
}
public class RoomNotFoundException : NotFoundException
{
    public RoomNotFoundException(object key) : base("Room", key) { }
}

public class MessageNotFoundException : NotFoundException
{
    public MessageNotFoundException(object key) : base("Message", key) { }
}

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(object key) : base("User", key) { }
}