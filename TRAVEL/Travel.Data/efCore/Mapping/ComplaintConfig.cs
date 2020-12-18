using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    public class ComplaintConfig : IEntityTypeConfiguration<Complaint>
    {
        public void Configure(EntityTypeBuilder<Complaint> builder)
        {
            builder.ToTable(nameof(Complaint));
        }
    }
}
