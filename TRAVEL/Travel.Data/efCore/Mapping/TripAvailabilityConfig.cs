using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class TripAvailabilityConfig : IEntityTypeConfiguration<TripAvailability>
    {
        public void Configure(EntityTypeBuilder<TripAvailability> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(TripAvailability));

        }
    }
}