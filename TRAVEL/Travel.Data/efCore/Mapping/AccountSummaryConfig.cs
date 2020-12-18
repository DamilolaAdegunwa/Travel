using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class AccountSummaryConfig : IEntityTypeConfiguration<AccountSummary>
    {
        public void Configure(EntityTypeBuilder<AccountSummary> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(AccountSummary));
        }
    }
}