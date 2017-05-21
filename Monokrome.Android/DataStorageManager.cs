using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Monokrome.Interfaces;
using Android.Preferences;

namespace Monokrome.Android
{
    class DataStorageManager : IDataStorageManager
    {
		private ISharedPreferences prefs;
		private Context context;
		public DataStorageManager(Context context)
		{
			this.context = context;
			this.prefs = PreferenceManager.GetDefaultSharedPreferences(this.context); 
		}

		public bool AddOrUpdateData(string key, string value)
        {
			ISharedPreferencesEditor editor = PreferenceManager.GetDefaultSharedPreferences(this.context).Edit();
			editor.PutString(key, value.ToString());
			editor.Apply();
            return true;
        }

		public string GetDataOrDefault(string key, string defaultValue)
		{
			return PreferenceManager.GetDefaultSharedPreferences (this.context).GetString (key, defaultValue.ToString ());
		//	return this.prefs.GetString (key, defaultValue.ToString());
        }
    }
}