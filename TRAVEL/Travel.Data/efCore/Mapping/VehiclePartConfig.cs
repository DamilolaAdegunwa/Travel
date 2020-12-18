using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class VehiclePartConfig : IEntityTypeConfiguration<VehiclePart>
    {
        public void Configure(EntityTypeBuilder<VehiclePart> builder)
        {
            builder.ToTable(nameof(VehiclePart));

        }
    }
}