using Application.Exceptions;
using Application.Features.Messages.Commands.Delete;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Messages.Commands.DeleteMessage;

public class DeleteMessageCommandHandler
: IRequestHandler<DeleteMessageCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMessageCommandHandler(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.Messages.GetByIdAsync(request.MessageId);

        if (message is null)
            throw new NotFoundException("Message", request.MessageId);

        if (message.SenderId != request.UserId)
            throw new ForbiddenException("You can only delete your own messages");

        message.IsDeleted = true;

        _unitOfWork.Messages.Update(message);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

}