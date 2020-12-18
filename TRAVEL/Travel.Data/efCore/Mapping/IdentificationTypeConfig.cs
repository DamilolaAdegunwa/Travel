using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class IdentificationTypeConfig : IEntityTypeConfiguration<IdentificationType>
    {
        public void Configure(EntityTypeBuilder<IdentificationType> builder)
        {
            builder.ToTable(nameof(IdentificationType));

        }
    }
}
