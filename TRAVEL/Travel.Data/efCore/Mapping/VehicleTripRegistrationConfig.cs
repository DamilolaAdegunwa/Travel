using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class VehicleTripRegistrationConfig : IEntityTypeConfiguration<VehicleTripRegistration>
    {
        public void Configure(EntityTypeBuilder<VehicleTripRegistration> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(VehicleTripRegistration));

        }
    }
}
