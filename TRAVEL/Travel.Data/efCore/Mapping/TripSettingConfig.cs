using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class TripSettingConfig : IEntityTypeConfiguration<TripSetting>
    {
        public void Configure(EntityTypeBuilder<TripSetting> builder)
        {
            builder.HasKey(x => x.Id);
            builder.ToTable(nameof(TripSetting));

        }
    }
}