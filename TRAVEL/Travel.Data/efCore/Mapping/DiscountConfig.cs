using Microsoft.EntityFrameworkCore;
using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Travel.Data.efCore.Mapping
{
    class DiscountConfig : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(Discount));

        }
    }
}
