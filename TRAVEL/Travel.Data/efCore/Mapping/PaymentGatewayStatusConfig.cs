using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class PaymentGatewayStatusConfig : IEntityTypeConfiguration<PaymentGatewayStatus>
    {
        public void Configure(EntityTypeBuilder<PaymentGatewayStatus> builder)
        {
            builder.ToTable(nameof(PaymentGatewayStatus));
        }
    }
}