using System.ComponentModel.DataAnnotations;

namespace Atm.Web.Operations;

public sealed class TransferInput
{
    [Required(ErrorMessage = "Choose a source account.")]
    public string? SourceAccountId { get; set; }

    [Required(ErrorMessage = "Choose a destination account.")]
    public string? DestinationAccountId { get; set; }

    [Required(ErrorMessage = "Enter an amount.")]
    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Enter an amount of at least $0.01.")]
    public decimal? Amount { get; set; }
}
