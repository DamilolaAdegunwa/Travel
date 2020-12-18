using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class StateConfig : IEntityTypeConfiguration<State>
    {

        public void Configure(EntityTypeBuilder<State> builder)
        {
            builder.ToTable(nameof(State));

        }
    }
}