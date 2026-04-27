using ASC.Business.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ASC.Web.Data
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly IMasterDataOperations _masterData;

        private const string MasterDataCacheName = "MasterDataCache";

        public MasterDataCacheOperations(
            IDistributedCache cache,
            IMasterDataOperations masterData)
        {
            _cache = cache;
            _masterData = masterData;
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var masterDataCache = new MasterDataCache
            {
                Keys = (await _masterData.GetAllMasterKeysAsync())
                    .Where(p => p.IsActive == true)
                    .ToList(),

                Values = (await _masterData.GetAllMasterValuesAsync())
                    .Where(p => p.IsActive == true)
                    .ToList()
            };

            var jsonData = JsonSerializer.Serialize(masterDataCache);

            await _cache.SetStringAsync(MasterDataCacheName, jsonData);
        }

        public async Task<MasterDataCache> GetMasterDataCacheAsync()
        {
            var jsonData = await _cache.GetStringAsync(MasterDataCacheName);

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                await CreateMasterDataCacheAsync();
                jsonData = await _cache.GetStringAsync(MasterDataCacheName);
            }

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new MasterDataCache();
            }

            return JsonSerializer.Deserialize<MasterDataCache>(jsonData)
                   ?? new MasterDataCache();
        }
    }
}