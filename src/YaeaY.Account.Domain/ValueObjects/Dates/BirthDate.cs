using YaeaY.Account.Domain.Abstraction.Result;
using YaeaY.Account.Domain.Errors.BirthDate;

namespace YaeaY.Account.Domain.ValueObjects.Dates;

public sealed record BirthDate
{
    private const int MaximumAgeYears = 150;

    private readonly DateOnly _date;

    public DateOnly Date => _date;

    private BirthDate(DateOnly date)
    {
        _date = date;
    }

    public static Result<BirthDate> Create(DateOnly date)
    {
        var validatedBirthDate = ValidateBirthDate(date);

        if (validatedBirthDate.IsFailure)
            return Result<BirthDate>.Failure(validatedBirthDate.Error);
          
        var birthDate = new BirthDate(validatedBirthDate.Value);

        return Result<BirthDate>.Success(birthDate);
    }

    private static Result<DateOnly> ValidateBirthDate(DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (date > today)
            return Result<DateOnly>.Failure(BirthDateErrors.InFuture(date, today));

        var minAllowed = today.AddYears(-MaximumAgeYears);

        if (date < minAllowed)
            return Result<DateOnly>.Failure(BirthDateErrors.TooOld(date, minAllowed, MaximumAgeYears));

        return Result<DateOnly>.Success(date);
    }

    public int GetAge()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var age = today.Year - Date.Year;

        if (Date > today.AddYears(-age))
            age--;

        return age;
    }

    public bool IsAdult(int adultAge = 18)
        => GetAge() >= adultAge;

    public int GetDaysOfLife()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return today.DayNumber - Date.DayNumber;
    }
}
