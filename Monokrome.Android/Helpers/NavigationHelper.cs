using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;

using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Monokrome.Android;
using Rivets;

namespace Monokrome.Helpers
{
    public static class NavigationHelper
    {
		public static Activity1 MainActivity { get; set; }

        public static void NavigateToUrl(string url)
        {
            NavigateToUrl(new Uri(url));
        }

        public static void NavigateToUrl(Uri uri)
        {
			var androidUri = global::Android.Net.Uri.Parse (uri.AbsoluteUri);
			var intent = new Intent (Intent.ActionView, androidUri);   
			MainActivity.StartActivity (intent);  
        }

        public static void RateApp()
        {
			NavigateToUrl("https://play.google.com/store/apps/details?id=com.beetrootsoup.monokrome");
        }
    }
}
