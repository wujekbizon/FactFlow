using System.ComponentModel.DataAnnotations;

namespace FactFlow.Web.Models;

public sealed class ManualFactInputModel
{
    public string TabId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fact content is required.")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Fact must contain between 5 and 1000 characters.")]
    [Display(Name = "Fact content")]
    public string Fact { get; set; } = string.Empty;
}
