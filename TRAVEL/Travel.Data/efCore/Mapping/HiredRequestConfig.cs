using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class HiredRequestConfig : IEntityTypeConfiguration<HireRequest>
    {
        public void Configure(EntityTypeBuilder<HireRequest> builder)
        {
            builder.ToTable(nameof(HireRequest));

        }
    }
}