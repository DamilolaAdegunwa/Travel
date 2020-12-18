using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class ErrorCodeConfig : IEntityTypeConfiguration<ErrorCode>
    {
        public void Configure(EntityTypeBuilder<ErrorCode> builder)
        {
            builder.ToTable(nameof(ErrorCode));
        }
    }
}