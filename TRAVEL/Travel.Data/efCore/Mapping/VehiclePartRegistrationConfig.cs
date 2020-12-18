using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class VehiclePartRegistrationConfig : IEntityTypeConfiguration<VehiclePartRegistration>
    {
        public void Configure(EntityTypeBuilder<VehiclePartRegistration> builder)
        {
            builder.ToTable(nameof(VehiclePartRegistration));

        }
    }
}
