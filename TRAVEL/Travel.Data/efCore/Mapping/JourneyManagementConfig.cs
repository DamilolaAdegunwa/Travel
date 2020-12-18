using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Travel.Core.Domain.Entities;

namespace Travel.Data.efCore.Mapping
{
    class JourneyManagementConfig : IEntityTypeConfiguration<JourneyManagement>
    {
        public void Configure(EntityTypeBuilder<JourneyManagement> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(JourneyManagement));

        }
    }
}