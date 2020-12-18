using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class MtuConfig : IEntityTypeConfiguration<MtuReportModel>
    {
        public void Configure(EntityTypeBuilder<MtuReportModel> builder)
        {
            builder.ToTable(nameof(MtuReportModel));

        }
    }
}
