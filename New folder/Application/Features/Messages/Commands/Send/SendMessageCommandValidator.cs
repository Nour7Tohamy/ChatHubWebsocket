using Application.Features.Messages.Commands.Send;
using FluentValidation;

namespace Application.Features.Messages.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Dto.Content)
            .NotEmpty().WithMessage("Message content is required")
            .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters");

        RuleFor(x => x.Dto)
            .Must(dto => dto.RoomId.HasValue || !string.IsNullOrEmpty(dto.ReceiverId))
            .WithMessage("Message must have either a RoomId or a ReceiverId");
    }
}