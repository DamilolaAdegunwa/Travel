using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class EmployeeRouteConfig : IEntityTypeConfiguration<EmployeeRoute>
    {
        public void Configure(EntityTypeBuilder<EmployeeRoute> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(EmployeeRoute));

        }
    }
}