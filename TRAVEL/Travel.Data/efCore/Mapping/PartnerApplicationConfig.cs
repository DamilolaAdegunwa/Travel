using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    //class PartnerApplicationConfig : IEntityTypeConfiguration<PartnerApplication>
    //{
    //    public void Configure(EntityTypeBuilder<PartnerApplication> builder)
    //    {
    //        builder.HasOne(s => s.Approver)
    //            .WithMany()
    //            .HasForeignKey(s => s.ApproverId)
    //            .OnDelete(DeleteBehavior.ClientSetNull);

    //        builder.ToTable(nameof(PartnerApplication));

    //    }
    //}
}