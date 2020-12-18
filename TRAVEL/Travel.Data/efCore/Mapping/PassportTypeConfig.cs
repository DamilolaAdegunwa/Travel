using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    public class PassportTypeConfig : IEntityTypeConfiguration<PassportType>
    {
        public void Configure(EntityTypeBuilder<PassportType> builder)
        {
            builder.ToTable(nameof(PassportType));
        }
    }
}