using Monokrome.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
namespace Monokrome.WP8
{
    public class DataStorageManager : IDataStorageManager
    {
        private IsolatedStorageSettings dataStorage;

        public DataStorageManager()
        {
            this.dataStorage = IsolatedStorageSettings.ApplicationSettings;
        }
        public bool AddOrUpdateData(string key, string value)
        {
            bool valueChanged = false;

            // If the key exists
            if (this.dataStorage.Contains(key))
            {
                // If the value has changed
                if (this.dataStorage[key] != value)
                {
                    // Store the new value
                    this.dataStorage[key] = value;
                    this.dataStorage.Save();
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                this.dataStorage.Add(key, value);
                this.dataStorage.Save();
                valueChanged = true;
            }

            ////GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings changed", Key + " set to " + value.ToString(), string.Empty, 0);
            return valueChanged;
        }

        public string GetDataOrDefault(string key, string defaultValue)
        {
            // If the key exists, retrieve the value.
            if (this.dataStorage.Contains(key))
               return this.dataStorage[key].ToString();
            // Otherwise, use the default value.
            return defaultValue;
        }

        public void AddUser(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
