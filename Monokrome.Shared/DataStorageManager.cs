using Monokrome.Interfaces;
using Monokrome.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Monokrome
{
    public class DataStorageManager : IDataStorageManager
    {
        public bool AddOrUpdateData(string key, string value)
        {
            bool result = false;
            if (ApplicationData.Current.RoamingSettings.Values.Keys.Contains(key))
                result = true;
            ApplicationData.Current.RoamingSettings.Values[key] = value;

            return result;
        }

        public string GetDataOrDefault(string key, string defaultValue)
        {
            if (ApplicationData.Current.RoamingSettings.Values.Keys.Contains(key))
                return ApplicationData.Current.RoamingSettings.Values[key].ToString();
            return defaultValue;
        }
    }
}
