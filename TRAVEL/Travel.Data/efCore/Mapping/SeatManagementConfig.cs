using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class SeatManagementConfig : IEntityTypeConfiguration<SeatManagement>
    {
        public void Configure(EntityTypeBuilder<SeatManagement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ToTable(nameof(SeatManagement));

        }
    }
}