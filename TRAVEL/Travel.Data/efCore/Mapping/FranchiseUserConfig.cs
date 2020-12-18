
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class FranchiseUserConfig : IEntityTypeConfiguration<FranchiseUser>
    {
        public void Configure(EntityTypeBuilder<FranchiseUser> builder)
        {
            builder.ToTable(nameof(FranchiseUser));

        }
    }
}