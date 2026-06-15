using Application.DTOs.RoomDTOs;
using Application.Features.Rooms.Queries.GetById;
using AutoMapper;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Rooms.Queries.GetRoomById;

public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, RoomDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRoomByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoomDetailsDto> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await _unitOfWork.Rooms.GetRoomWithMembersAsync(request.RoomId)
            ?? throw new Exception("Room not found");

        return _mapper.Map<RoomDetailsDto>(room);
    }
}