using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Monokrome.WP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Monokrome.WP8
{
    public partial class MenuPage : PhoneApplicationPage
    {
        // Constructor
        public MenuPage()
        {
            InitializeComponent();
            this.Loaded += (sender, e) =>
                {
                    NavigationService.RemoveBackEntry();
                    GoogleAnalytics.EasyTracker.GetTracker().SendView("Main");
                };
            
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                  new System.Uri("/GamePage.xaml",
                        System.UriKind.Relative));
        }

        private void RulesButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(
                new System.Uri("/RulesPage.xaml",
                    System.UriKind.Relative));
        }

        private void SendMailButton_Click(object sender, RoutedEventArgs e)
        {
            var sendMailTask = new EmailComposeTask();
            sendMailTask.To = "beetrootsoup@outlook.com";
            sendMailTask.Subject = "Feedback from monokrome!";
            sendMailTask.Show();
        }

        private void RateAppButton_Click(object sender, RoutedEventArgs e)
        {
            RateMyApp.Helpers.FeedbackHelper.Default.Review();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}