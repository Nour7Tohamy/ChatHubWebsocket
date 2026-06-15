using Application.Features.Rooms.Commands.Create;
using FluentValidation;

namespace Application.Features.Rooms.Commands.CreateRoom;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Room name is required")
            .MinimumLength(3).WithMessage("Room name must be at least 3 characters")
            .MaximumLength(100).WithMessage("Room name cannot exceed 100 characters");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}