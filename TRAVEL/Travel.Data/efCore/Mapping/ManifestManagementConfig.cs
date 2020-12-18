using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class ManifestManagementConfig : IEntityTypeConfiguration<Manifest>
    {
        public void Configure(EntityTypeBuilder<Manifest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(Manifest));

        }
    }
}