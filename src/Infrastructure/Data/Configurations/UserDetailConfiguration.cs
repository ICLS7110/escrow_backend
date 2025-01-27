using Escrow.Api.Domain.Entities.UserPanel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Escrow.Api.Infrastructure.Data.Configurations;
internal class UserDetailConfiguration : IEntityTypeConfiguration<UserDetail>
{
    public void Configure(EntityTypeBuilder<UserDetail> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
        builder.Property(t => t.FullName)
            .HasMaxLength(200);
    }
}
