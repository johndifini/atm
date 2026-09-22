using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Atm.Infrastructure.Persistence.Conversions;

public sealed class AccountIdConverter : ValueConverter<AccountId, string>
{
    public AccountIdConverter()
        : base(id => id.Value, value => AccountId.From(value))
    {
    }
}
