﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel;
using Windows.System;

namespace Monokrome.Helpers
{
    public static class NavigationHelper
    {
        public static void NavigateToUrl(string url)
        {
            NavigateToUrl(new Uri(url));
        }

        public static void NavigateToUrl(Uri uri)
        {
            Launcher.LaunchUriAsync(uri);
        }

        public static void RateApp()
        {
            Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName));
        }
    }
}
