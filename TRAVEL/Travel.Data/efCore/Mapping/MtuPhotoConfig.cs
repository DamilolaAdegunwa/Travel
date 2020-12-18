using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class MtuPhotoConfig : IEntityTypeConfiguration<MtuPhoto>
    {
        public void Configure(EntityTypeBuilder<MtuPhoto> builder)
        {
            builder.ToTable(nameof(MtuPhoto));

        }
    }
}
