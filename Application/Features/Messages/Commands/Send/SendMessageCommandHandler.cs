using Application.DTOs.MessageDTOs;
using AutoMapper;
using Application.Infrastructure.Repositories;
using Domain.Entities.Main;
using MediatR;

namespace Application.Features.Messages.Commands.Send;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SendMessageCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<MessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new Message
        {
            Content = request.Dto.Content,
            SenderId = request.SenderId,
            RoomId = request.Dto.RoomId,
            ReceiverId = request.Dto.ReceiverId
        };

        await _unitOfWork.Messages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<MessageDto>(message);
        result.SenderName = ""; // هيتحط من MessageService اللي عنده UserManager
        return result;
    }
}