using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class VehicleMileageConfig : IEntityTypeConfiguration<VehicleMileage>
    {
        public void Configure(EntityTypeBuilder<VehicleMileage> builder)
        {
            builder.HasKey(x =>
            new
            {
                x.VehicleRegistrationNumber,
                x.ServiceLevel
            });

            builder.ToTable(nameof(VehicleMileage));
        }
    }
}