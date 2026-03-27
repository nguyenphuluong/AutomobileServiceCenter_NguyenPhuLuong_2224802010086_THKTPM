using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASC.Tests.TestUtilities
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public bool IsAvailable => true;

        public string Id => "FakeSessionId";

        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            if (_sessionStorage.ContainsKey(key))
                _sessionStorage[key] = value;
            else
                _sessionStorage.Add(key, value);
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_sessionStorage.ContainsKey(key) && _sessionStorage[key] != null)
            {
                value = _sessionStorage[key];
                return true;
            }

            value = null;
            return false;
        }
    }
}