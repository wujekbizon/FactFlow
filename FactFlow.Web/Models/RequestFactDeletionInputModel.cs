using System.ComponentModel.DataAnnotations;

namespace FactFlow.Web.Models;

public sealed class RequestFactDeletionInputModel
{
    public int FactId { get; set; }
    public string TabId { get; set; } = string.Empty;
    public string FactContent { get; set; } = string.Empty;

    [Required(ErrorMessage = "A deletion reason is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Reason must contain between 10 and 1000 characters.")]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
}
