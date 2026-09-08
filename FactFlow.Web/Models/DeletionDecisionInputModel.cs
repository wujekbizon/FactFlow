using System.ComponentModel.DataAnnotations;

namespace FactFlow.Web.Models;

public sealed class DeletionDecisionInputModel
{
    public int RequestId { get; set; }
    public string TabId { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Decision note cannot exceed 1000 characters.")]
    public string? Note { get; set; }
}
