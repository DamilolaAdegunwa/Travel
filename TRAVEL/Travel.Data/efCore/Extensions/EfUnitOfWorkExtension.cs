using Travel.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Travel.Data.efCore.Extensions
{
   public static class EfUnitOfWorkExtension
    {
        public static TDbContext GetDbContext<TDbContext>(this IUnitOfWork unitOfWork)
           where TDbContext : DbContext
        {
            if (unitOfWork == null) {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            if (!(unitOfWork is EfCoreUnitOfWork)) {
                throw new ArgumentException("unitOfWork is not type of " + typeof(EfCoreUnitOfWork).FullName, nameof(unitOfWork));
            }

            return (unitOfWork as EfCoreUnitOfWork).GetOrCreateDbContext<TDbContext>();
        }
    }
}
