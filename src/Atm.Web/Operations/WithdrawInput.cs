using System.ComponentModel.DataAnnotations;

namespace Atm.Web.Operations;

public sealed class WithdrawInput
{
    [Required(ErrorMessage = "Choose an account.")]
    public string? AccountId { get; set; }

    [Required(ErrorMessage = "Enter an amount.")]
    [Range(typeof(decimal), "0.01", "1000000000", ErrorMessage = "Enter an amount of at least $0.01.")]
    public decimal? Amount { get; set; }
}
