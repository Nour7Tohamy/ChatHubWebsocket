using Application.DTOs.MessageDTOs;
using Application.Features.Messages.Queries.GetPrivate;
using AutoMapper;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Messages.Queries.GetPrivateMessages;

public class GetPrivateMessagesQueryHandler : IRequestHandler<GetPrivateMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPrivateMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MessageDto>> Handle(GetPrivateMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _unitOfWork.Messages.GetPrivateMessagesAsync(
            request.SenderId, request.ReceiverId);

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }
}