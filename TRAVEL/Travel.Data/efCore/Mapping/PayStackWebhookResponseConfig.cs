using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class PayStackWebhookResponseConfig : IEntityTypeConfiguration<PayStackWebhookResponse>
    {
        public void Configure(EntityTypeBuilder<PayStackWebhookResponse> builder)
        {
            builder.HasKey(x => x.Reference);
            builder.ToTable(nameof(PayStackWebhookResponse));

        }
    }
}
