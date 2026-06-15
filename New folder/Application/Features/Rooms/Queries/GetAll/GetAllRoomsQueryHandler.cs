using Application.DTOs.RoomDTOs;
using AutoMapper;
using Application.Infrastructure.Repositories;
using MediatR;

namespace Application.Features.Rooms.Queries.GetAll;

public class GetAllRoomsQueryHandler : IRequestHandler<GetAllRoomsQuery, IEnumerable<RoomDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllRoomsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RoomDto>> Handle(GetAllRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await _unitOfWork.Rooms.GetAllWithMembersAsync(); // ✅
        return _mapper.Map<IEnumerable<RoomDto>>(rooms);
    }
}