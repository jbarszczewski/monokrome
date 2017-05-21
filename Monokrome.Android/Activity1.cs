using Android.App;
using Android.Content.PM;
using Android.OS;
using Monokrome.Interfaces;
using Android.Widget;
using Android.Views;
using Android.Gms.Ads;
using Android.Gms.Analytics;
using System;
using System.Threading.Tasks;
using Monokrome.DataModels;
using System.Linq;
using Monokrome.Helpers;

namespace Monokrome.Android
{
	[Activity (Label = "monokrome"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{
		private AdView adView;
		private GameManager game;
		private IDataStorageManager dataStorageManager;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			NavigationHelper.MainActivity = this;
			//GameManager.Activity = this;
			this.game = new GameManager ();
			this.game.Content.RootDirectory = "Content";
			this.dataStorageManager = new DataStorageManager (this);
			this.game.DataStorageManager = this.dataStorageManager;
			this.game.RefreshScores ();

			var frameLayout = new FrameLayout (this);
			var linearLayout = new LinearLayout (this);

			linearLayout.Orientation = Orientation.Horizontal;
			linearLayout.SetGravity (GravityFlags.CenterHorizontal | GravityFlags.Bottom);
			frameLayout.AddView ((View)game.Services.GetService (typeof(View)));

			this.adView = new AdView (this);
			this.adView.AdUnitId = "ca-app-pub-2198044040722697/6467970367";
			this.adView.AdSize = AdSize.SmartBanner;

			linearLayout.AddView (this.adView);
			frameLayout.AddView (linearLayout);
			SetContentView (frameLayout);

			try {
				// Initiate a generic request.
				var adRequestBuilder = new AdRequest.Builder ();

				// Load the adView with the ad request.
				this.adView.LoadAd (adRequestBuilder.Build ());
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}

			//GoogleAnalytics newAnalytics = new GoogleAnalytics ();
			//Tracker tracker = newAnalytics.NewTracker (1);
			//tracker.SetAppId ("UA-40024608-5");
			this.game.GameStarted += () => {
				this.adView.Pause ();
				this.adView.Visibility = ViewStates.Invisible;
			};
			this.game.GameStoped += () => {
				this.adView.Resume ();
				this.adView.Visibility = ViewStates.Visible;
				this.adView.BringToFront ();
			};
			this.game.GameOver += () => {
				this.adView.Resume ();
				this.adView.Visibility = ViewStates.Visible;
				this.adView.BringToFront ();
				this.UpdatesOnlineScores ();
			};

			// remove after few versions (added in 1.4 along with online score
			{
				string userName = this.dataStorageManager.GetDataOrDefault("UserName", string.Empty);
				if (!string.IsNullOrEmpty(userName))
					this.dataStorageManager.AddOrUpdateData("PlayerName", userName);

				string normalScore = this.dataStorageManager.GetDataOrDefault ("normalMaxScore", "0");
				if (!normalScore.Equals ("0"))
					this.dataStorageManager.AddOrUpdateData ("NormalMaxScore", normalScore);

				string hardScore = this.dataStorageManager.GetDataOrDefault ("hardMaxScore", "0");
				if (!hardScore.Equals ("0"))
					this.dataStorageManager.AddOrUpdateData ("HardMaxScore", hardScore);
			}

			this.game.RefreshScores();
			this.InitializeOnlineScoreManager ();
			this.game.Run ();
		}

		public override void OnBackPressed ()
		{
			if (this.game.GameScreen == GameScreen.MainMenu && this.game.GameMenu.CurrentScreen == 0)
				base.OnBackPressed ();
			else
				this.game.PauseGame ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			//	this.game.PauseGame ();
			this.adView.Pause ();
		}

		protected override void OnResume ()
		{
			// TODO: Tu sie zawiesza
			base.OnResume ();
			//	this.game.ResumeGame ();
			this.adView.Resume ();
		}

		protected override void OnDestroy ()
		{
			this.adView.Destroy ();
			base.OnDestroy ();
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
			catch(Exception ex) 
			{
			}
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
	}
}

