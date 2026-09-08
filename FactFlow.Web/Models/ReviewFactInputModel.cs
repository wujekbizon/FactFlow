using System.ComponentModel.DataAnnotations;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Web.Models;

public sealed class ReviewFactInputModel
{
    public int Id { get; set; }
    public string TabId { get; set; } = string.Empty;

    [Required]
    public FactReviewStatus Status { get; set; }

    [StringLength(2000, ErrorMessage = "Review note cannot exceed 2000 characters.")]
    public string? Note { get; set; }
}
