using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Application.UseCases.Users.Commands.UpdateBasicData;

public sealed record Response(Guid Id, string FullName, DateOnly BirthDate, Gender Gender, bool HasChanges, string Message);
