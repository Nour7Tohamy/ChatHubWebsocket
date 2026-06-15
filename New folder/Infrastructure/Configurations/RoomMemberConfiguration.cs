using Domain.Entities.Main;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Data.Configurations;

public class RoomMemberConfiguration : IEntityTypeConfiguration<RoomMember>
{
    public void Configure(EntityTypeBuilder<RoomMember> builder)
    {
        builder.HasKey(rm => new { rm.RoomId, rm.UserId });

        builder.HasOne(rm => rm.Room)
            .WithMany(r => r.Members)
            .HasForeignKey(rm => rm.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rm => rm.User)
            .WithMany(u => u.RoomMembers)
            .HasForeignKey(rm => rm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}