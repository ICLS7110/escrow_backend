using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Infrastructure.Data.Configurations;
internal class UserDetailConfiguration : IEntityTypeConfiguration<UserDetail>
{
    public void Configure(EntityTypeBuilder<UserDetail> builder)
    {
        builder.Property(t => t.FullName)
            .HasMaxLength(200);
    }
}
