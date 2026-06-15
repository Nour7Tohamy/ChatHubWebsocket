using Application.DTOs.RoomDTOs;
using AutoMapper;
using Application.Infrastructure.Repositories;
using Domain.Entities.Main;
using MediatR;

namespace Application.Features.Rooms.Commands.Create;

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateRoomCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = new Room
        {
            Name = request.Dto.Name,
            Description = request.Dto.Description,
            OwnerId = request.OwnerId
        };

        await _unitOfWork.Rooms.AddAsync(room);

        // ✅ حذف الـ RoomMember object الـ unused
        // كان بيتعمل object لكن مش بيتضاف لأي حاجة
        await _unitOfWork.Rooms.AddMemberAsync(room.Id, request.OwnerId);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<RoomDto>(room);
    }
}