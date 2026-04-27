using ASC.Model.Models;
using ASC.Web.Areas.Configuration.Models;

namespace ASC.Web.Data
{
    public class MasterDataCache
    {
        public List<MasterDataKey> Keys { get; set; } = new List<MasterDataKey>();

        public List<MasterDataValue> Values { get; set; } = new List<MasterDataValue>();
    }
}