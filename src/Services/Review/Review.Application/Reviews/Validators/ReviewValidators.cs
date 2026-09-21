using FluentValidation;
using Review.Application.Reviews.Dtos;

namespace Review.Application.Reviews.Validators;

public sealed class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(3000);
    }
}
public sealed class UpdateReviewValidator : AbstractValidator<UpdateReviewDto>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(3000);
    }
}
public sealed class ReactValidator : AbstractValidator<ReactDto>
{
    public ReactValidator()
    {
        RuleFor(x => x.Type).NotEmpty().Must(v => v == "like" || v == "dislike");
    }
}
public sealed class ReportValidator : AbstractValidator<ReportReviewDto>
{
    public ReportValidator() { RuleFor(x => x.Reason).NotEmpty().MaximumLength(500); }
}
