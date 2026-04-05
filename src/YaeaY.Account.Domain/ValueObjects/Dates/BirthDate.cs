using YaeaY.Account.Domain.Abstraction.Records;

namespace YaeaY.Account.Domain.ValueObjects.Dates;

public sealed record BirthDate
{
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
        {
            return Result<DateOnly>.Failure(new Error(
                Identifier: "BIRTH_DATE_IN_FUTURE",
                Message: "Birth date cannot be in the future.\n" +
                         $"Received: {date:yyyy-MM-dd}.\n" +
                         $"Today (UTC): {today:yyyy-MM-dd}."
            ));
        }

        const int MaxAgeYears = 150;

        var minAllowed = today.AddYears(-MaxAgeYears);

        if (date < minAllowed)
        {
            return Result<DateOnly>.Failure(new Error(
                Identifier: "BIRTH_DATE_TOO_OLD",
                Message: $"Birth date cannot be more than {MaxAgeYears} years ago.\n" +
                         $"Received: {date:yyyy-MM-dd}.\n" +
                         $"Minimum allowed (UTC): {minAllowed:yyyy-MM-dd}."
            ));
        }

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