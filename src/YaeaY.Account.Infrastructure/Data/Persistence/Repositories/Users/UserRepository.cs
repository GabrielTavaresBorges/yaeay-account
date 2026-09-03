using Microsoft.EntityFrameworkCore;
using YaeaY.Account.Domain.Entities.AggregateRoots.Users;
using YaeaY.Account.Domain.Repositories.Users;
using YaeaY.Account.Domain.ValueObjects.Emails;
using YaeaY.Account.Infrastructure.Data.Context;

namespace YaeaY.Account.Infrastructure.Data.Repositories.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _context.DomainUsers.AddAsync(user, cancellationToken);
    }

    public Task UpdateUserAsync(User user, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task UpdateUserPhonesAsync(
        User user,
        IReadOnlyCollection<YaeaY.Account.Domain.Entities.UserPhones.UserPhone> addedPhones,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(addedPhones);

        foreach (var phone in addedPhones)
        {
            if (!user.Phones.Contains(phone))
                throw new InvalidOperationException("The added phone must belong to the user aggregate.");

            _context.Entry(phone).State = EntityState.Added;
        }

        return Task.CompletedTask;
    }

    public Task UpdateUserDocumentsAsync(
        User user,
        IReadOnlyCollection<YaeaY.Account.Domain.Entities.UserDocuments.UserDocument> addedDocuments,
        IReadOnlyCollection<YaeaY.Account.Domain.Entities.UserDocuments.UserDocumentImage> addedImages,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(addedDocuments);
        ArgumentNullException.ThrowIfNull(addedImages);

        foreach (var document in addedDocuments)
        {
            if (!user.Documents.Contains(document))
                throw new InvalidOperationException("The added document must belong to the user aggregate.");

            _context.Add(document);
        }

        foreach (var image in addedImages)
        {
            if (!user.Documents.Any(document => document.Images.Contains(image)))
                throw new InvalidOperationException("The added document image must belong to the user aggregate.");

            _context.Add(image);
        }

        _context.ChangeTracker.DetectChanges();

        var currentDocuments = user.Documents.ToHashSet();
        foreach (var entry in _context.ChangeTracker.Entries<YaeaY.Account.Domain.Entities.UserDocuments.UserDocument>().ToArray())
        {
            if (entry.Property<Guid>("UserId").OriginalValue == user.Id && !currentDocuments.Contains(entry.Entity))
                _context.Remove(entry.Entity);
        }

        return Task.CompletedTask;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdWithDocumentsAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.DomainUsers
            .Include(user => user.Documents)
                .ThenInclude(document => document.Cpf)
            .Include(user => user.Documents)
                .ThenInclude(document => document.Rg)
            .Include(user => user.Documents)
                .ThenInclude(document => document.Images)
            .AsSplitQuery()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdWithPhonesAsync(Guid id, CancellationToken cancellationToken)
    {
        return _context.DomainUsers
            .Include(user => user.Phones)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return _context.DomainUsers.FirstOrDefaultAsync(
            user => user.Email.EmailAddress == email.EmailAddress,
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return _context.DomainUsers.AnyAsync(
            user => user.Email.EmailAddress == email.EmailAddress, cancellationToken);
    }
}
