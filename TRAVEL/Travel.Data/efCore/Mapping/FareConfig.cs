using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class FareConfig : IEntityTypeConfiguration<Fare>
    {
        public void Configure(EntityTypeBuilder<Fare> builder)
        {
            builder.ToTable(nameof(Fare));

        }
    }
}
