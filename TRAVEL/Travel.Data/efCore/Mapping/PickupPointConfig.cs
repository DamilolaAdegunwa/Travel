using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class PickupPointConfig : IEntityTypeConfiguration<PickupPoint>
    {
        public void Configure(EntityTypeBuilder<PickupPoint> builder)
        {
            builder.HasOne(s => s.Trip)
                .WithMany()
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.ToTable(nameof(PickupPoint));

        }
    }
}