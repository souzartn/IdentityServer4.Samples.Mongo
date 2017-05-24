using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using QuickstartIdentityServer.Quickstart.Interface;

namespace QuickstartIdentityServer.Quickstart.Store
{
    public class CustomResourceStore : IResourceStore
    {
        protected IRepository _dbRepository;

        public CustomResourceStore(IRepository repository)
        {
            _dbRepository = repository;
        }


        private IEnumerable<ApiResource> GetAllApiResources()
        {
            return _dbRepository.All<ApiResource>();
        }

        private IEnumerable<IdentityResource> GetAllIdentityResources()
        {
            return _dbRepository.All<IdentityResource>();
        }

        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            return Task.Run(() =>
            {
                return _dbRepository.Single<ApiResource>(a => a.Name == name);
            });
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return Task.Run(() =>
            {
                var list = _dbRepository.Where<ApiResource>(a => scopeNames.Contains(a.Name));

                var t = list.ToList();
                return list.AsEnumerable();
            });


        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return Task.Run(() =>
            {
                var list = _dbRepository.Where<IdentityResource>(e => scopeNames.Contains(e.Name));
                return list.AsEnumerable();
            });
        }

        public Task<Resources> GetAllResources()
        {
            return Task.Run(() =>
            {
                var result = new Resources(GetAllIdentityResources(), GetAllApiResources());
                return result;
            });
        }

        private Func<IdentityResource, bool> BuildPredicate(Func<IdentityResource, bool> predicate)
        {
            return predicate;
        }
    }
}
