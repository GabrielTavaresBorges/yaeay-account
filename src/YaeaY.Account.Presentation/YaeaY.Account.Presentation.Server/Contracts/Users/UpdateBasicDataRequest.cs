using YaeaY.Account.Domain.Enumerators;

namespace YaeaY.Account.Presentation.Server.Contracts.Users;

public sealed record UpdateBasicDataRequest(string? FullName, DateOnly? BirthDate, Gender? Gender);
