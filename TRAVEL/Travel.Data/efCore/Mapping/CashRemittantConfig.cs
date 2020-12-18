using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class CashRemittantConfig : IEntityTypeConfiguration<CashRemittant>
    {
        public void Configure(EntityTypeBuilder<CashRemittant> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(CashRemittant));
        }
    }
}