using System.Globalization;
using Atm.Domain;
using Atm.Domain.Transactions;

namespace Atm.Web.Presentation;

/// <summary>
/// Presentation-boundary formatting. Money is US dollars; timestamps are UTC
/// and are labelled as such.
/// </summary>
public static class Format
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    public static string Currency(Money money) => money.Value.ToString("C2", Culture);

    public static string Timestamp(DateTime utc) =>
        utc.ToString("MMM d, yyyy, HH:mm:ss", Culture) + " UTC";

    public static string MachineTimestamp(DateTime utc) =>
        utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    public static string TypeLabel(TransactionType type) => type switch
    {
        TransactionType.Deposit => "Deposit",
        TransactionType.Withdrawal => "Withdrawal",
        TransactionType.Transfer => "Transfer",
        _ => type.ToString(),
    };

    public static string SignedAmount(TransactionType type, Money amount) => type switch
    {
        TransactionType.Deposit => "+" + Currency(amount),
        TransactionType.Withdrawal => "-" + Currency(amount),
        _ => Currency(amount),
    };
}
