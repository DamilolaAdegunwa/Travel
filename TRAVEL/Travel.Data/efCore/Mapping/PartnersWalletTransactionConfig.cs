using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    //class PartnersWalletTransactionConfig : IEntityTypeConfiguration<PartnersWalletTransaction>
    //{
    //    public void Configure(EntityTypeBuilder<PartnersWalletTransaction> builder)
    //    {
    //        builder.HasOne(x => x.PartnersWallet)
    //           .WithMany()
    //           .HasForeignKey(x => x.WalletId)
    //           .OnDelete(DeleteBehavior.ClientSetNull);

    //        builder.ToTable(nameof(PartnersWalletTransaction));

    //    }
    //}
}
