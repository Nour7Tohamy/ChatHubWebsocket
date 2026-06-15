using Application.DTOs.MessageDTOs;
using Application.DTOs.RoomDTOs;
using AutoMapper;
using Domain.Entities.Main;

namespace Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src =>
                    src.Owner != null ? src.Owner.DisplayName ?? src.Owner.UserName : "Unknown"))
            .ForMember(dest => dest.MembersCount,
                opt => opt.MapFrom(src =>
                    src.RoomMembers != null ? src.RoomMembers.Count : 0));

        CreateMap<Room, RoomDetailsDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src =>
                    src.Owner != null ? src.Owner.DisplayName ?? src.Owner.UserName : "Unknown"));

        CreateMap<RoomMember, MemberDto>()
            .ForMember(dest => dest.UserId,
                opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src =>
                    src.User != null ? src.User.UserName : "Unknown"))
            .ForMember(dest => dest.DisplayName,
                opt => opt.MapFrom(src =>
                    src.User != null
                        ? src.User.DisplayName ?? src.User.UserName
                        : "Unknown"))
            .ForMember(dest => dest.IsOnline,
                opt => opt.MapFrom(src =>
                    src.User != null && src.User.IsOnline))
            .ForMember(dest => dest.JoinedAt,
                opt => opt.MapFrom(src => src.JoinedAt));

        // ✅ SenderName من DisplayName مش UserName
        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderName,
                opt => opt.MapFrom(src =>
                    src.Sender != null
                        ? src.Sender.DisplayName ?? src.Sender.UserName
                        : "Unknown"));

    }
}