using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class FranchizeConfig : IEntityTypeConfiguration<Franchize>
    {
        public void Configure(EntityTypeBuilder<Franchize> builder)
        {
            builder.ToTable(nameof(Franchize));
        }
    }
}