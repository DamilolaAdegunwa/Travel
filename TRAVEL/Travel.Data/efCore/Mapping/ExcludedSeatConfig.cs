using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class ExcludedSeatConfig : IEntityTypeConfiguration<ExcludedSeat>
    {
        public void Configure(EntityTypeBuilder<ExcludedSeat> builder)
        {
            builder.ToTable(nameof(ExcludedSeat));
        }
    }
}