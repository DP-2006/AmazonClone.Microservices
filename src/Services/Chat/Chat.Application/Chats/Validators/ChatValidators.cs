using FluentValidation;
using Chat.Application.Chats.Dtos;

namespace Chat.Application.Chats.Validators;

public sealed class CreateRoomValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
    }
}

public sealed class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.Content).MaximumLength(4000);
        RuleFor(x => x.Type).Must(t => t == "Text" || t == "Image" || t == "Video" || t == "File")
            .WithMessage("Type must be Text/Image/Video/File");
    }
}

public sealed class ReportMessageValidator : AbstractValidator<ReportMessageDto>
{
    public ReportMessageValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
