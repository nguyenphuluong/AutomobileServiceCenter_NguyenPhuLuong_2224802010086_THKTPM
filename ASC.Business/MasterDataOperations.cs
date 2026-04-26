using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Model.Models;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

            return masterKeys
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<List<MasterDataKey>> GetMaserKeyByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<MasterDataKey>();
            }

            var key = name.Trim();
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

            return masterKeys
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        string.Equals(x.PartitionKey?.Trim(), key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(x.Name?.Trim(), key, StringComparison.OrdinalIgnoreCase)
                    ))
                .ToList();
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            if (key == null)
            {
                return false;
            }

            var now = DateTime.Now;

            key.Name = key.Name?.Trim();

            if (string.IsNullOrWhiteSpace(key.Name))
            {
                return false;
            }

            key.PartitionKey = string.IsNullOrWhiteSpace(key.PartitionKey)
                ? key.Name
                : key.PartitionKey.Trim();

            // Giữ RowKey tự sinh GUID. Nếu model chưa sinh thì mới sinh tại đây.
            key.RowKey = string.IsNullOrWhiteSpace(key.RowKey)
                ? Guid.NewGuid().ToString()
                : key.RowKey.Trim();

            key.IsDeleted = false;
            key.CreatedDate = key.CreatedDate == default ? now : key.CreatedDate;
            key.UpdatedDate = now;
            key.CreatedBy = string.IsNullOrWhiteSpace(key.CreatedBy) ? "admin" : key.CreatedBy;
            key.UpdatedBy = string.IsNullOrWhiteSpace(key.UpdatedBy) ? "admin" : key.UpdatedBy;

            var allKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

            // Không cho tạo trùng Master Key theo PartitionKey/Name
            var existingKey = allKeys.FirstOrDefault(x =>
                !x.IsDeleted &&
                (
                    string.Equals(x.PartitionKey?.Trim(), key.PartitionKey, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Name?.Trim(), key.Name, StringComparison.OrdinalIgnoreCase)
                ));

            if (existingKey == null)
            {
                await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
            }
            else
            {
                existingKey.Name = key.Name;
                existingKey.PartitionKey = key.PartitionKey;
                existingKey.IsActive = key.IsActive;
                existingKey.IsDeleted = false;
                existingKey.UpdatedDate = now;
                existingKey.UpdatedBy = key.UpdatedBy;

                _unitOfWork.Repository<MasterDataKey>().Update(existingKey);
            }

            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterKeyAsync(string orginalPartitionKey, MasterDataKey key)
        {
            if (key == null ||
                string.IsNullOrWhiteSpace(orginalPartitionKey) ||
                string.IsNullOrWhiteSpace(key.RowKey))
            {
                return false;
            }

            var masterKey = await _unitOfWork.Repository<MasterDataKey>()
                .FindAsync(orginalPartitionKey.Trim(), key.RowKey.Trim());

            if (masterKey == null)
            {
                return false;
            }

            masterKey.Name = key.Name?.Trim();
            masterKey.IsActive = key.IsActive;
            masterKey.IsDeleted = false;
            masterKey.UpdatedDate = DateTime.Now;
            masterKey.UpdatedBy = string.IsNullOrWhiteSpace(key.UpdatedBy) ? "admin" : key.UpdatedBy;

            _unitOfWork.Repository<MasterDataKey>().Update(masterKey);
            _unitOfWork.CommitTransaction();

            return true;
        }
        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new List<MasterDataValue>();
            }

            var partitionKey = key.Trim();

            var masterValues = await _unitOfWork.Repository<MasterDataValue>()
                .FindAllByPartitionKeyAsync(partitionKey);

            return masterValues
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();

            return masterValues
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.PartitionKey)
                .ThenBy(x => x.Name)
                .ToList();
        }

        public async Task<MasterDataValue> GetMasterValueByNameAsync(string key, string name)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var masterValues = await _unitOfWork.Repository<MasterDataValue>()
                .FindAllByPartitionKeyAsync(key.Trim());

            return masterValues.FirstOrDefault(x =>
                !x.IsDeleted &&
                string.Equals(x.Name?.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            if (value == null)
            {
                return false;
            }

            var now = DateTime.Now;

            value.PartitionKey = value.PartitionKey?.Trim();
            value.Name = value.Name?.Trim();

            if (string.IsNullOrWhiteSpace(value.PartitionKey) ||
                string.IsNullOrWhiteSpace(value.Name))
            {
                return false;
            }

            // Giữ RowKey tự sinh GUID. Nếu model chưa sinh thì mới sinh tại đây.
            value.RowKey = string.IsNullOrWhiteSpace(value.RowKey)
                ? Guid.NewGuid().ToString()
                : value.RowKey.Trim();

            value.IsDeleted = false;
            value.CreatedDate = value.CreatedDate == default ? now : value.CreatedDate;
            value.UpdatedDate = now;
            value.CreatedBy = string.IsNullOrWhiteSpace(value.CreatedBy) ? "admin" : value.CreatedBy;
            value.UpdatedBy = string.IsNullOrWhiteSpace(value.UpdatedBy) ? "admin" : value.UpdatedBy;

            var allValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();

            // Không tạo trùng value theo PartitionKey + Name
            var existingValue = allValues.FirstOrDefault(x =>
                !x.IsDeleted &&
                string.Equals(x.PartitionKey?.Trim(), value.PartitionKey, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Name?.Trim(), value.Name, StringComparison.OrdinalIgnoreCase));

            if (existingValue == null)
            {
                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
            }
            else
            {
                existingValue.Name = value.Name;
                existingValue.IsActive = value.IsActive;
                existingValue.IsDeleted = false;
                existingValue.UpdatedDate = now;
                existingValue.UpdatedBy = value.UpdatedBy;

                _unitOfWork.Repository<MasterDataValue>().Update(existingValue);
            }

            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            if (value == null ||
                string.IsNullOrWhiteSpace(originalPartitionKey) ||
                string.IsNullOrWhiteSpace(originalRowKey))
            {
                return false;
            }

            var masterValue = await _unitOfWork.Repository<MasterDataValue>()
                .FindAsync(originalPartitionKey.Trim(), originalRowKey.Trim());

            if (masterValue == null)
            {
                return false;
            }

            masterValue.Name = value.Name?.Trim();
            masterValue.IsActive = value.IsActive;
            masterValue.IsDeleted = false;
            masterValue.UpdatedDate = DateTime.Now;
            masterValue.UpdatedBy = string.IsNullOrWhiteSpace(value.UpdatedBy) ? "admin" : value.UpdatedBy;

            _unitOfWork.Repository<MasterDataValue>().Update(masterValue);
            _unitOfWork.CommitTransaction();

            return true;
        }

        public async Task<bool> UploadBulkMasterData(List<MasterDataValue> values)
        {
            if (values == null || !values.Any())
            {
                return false;
            }

            var now = DateTime.Now;

            var allMasterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

            var masterKeySet = allMasterKeys
                .Where(x => !string.IsNullOrWhiteSpace(x.PartitionKey))
                .Select(x => x.PartitionKey.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var item in values)
            {
                var partitionKey = item.PartitionKey?.Trim();

                if (string.IsNullOrWhiteSpace(partitionKey))
                {
                    continue;
                }

                if (!masterKeySet.Contains(partitionKey))
                {
                    var newMasterKey = new MasterDataKey
                    {
                        PartitionKey = partitionKey,
                        Name = partitionKey,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = now,
                        UpdatedDate = now,
                        CreatedBy = "admin",
                        UpdatedBy = "admin"
                    };

                    // Giữ RowKey tự sinh GUID
                    if (string.IsNullOrWhiteSpace(newMasterKey.RowKey))
                    {
                        newMasterKey.RowKey = Guid.NewGuid().ToString();
                    }

                    await _unitOfWork.Repository<MasterDataKey>().AddAsync(newMasterKey);
                    masterKeySet.Add(partitionKey);
                }
            }

            var allMasterValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();

            var existingValueMap = allMasterValues
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.PartitionKey) &&
                    !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => $"{x.PartitionKey.Trim()}|{x.Name.Trim()}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var addedValueSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in values)
            {
                var partitionKey = item.PartitionKey?.Trim();
                var name = item.Name?.Trim();

                if (string.IsNullOrWhiteSpace(partitionKey) ||
                    string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var mapKey = $"{partitionKey}|{name}";

                if (existingValueMap.TryGetValue(mapKey, out var existingValue))
                {
                    existingValue.Name = name;
                    existingValue.IsActive = item.IsActive;
                    existingValue.IsDeleted = false;
                    existingValue.UpdatedDate = now;
                    existingValue.UpdatedBy = "admin";

                    _unitOfWork.Repository<MasterDataValue>().Update(existingValue);
                }
                else
                {
                    if (addedValueSet.Contains(mapKey))
                    {
                        continue;
                    }

                    var newValue = new MasterDataValue
                    {
                        PartitionKey = partitionKey,
                        Name = name,
                        IsActive = item.IsActive,
                        IsDeleted = false,
                        CreatedDate = now,
                        UpdatedDate = now,
                        CreatedBy = "admin",
                        UpdatedBy = "admin"
                    };

                    // Giữ RowKey tự sinh GUID
                    if (string.IsNullOrWhiteSpace(newValue.RowKey))
                    {
                        newValue.RowKey = Guid.NewGuid().ToString();
                    }

                    await _unitOfWork.Repository<MasterDataValue>().AddAsync(newValue);
                    addedValueSet.Add(mapKey);
                }
            }

            _unitOfWork.CommitTransaction();

            return true;
        }
    }
}