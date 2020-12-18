using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class PayStackPaymentResponseConfig : IEntityTypeConfiguration<PayStackPaymentResponse>
    {
        public void Configure(EntityTypeBuilder<PayStackPaymentResponse> builder)
        {
            builder.HasKey(x => x.Reference);
            builder.ToTable(nameof(PayStackPaymentResponse));

        }
    }
}