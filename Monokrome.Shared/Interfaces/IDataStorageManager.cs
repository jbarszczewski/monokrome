using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monokrome.Interfaces
{
    public interface IDataStorageManager
    {
		bool AddOrUpdateData(string key, string value);

		string GetDataOrDefault(string key, string defaultValue);
    }
}
