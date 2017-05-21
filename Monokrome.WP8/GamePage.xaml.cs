using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using MonoGame.Framework.WindowsPhone;
using Monokrome.WP8.Resources;
using Monokrome.Interfaces;
using System.Threading.Tasks;
using Monokrome.DataModels;

namespace Monokrome.WP8
{
    public partial class GamePage : PhoneApplicationPage
    {
        private GameManager game;
        private IDataStorageManager dataStorageManager;

        // Constructor
        public GamePage()
        {
            InitializeComponent();

            this.game = XamlGame<GameManager>.Create("", this);
            this.dataStorageManager = new DataStorageManager();
            this.game.DataStorageManager = this.dataStorageManager;
            this.Loaded += (sender, e) =>
                {
                    GoogleAnalytics.EasyTracker.GetTracker().SendView("Game");
                };

            this.BackKeyPress += GamePage_BackKeyPress;
            this.game.Exiting += (sender, e) => App.Current.Terminate();
            this.game.GameStarted += () => this.Dispatcher.BeginInvoke(new Action(() => this.AddViewControl.Visibility = System.Windows.Visibility.Collapsed));
            this.game.GameStoped += () => this.Dispatcher.BeginInvoke(new Action(() => this.AddViewControl.Visibility = System.Windows.Visibility.Visible));
            this.game.GameOver += () => this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.AddViewControl.Visibility = System.Windows.Visibility.Visible;
                this.UpdatesOnlineScores();
            }));
            this.Unloaded += (sender, e) => this.game.StopGame();
            this.GotFocus += (sender, e) => this.game.PauseGame();
            this.LostFocus += (sender, e) => this.game.PauseGame();


            // remove after few versions (added in 1.4 along with online score
            {
                string userName = this.dataStorageManager.GetDataOrDefault("UserName", string.Empty);
                if (!string.IsNullOrEmpty(userName))
                    this.dataStorageManager.AddOrUpdateData("PlayerName", userName);

                string normalScore = this.dataStorageManager.GetDataOrDefault("normalMaxScore", "0");
                if (!normalScore.Equals("0"))
                    this.dataStorageManager.AddOrUpdateData("NormalMaxScore", normalScore);

                string hardScore = this.dataStorageManager.GetDataOrDefault("hardMaxScore", "0");
                if (!hardScore.Equals("0"))
                    this.dataStorageManager.AddOrUpdateData("HardMaxScore", hardScore);
            }

            // set user for online scoring
            this.game.RefreshScores();
            this.InitializeOnlineScoreManager();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void GamePage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.game.GameScreen == GameScreen.MainMenu && this.game.GameMenu.CurrentScreen == 0)
                e.Cancel = false;
            else
                this.game.PauseGame();
        }

        private async void InitializeOnlineScoreManager()
        {
            try
            {
                await OnlineScoreManager.Initialize("monokrome");
                string userName = this.dataStorageManager.GetDataOrDefault("PlayerName", string.Empty);
                await OnlineScoreManager.SetUser(userName);
                if (userName.Equals(string.Empty))
                    this.dataStorageManager.AddOrUpdateData("PlayerName", OnlineScoreManager.Player.Name);

                await this.UpdatesOnlineScores();
                await this.RetrieveHighscores();
            }
            catch { }
            finally
            {
                this.game.RefreshScores();
            }
        }

        private async Task UpdatesOnlineScores()
        {
            try
            {
                int normalLocalScore = int.Parse(this.dataStorageManager.GetDataOrDefault("NormalMaxScore", "0"));
                int hardLocalScore = int.Parse(this.dataStorageManager.GetDataOrDefault("HardMaxScore", "0"));
                Score normalOnlineScore = await OnlineScoreManager.GetScore(OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Normal")));
                Score hardOnlineScore = await OnlineScoreManager.GetScore(OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Hard")));

                if (normalOnlineScore == null)
                    OnlineScoreManager.PublishScore(OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Normal")), normalLocalScore.ToString());
                else if (normalLocalScore > int.Parse(normalOnlineScore.Value))
                {
                    normalOnlineScore.Value = normalLocalScore.ToString();
                    OnlineScoreManager.UpdateScore(normalOnlineScore);
                }
                else
                    this.dataStorageManager.AddOrUpdateData("NormalMaxScore", normalOnlineScore.Value);

                if (hardOnlineScore == null)
                    OnlineScoreManager.PublishScore(OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Hard")), hardLocalScore.ToString());
                else if (hardLocalScore > int.Parse(hardOnlineScore.Value))
                {
                    hardOnlineScore.Value = hardLocalScore.ToString();
                    OnlineScoreManager.UpdateScore(hardOnlineScore);
                }
                else
                    this.dataStorageManager.AddOrUpdateData("HardMaxScore", hardOnlineScore.Value);
            }
            catch { }
        }

        private async Task RetrieveHighscores()
        {
            try
            {
                var normalScores = await OnlineScoreManager.GetTopScores(5, OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Normal")));
                this.game.NormalHighscores = normalScores.ToList();
                var hardScores = await OnlineScoreManager.GetTopScores(5, OnlineScoreManager.GameVariants.First(x => x.Name.Equals("Hard")));
                this.game.HardHighscores = hardScores.ToList();
            }
            catch { }
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