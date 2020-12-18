using Travel.Core.Utils;
using Travel.Data.efCore;
using Travel.Data.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Travel.WebAPI.Utils.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static void RegisterGenericRepos(this IServiceCollection self, Type dbContextType)
        {

            var repositoryInterface = typeof(IRepository<>);
            var repositoryInterfaceWithPrimaryKey = typeof(IRepository<,>);
            var repositoryImplementation = typeof(EfCoreRepository<,>);
            var repositoryImplementationWithPrimaryKey = typeof(EfCoreRepository<,,>);

            foreach (var entityTypeInfo in EfCoreDbContextEntityFinder.GetEntityTypeInfos(dbContextType)) {

                var primaryKeyType = EntityHelper.GetPrimaryKeyType(entityTypeInfo.EntityType);
                if (primaryKeyType == typeof(int)) {
                    var genericRepositoryType = repositoryInterface.MakeGenericType(entityTypeInfo.EntityType);
                    //if ( !iocManager.AddIfNotContains<.Contains(genericRepositoryType)) {
                    var implType = repositoryImplementation.GetGenericArguments().Length == 1
                        ? repositoryImplementation.MakeGenericType(entityTypeInfo.EntityType)
                        : repositoryImplementation.MakeGenericType(entityTypeInfo.DeclaringType,
                            entityTypeInfo.EntityType);

                    self.AddTransient(genericRepositoryType, implType);
                    //}
                }

                else {
                    var genericRepositoryTypeWithPrimaryKey = repositoryInterfaceWithPrimaryKey.MakeGenericType(entityTypeInfo.EntityType, primaryKeyType);
                    //if (!ioc.Contains(genericRepositoryTypeWithPrimaryKey)) {
                    var implType = repositoryImplementationWithPrimaryKey.GetGenericArguments().Length == 2
                        ? repositoryImplementationWithPrimaryKey.MakeGenericType(entityTypeInfo.EntityType, primaryKeyType)
                        : repositoryImplementationWithPrimaryKey.MakeGenericType(entityTypeInfo.DeclaringType, entityTypeInfo.EntityType, primaryKeyType);

                    self.AddTransient(genericRepositoryTypeWithPrimaryKey, implType);

                    //}
                }
            }
        }

    }
}
