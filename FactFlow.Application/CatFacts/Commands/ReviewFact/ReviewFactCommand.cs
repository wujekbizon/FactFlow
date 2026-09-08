using FactFlow.Application.Common.Messaging;
using FactFlow.Domain.CatFacts;

namespace FactFlow.Application.CatFacts.Commands.ReviewFact;

public sealed record ReviewFactCommand(
    int Id,
    FactReviewStatus Status,
    string? Note) : ICommand<bool>;
