using Application.DTOs.MessageDTOs;
using AutoMapper;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Messages.Queries.Get;

public class GetRoomMessagesQueryHandler : IRequestHandler<GetRoomMessagesQuery, IEnumerable<MessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRoomMessagesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MessageDto>> Handle(GetRoomMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _unitOfWork.Messages.GetRoomMessagesAsync(
            request.RoomId, request.Page, request.PageSize);

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }
}