using YaeaY.Account.Application.UseCases.Users.Queries.GetMyData;

namespace YaeaY.Account.Application.Services.ReadModels.Interfaces;

public interface IMyDataReader
{
    Task<Response?> GetAsync(Guid userId, CancellationToken cancellationToken);
}
