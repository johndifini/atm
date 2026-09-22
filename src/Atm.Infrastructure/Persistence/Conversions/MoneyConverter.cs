using Atm.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atm.Infrastructure.Persistence.Conversions;

public sealed class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter()
        : base(money => money.Value, value => Money.From(value))
    {
    }
}
