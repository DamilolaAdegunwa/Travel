using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class SubRouteConfig : IEntityTypeConfiguration<SubRoute>
    {
        public void Configure(EntityTypeBuilder<SubRoute> builder)
        {
            builder.ToTable(nameof(SubRoute));

        }
    }
}