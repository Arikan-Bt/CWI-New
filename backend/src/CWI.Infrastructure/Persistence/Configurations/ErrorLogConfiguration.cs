using CWI.Domain.Entities.System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CWI.Infrastructure.Persistence.Configurations;

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.Property(x => x.TraceId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.ExceptionType)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ErrorCode)
            .HasMaxLength(128);

        builder.Property(x => x.ParameterName)
            .HasMaxLength(256);

        builder.Property(x => x.Environment)
            .HasMaxLength(64);

        builder.Property(x => x.MachineName)
            .HasMaxLength(128);

        builder.Property(x => x.RequestContentType)
            .HasMaxLength(256);

        builder.HasIndex(x => x.OccurredAt);
        builder.HasIndex(x => new { x.IsResolved, x.OccurredAt });
        builder.HasIndex(x => new { x.UserId, x.OccurredAt });
        builder.HasIndex(x => x.TraceId);
    }
}
