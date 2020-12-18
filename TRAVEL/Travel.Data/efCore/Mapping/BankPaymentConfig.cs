using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class BankPaymentConfig : IEntityTypeConfiguration<BankPayment>
    {
        public void Configure(EntityTypeBuilder<BankPayment> builder)
        {
            builder.ToTable(nameof(BankPayment));
        }
    }
}