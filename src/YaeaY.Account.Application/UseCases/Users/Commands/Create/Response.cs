namespace YaeaY.Account.Application.UseCases.Users.Commands.Create;

public sealed record Response
{
    public Guid Id { get; init; }
    public string FullName { get; init; }
    public string Message { get; init; }

    public Response(Guid id, string fullName, string message)
    {
        Id = id;
        FullName = fullName;
        Message = message;
    }
}
