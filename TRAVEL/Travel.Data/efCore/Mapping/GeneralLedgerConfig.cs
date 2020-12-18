using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class GeneralLedgerConfig : IEntityTypeConfiguration<GeneralLedger>
    {
        public void Configure(EntityTypeBuilder<GeneralLedger> builder)
        {
            builder.ToTable(nameof(GeneralLedger));

        }
    }
}
