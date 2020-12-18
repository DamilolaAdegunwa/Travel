using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class CustomerCouponRegistrationConfig : IEntityTypeConfiguration<CustomerCouponRegistration>
    {
        public void Configure(EntityTypeBuilder<CustomerCouponRegistration> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(CustomerCouponRegistration));
        }
    }
}