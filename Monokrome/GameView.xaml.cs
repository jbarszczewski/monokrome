using Microsoft.WindowsAzure.MobileServices;
using MonoGame.Framework;
using System.Linq;
using System;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Monokrome.DataModels;
using Monokrome.Interfaces;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Monokrome
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameView : SwapChainBackgroundPanel
    {
        readonly private GameManager game;
        readonly private IDataStorageManager dataStorageManager;

        public GameView(LaunchActivatedEventArgs args)
        {
            this.InitializeComponent();
            this.game = XamlGame<GameManager>.Create(args.Arguments, Window.Current.CoreWindow, this);
            this.dataStorageManager = new DataStorageManager();
            this.game.DataStorageManager = this.dataStorageManager;
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Game");
            // will prevent ad from stealing focus
            this.BannerAd.IsEnabled = false;
            SettingsPane.GetForCurrentView().CommandsRequested += SettingCharmManager_CommandsRequested;
            this.game.Exiting += (sender, e) => App.Current.Exit();
            this.Unloaded += (sender, e) => this.game.StopGame();
            this.LostFocus += (sender, e) => this.game.PauseGame();
            this.game.GameStarted += async () => await Window.Current.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => { this.BannerAd.Visibility = Visibility.Collapsed; });
            this.game.GameStoped += async () => await Window.Current.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => { this.BannerAd.Visibility = Visibility.Visible; });
            this.game.GameOver += async () => await Window.Current.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () =>
                {
                    this.BannerAd.Visibility = Visibility.Visible;
                    this.UpdatesOnlineScores();
                });

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

            this.game.RefreshScores();
            // set user for online scoring
            this.InitializeOnlineScoreManager();
        }

        private void SettingCharmManager_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy Policy", PrivacyPolicyLink));
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
                this.game.RefreshScores();
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

        private async void PrivacyPolicyLink(IUICommand command)
        {
            Uri uri = new Uri("http://beetrootsoup.net/monokrome_privacy_policy.html");
            await Launcher.LaunchUriAsync(uri);
        }
    }
}
