using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class FareCalendarConfig : IEntityTypeConfiguration<FareCalendar>
    {
        public void Configure(EntityTypeBuilder<FareCalendar> builder)
        {
            builder.ToTable(nameof(FareCalendar));

        }
    }
}