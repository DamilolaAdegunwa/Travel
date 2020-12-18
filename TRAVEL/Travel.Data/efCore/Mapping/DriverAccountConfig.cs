using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class DriverAccountConfig : IEntityTypeConfiguration<DriverAccount>
    {
        public void Configure(EntityTypeBuilder<DriverAccount> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(DriverAccount));

        }
    }
}
