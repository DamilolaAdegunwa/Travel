using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class VehiclePartPositionConfig : IEntityTypeConfiguration<VehiclePartPosition>
    {
        public void Configure(EntityTypeBuilder<VehiclePartPosition> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(VehiclePartPosition));

        }
    }
}
