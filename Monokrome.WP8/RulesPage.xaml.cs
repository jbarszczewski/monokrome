using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Monokrome.WP8
{
    public partial class RulesPage : PhoneApplicationPage
    {
        public RulesPage()
        {
            InitializeComponent();
            this.Loaded += (sender, e) =>
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Rules");
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new System.Uri("/MenuPage.xaml", System.UriKind.Relative));
        }
    }
}