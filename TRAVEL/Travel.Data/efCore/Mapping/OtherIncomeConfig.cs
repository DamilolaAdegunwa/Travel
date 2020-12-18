using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class OtherIncomeConfig : IEntityTypeConfiguration<OtherIncome>
    {
        public void Configure(EntityTypeBuilder<OtherIncome> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(OtherIncome));

        }
    }
}