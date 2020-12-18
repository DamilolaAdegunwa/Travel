using Travel.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Travel.Data.efCore.Mapping
{
    class RouteConfig : IEntityTypeConfiguration<Route>
    {

        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.HasOne(r => r.DepartureTerminal)
                .WithMany()
                .HasForeignKey(r => r.DepartureTerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasOne(r => r.DestinationTerminal)
               .WithMany()
               .HasForeignKey(r => r.DestinationTerminalId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.ToTable(nameof(Route));

        }
    }
}