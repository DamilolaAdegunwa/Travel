using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Travel.Data.efCore.Mapping
{
    class RegionConfig : IEntityTypeConfiguration<Region>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Region> builder)
        {
            builder.ToTable(nameof(Region));
        }
    }
}
