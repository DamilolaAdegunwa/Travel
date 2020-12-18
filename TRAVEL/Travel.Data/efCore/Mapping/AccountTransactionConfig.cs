using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class AccountTransactionConfig : IEntityTypeConfiguration<AccountTransaction>
    {
        public void Configure(EntityTypeBuilder<AccountTransaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(AccountTransaction));
        }
    }
}