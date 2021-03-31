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

        private IEnumerable<ApiScope> GetAllApiScopes()
        {
            return _dbRepository.All<ApiScope>();
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

            return Task.FromResult(_dbRepository.Single<ApiResource>(a => a.Name == name));
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<ApiResource>(a => a.Scopes.Any(s => scopeNames.Contains(s)));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<IdentityResource>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

        public Resources GetAllResources()
        {
            var result = new Resources(GetAllIdentityResources(), GetAllApiResources(), GetAllApiScopes());
            return result;
        }

        private Func<IdentityResource, bool> BuildPredicate(Func<IdentityResource, bool> predicate)
        {
            return predicate;
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            var result = new Resources(GetAllIdentityResources(), GetAllApiResources(), GetAllApiScopes());
            return Task.FromResult(result);
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<IdentityResource>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<ApiScope>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            var list = _dbRepository.Where<ApiResource>(e => scopeNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            var list = _dbRepository.Where<ApiResource>(e => apiResourceNames.Contains(e.Name));

            return Task.FromResult(list.AsEnumerable());
        }
    }
}
