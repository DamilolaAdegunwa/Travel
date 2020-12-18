using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class WalletNumberConfig : IEntityTypeConfiguration<WalletNumber>
    {
        public void Configure(EntityTypeBuilder<WalletNumber> builder)
        {
            builder.HasIndex(x => x.WalletPan).IsUnique();
            builder.ToTable(nameof(WalletNumber));

        }
    }
}
