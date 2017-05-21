using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Monokrome.Interfaces;
using Monokrome.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monokrome.Helpers;
using Monokrome.DataModels;
using Microsoft.Xna.Framework.Audio;

namespace Monokrome
{
    // public delegate void NavigateToMainMenuEventHandler(object sender, EventArgs e);
    public delegate void GameStart();
    public delegate void GameOver();
    public delegate void GameStop();

    public class GameManager : Microsoft.Xna.Framework.Game
    {
        public const int VirtualScreenWidth = 1920;
        public const int VirtualScreenHeight = 1080;

        private Vector3 screenScale;
        private Matrix scaleMatrix;
        private IDataStorageManager dataStorageManager;
        private GraphicsDeviceManager graphicsManager;
        private SpriteBatch spriteBatch;
        private Texture2D obstacleTextureBlack;
        private Texture2D obstacleTextureWhite;
        private Texture2D obstacleIntersectionTexture;    
        private SpriteFont gameScoreFont;
        private SpriteFont gameMenuFont;
        private SpriteFont gameTitleFont;
        private SoundEffect pickSound;
        private Monokrome.GameLogic.Player player;
        private IList<Obstacle> obstacles;
        private IList<Rectangle> intersections;
        private IList<Tuple<Obstacle, Obstacle>> intersectingObstaclesPairs;
        private Random randomGenerator;
        private int obstacleSize = 200;
        private int numberOfObstacles = 10;
        private int baseSpeed = 1;
        private int dificultyMultiplier = 1;
        private int currentSpeed = 1;
        private int speedVariation = 6;
        private int maxSpeed = 50;
        private int speedUpThreshold = 5;
        private string difficulty = "Normal";
        private PowerUpType playerPowerUp = PowerUpType.None;
        private PowerUpType activePowerUp = PowerUpType.None;
        private int gameScore;
        private Dictionary<string, string> maxScore;
        private bool playSound;
        private bool isInitialized;
        private bool wasGameOverEventFired = true;

        public event GameStart GameStarted;
        public event GameStop GameStoped;
        public event GameOver GameOver;
        public GameState GameState { get; private set; }
        public GameScreen GameScreen { get; private set; }
        public GameMenu GameMenu { get; private set; }
        public IList<Score> NormalHighscores { get; set; }
        public IList<Score> HardHighscores { get; set; }

        public GameManager()
        {
            this.graphicsManager = new GraphicsDeviceManager(this);
            TouchPanel.EnabledGestures =
                GestureType.FreeDrag |
                GestureType.Tap |
                GestureType.DoubleTap;
            this.Content.RootDirectory = "Content";
            this.GameState = GameState.Stoped;
            this.GameScreen = GameScreen.MainMenu;
            this.IsMouseVisible = true;
            this.maxScore = new Dictionary<string, string>() { { "Normal", "0" }, { "Hard", "0" } };
        }

        public IDataStorageManager DataStorageManager
        {
            get { return this.dataStorageManager; }
            set { this.dataStorageManager = value; }
        }

        public void RefreshScores()
        {
            this.maxScore["Normal"] = this.dataStorageManager.GetDataOrDefault("NormalMaxScore", "0");
            this.maxScore["Hard"] = this.dataStorageManager.GetDataOrDefault("HardMaxScore", "0");
            try
            {
                HighscoreList normalScorelist = this.GameMenu.GetVisual("normalHighscoreList") as HighscoreList;
                if (normalScorelist != null && this.NormalHighscores.Any())
                {
                    normalScorelist.TextRows.Clear();
                    foreach (Score score in this.NormalHighscores)
                    {
                        normalScorelist.TextRows.Add((this.NormalHighscores.IndexOf(score) + 1).ToString() + ". " + score.Value);
                    }
                }

                HighscoreList hardScorelist = this.GameMenu.GetVisual("hardHighscoreList") as HighscoreList;
                if (hardScorelist != null && this.HardHighscores.Any())
                {
                    hardScorelist.TextRows.Clear();
                    foreach (Score score in this.HardHighscores)
                    {
                        hardScorelist.TextRows.Add((this.HardHighscores.IndexOf(score) + 1).ToString() + ". " + score.Value);
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        private async Task<IEnumerable<Score>> RetrieveHighscores(string dificultyName)
        {
            return await OnlineScoreManager.GetTopScores(5, OnlineScoreManager.GameVariants.First(x => x.Name.Equals(dificultyName)));
        }

        public void StartGame()
        {
            this.GameScreen = GameScreen.Game;
            this.GameState = GameState.Playing;
            this.GameMenu.DeactivateVisual("resume");
            this.wasGameOverEventFired = false;
            if (this.isInitialized)
                this.ResetGame();
            if (this.GameStarted != null)
                this.GameStarted();
        }

        public void ResumeGame()
        {
            this.GameScreen = GameScreen.Game;
            this.GameState = GameState.Playing;
            if (this.GameStarted != null)
                this.GameStarted();
        }

        public void PauseGame()
        {
            if (this.GameState == GameState.Playing)
                this.GameMenu.ActivateVisual("resume");
            this.GameScreen = GameScreen.MainMenu;
            this.GameState = GameState.Paused;
            this.GameMenu.CurrentScreen = 0;
            if (this.GameStoped != null)
                this.GameStoped();
        }

        public void StopGame()
        {
            this.GameScreen = GameScreen.MainMenu;
            this.GameState = GameState.Stoped;
            this.GameMenu.CurrentScreen = 0;
            if (this.GameStoped != null)
                this.GameStoped();
        }

        public void GoToSettings()
        {
            this.GameScreen = GameScreen.Settings;
        }

        public void GoToMenu()
        {
            this.GameScreen = GameScreen.MainMenu;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // initialize game menu
            this.GameMenu = new GameMenu();

            // initialize game objects          
            this.obstacles = new List<Obstacle>();
            this.intersections = new List<Rectangle>();
            this.intersectingObstaclesPairs = new List<Tuple<Obstacle, Obstacle>>();
            this.randomGenerator = new Random();
            this.currentSpeed = this.baseSpeed;

            base.Initialize();

            float scaleX = (float)GraphicsDevice.Viewport.Width / (float)VirtualScreenWidth;
            float scaleY = (float)GraphicsDevice.Viewport.Height / (float)VirtualScreenHeight;
            this.screenScale = new Vector3(scaleX, scaleY, 1.0f);
            this.scaleMatrix = Matrix.CreateScale(this.screenScale);
            InputHelper.ScreenScale = this.screenScale;

            if (this.GameState == GameState.Playing)
            {
                this.ResetGame();
            }

            this.isInitialized = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //zmienic na male litery
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);
            this.gameScoreFont = this.Content.Load<SpriteFont>("scorefont");
            this.gameMenuFont = this.Content.Load<SpriteFont>("menufont");
            this.gameTitleFont = this.Content.Load<SpriteFont>("titlefont");
            this.obstacleTextureBlack = this.Content.Load<Texture2D>("obstacleblack");
            this.obstacleTextureWhite = this.Content.Load<Texture2D>("obstaclewhite");
            this.obstacleIntersectionTexture = this.Content.Load<Texture2D>("obstacleintersection");
            var playerTextureBlack = this.Content.Load<Texture2D>("playerblack");
            var playerTextureWhite = this.Content.Load<Texture2D>("playerwhite");
            var buttonTexture = this.Content.Load<Texture2D>("menubutton");
            var buttonTexturePressed = this.Content.Load<Texture2D>("menubuttonpressed");
            var facebookButtonTexture = this.Content.Load<Texture2D>("facebookicon");
            var twitterButtonTexture = this.Content.Load<Texture2D>("twittericon");
            var wwwButtonTexture = this.Content.Load<Texture2D>("wwwicon");
            var rateButtonTexture = this.Content.Load<Texture2D>("staricon");
            var soundButtonTexture = this.Content.Load<Texture2D>("soundicon");
            var nosoundButtonTexture = this.Content.Load<Texture2D>("nosoundicon");

            // set up player
            this.player = new Monokrome.GameLogic.Player(Color.White, Color.Black, playerTextureWhite, playerTextureBlack, new Rectangle(0, 0, VirtualScreenWidth, VirtualScreenHeight));
            this.player.Size = new Vector2(50, 50);
            this.player.Speed = 1.0f;
            this.player.HorizontalMinOffset = -50;
            this.player.HorizontalMaxOffset = 200;
            this.player.HorizontalNeutralPosition = 100;

            // set up menu
            var startButton = new Button
            {
                Name = "start",
                Position = new Vector2(590, 250),
                Text = "start",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 15),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => this.GameMenu.CurrentScreen = 1),
                IsActive = true
            };
            this.GameMenu.AddVisual(startButton, 0);

            var scoresButton = new Button
            {
                Name = "scores",
                Position = new Vector2(590, 500),
                Text = "scores",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 15),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => this.GameMenu.CurrentScreen = 2),
                IsActive = true
            };
            this.GameMenu.AddVisual(scoresButton, 0);

            var resumeButton = new Button
            {
                Name = "resume",
                Position = new Vector2(590, 750),
                Text = "resume",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 15),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => this.ResumeGame()),
                IsActive = false
            };
            this.GameMenu.AddVisual(resumeButton, 0);

            var facebookButton = new Button
            {
                Name = "facebook",
                Position = new Vector2(1700, 50),
                Texture = facebookButtonTexture,
                TexturePressed = facebookButtonTexture,
                ClickAction = new Action(() => NavigationHelper.NavigateToUrl("https://www.facebook.com/beetrootsoupnet")),
                IsActive = true
            };
            this.GameMenu.AddVisual(facebookButton, 0);

            var twitterButton = new Button
            {
                Name = "twitter",
                Position = new Vector2(1700, 200),
                Texture = twitterButtonTexture,
                TexturePressed = twitterButtonTexture,
                ClickAction = new Action(() => NavigationHelper.NavigateToUrl("https://twitter.com/beetrootSOUPnet")),
                IsActive = true
            };
            this.GameMenu.AddVisual(twitterButton, 0);

            var wwwButton = new Button
            {
                Name = "www",
                Position = new Vector2(1700, 350),
                Texture = wwwButtonTexture,
                TexturePressed = wwwButtonTexture,
                ClickAction = new Action(() => NavigationHelper.NavigateToUrl("http://beetrootsoup.net")),
                IsActive = true
            };
            this.GameMenu.AddVisual(wwwButton, 0);
            
            var rateButton = new Button
            {
                Name = "rate",
                Position = new Vector2(1700, 500),
                Texture = rateButtonTexture,
                TexturePressed = rateButtonTexture,
                ClickAction = new Action(() => NavigationHelper.RateApp()),
                IsActive = true
            };
            this.GameMenu.AddVisual(rateButton, 0);

            var soundButton = new Button
            {
                Name = "rate",
                Position = new Vector2(1700, 650),
                Texture = soundButtonTexture,
                TexturePressed = soundButtonTexture,
                IsActive = true
            };
            soundButton.ClickAction = new Action(() =>
                    {
                        this.playSound = !this.playSound;
                        if (this.playSound)
                            soundButton.Texture = soundButton.TexturePressed = soundButtonTexture;
                        else
                            soundButton.Texture = soundButton.TexturePressed = nosoundButtonTexture;
                    });
            this.GameMenu.AddVisual(soundButton, 0);

            var normalButton = new Button
            {
                Name = "normal",
                Position = new Vector2(590, 250),
                Text = "normal",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 20),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => { this.difficulty = "Normal"; this.baseSpeed = 1; this.maxSpeed = 10; this.dificultyMultiplier = 2; this.speedUpThreshold = 10; this.StartGame(); }),
                IsActive = true
            };
            this.GameMenu.AddVisual(normalButton, 1);

            var hardButton = new Button
            {
                Name = "hard",
                Position = new Vector2(590, 500),
                Text = "hard",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 20),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => { this.difficulty = "Hard"; this.baseSpeed = 1; this.maxSpeed = 50; this.dificultyMultiplier = 2; this.speedUpThreshold = 5; this.StartGame(); }),
                IsActive = true
            };
            this.GameMenu.AddVisual(hardButton, 1);

            var backButton = new Button
            {
                Name = "back",
                Position = new Vector2(590, 750),
                Text = "back",
                TextFont = this.gameMenuFont,
                TextColor = Color.Black,
                TextPositionOffset = new Vector2(-20, 20),
                Texture = buttonTexture,
                TexturePressed = buttonTexturePressed,
                ClickAction = new Action(() => { this.GameMenu.CurrentScreen = 0; }),
                IsActive = true
            };
            this.GameMenu.AddVisual(backButton, 2);

            var normalHighscoreList = new HighscoreList
            {
                Name = "normalHighscoreList",
                Position = new Vector2(300, 250),
                TextFont = this.gameScoreFont,
                TextColor = Color.Black,
                HeaderText = "normal",
                TextRows = new List<string> { "no data yet" },
                IsActive = true
            };
            this.GameMenu.AddVisual(normalHighscoreList, 2);

            var hardHighscoreList = new HighscoreList
            {
                Name = "hardHighscoreList",
                Position = new Vector2(700, 250),
                TextFont = this.gameScoreFont,
                TextColor = Color.Black,
                HeaderText = "hard",
                TextRows = new List<string>(),
                IsActive = true
            };
            this.GameMenu.AddVisual(hardHighscoreList, 2);

            this.GameMenu.CurrentScreen = 0;

            // Load sound
            this.pickSound = this.Content.Load<SoundEffect>("picksound");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if ANDROID
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				if (this.GameScreen == GameScreen.MainMenu && this.GameMenu.CurrentScreen == 0)
					Exit();
				else
					this.PauseGame ();
			}
#endif

            InputHelper.RefreshInput();
            if (this.GameScreen.Equals(GameScreen.MainMenu))
            {
                this.GameMenu.Update(gameTime);
            }

            else if (this.GameScreen.Equals(GameScreen.Game) && this.GameState.Equals(GameState.Playing))
            {
                this.intersections.Clear();
                this.intersectingObstaclesPairs.Clear();
                this.player.Update(gameTime);
                foreach (Obstacle obstacle in this.obstacles)
                {
                    obstacle.Update(gameTime);
                    int i = this.obstacles.IndexOf(obstacle);
                    foreach (Obstacle nextObstacle in this.obstacles.Skip(++i))
                        if (obstacle.Rectangle.Intersects(nextObstacle.Rectangle) && !obstacle.CurrentColour.Equals(nextObstacle.CurrentColour))
                        {
                            this.intersections.Add(Rectangle.Intersect(obstacle.Rectangle, nextObstacle.Rectangle));
                            this.intersectingObstaclesPairs.Add(new Tuple<Obstacle, Obstacle>(obstacle, nextObstacle));
                        }

                    if (obstacle.Rectangle.Intersects(this.player.Rectangle))
                    {
                        if (obstacle.CurrentColour.Equals(this.player.CurrentColour))
                        {
                            if (obstacle.IsActive)
                                obstacle.IsActive = false;

                            if (this.intersectingObstaclesPairs.Any(x => x.Item1.Equals(obstacle) || x.Item2.Equals(obstacle)))
                            {
                                // nie kasuje kwadratu drugiego
                                foreach (var intesectedObstaclePair in this.intersectingObstaclesPairs.Where(x => x.Item1.Equals(obstacle) || x.Item2.Equals(obstacle)))
                                {
                                    if (intesectedObstaclePair.Item1.IsActive)
                                        intesectedObstaclePair.Item1.IsActive = false;

                                    if (intesectedObstaclePair.Item2.IsActive)
                                        intesectedObstaclePair.Item2.IsActive = false;

                                    this.gameScore++;
                                    if (this.gameScore % this.speedUpThreshold == 0 && this.currentSpeed <= this.maxSpeed)
                                        this.currentSpeed++;
                                }

                                this.player.SwitchColour();
                            }

                            if(this.playSound)
                                this.pickSound.Play();
                            this.gameScore++;

                            if (this.gameScore % this.speedUpThreshold == 0 && this.currentSpeed <= this.maxSpeed)
                                this.currentSpeed++;
                        }
                        else
                        {
                            if (this.gameScore > int.Parse(this.maxScore[this.difficulty]))
                            {
                                this.maxScore[this.difficulty] = this.gameScore.ToString();
                                this.dataStorageManager.AddOrUpdateData(this.difficulty + "MaxScore", this.maxScore[this.difficulty]);
                            }

                            this.GameState = GameState.GameOver;
                            return;
                        }
                    }
                }

                IEnumerable<Obstacle> inactiveObstacles = this.obstacles.Where(x => !x.IsActive).ToList();

                foreach (Obstacle obstacle in inactiveObstacles)
                {
                    this.obstacles.Remove(obstacle);
                    Obstacle newObstacle = CreateRandomObstacle();
                    this.obstacles.Add(newObstacle);
                }

                var touchGesture = InputHelper.GetTouchGesture();
                if ((InputHelper.CurrentKeyboardState.IsKeyUp(Keys.Space) && InputHelper.PreviousKeyboardState.IsKeyDown(Keys.Space)) || touchGesture.GestureType == GestureType.DoubleTap)
                    if (this.playerPowerUp != PowerUpType.None)
                    {
                        this.activePowerUp = this.playerPowerUp;
                        this.playerPowerUp = PowerUpType.None;
                    }
            }

            else if (this.GameScreen.Equals(GameScreen.Game) && this.GameState == GameState.GameOver)
            {
                if (!this.wasGameOverEventFired)
                {
                    if (this.GameOver != null)
                        this.GameOver();
                    this.wasGameOverEventFired = true;
                }
                var mouseState = Mouse.GetState();
                if (mouseState.LeftButton == ButtonState.Pressed)
                    this.StartGame();

                var touchGesture = InputHelper.GetTouchGesture();
                if ((InputHelper.CurrentKeyboardState.IsKeyUp(Keys.Space) && InputHelper.PreviousKeyboardState.IsKeyDown(Keys.Space)) || touchGesture.GestureType == GestureType.DoubleTap)
                    this.StartGame();

                if (InputHelper.CurrentKeyboardState.IsKeyUp(Keys.Escape) && InputHelper.PreviousKeyboardState.IsKeyDown(Keys.Escape))
                    this.StopGame();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, this.scaleMatrix);

            // main menu
            if (this.GameScreen.Equals(GameScreen.MainMenu))
            {
                this.GraphicsDevice.Clear(Color.White);
                string title;
                switch (this.GameMenu.CurrentScreen)
                {
                    case 0:
                        title = "monokrome";
                        break;
                    case 1:
                        title = "difficulty";
                        break;
                    case 2:
                        title = "global scores";
                        break;
                    default:
                        title = "monokrome";
                        break;
                }

                this.spriteBatch.DrawString(this.gameTitleFont, title, new Vector2(20, -50), Color.Black);

                if (this.GameMenu.CurrentScreen == 2)
                {
                    this.spriteBatch.DrawString(this.gameScoreFont, "personal scores", new Vector2(1100, 250), Color.Black);
                    this.spriteBatch.DrawString(this.gameScoreFont, "Normal", new Vector2(1100, 330), Color.Black);
                    this.spriteBatch.DrawString(this.gameScoreFont, this.maxScore["Normal"], new Vector2(1100 + this.gameScoreFont.MeasureString("Normal").X + 50, 330), Color.Black);
                    this.spriteBatch.DrawString(this.gameScoreFont, "Hard", new Vector2(1100, 410), Color.Black);
                    this.spriteBatch.DrawString(this.gameScoreFont, this.maxScore["Hard"], new Vector2(1100 + this.gameScoreFont.MeasureString("Hard").X + 50, 410), Color.Black);
                }

                this.GameMenu.Draw(this.spriteBatch);
            }

            // playing game
            else if (this.GameScreen.Equals(GameScreen.Game) && this.GameState.Equals(GameState.Playing))
            {
                this.GraphicsDevice.Clear(Color.Gray);

                // Draw obstacles
                foreach (Obstacle obstacle in this.obstacles)
                    obstacle.Draw(this.spriteBatch);
                // Draw intersections
                foreach (Rectangle intersection in this.intersections)
                    this.spriteBatch.Draw(this.obstacleIntersectionTexture, intersection, Color.White);

                // Draw player
                this.player.Draw(this.spriteBatch);

                // speed
                string speedString = "speed: " + this.currentSpeed + "/" + this.maxSpeed;
                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    speedString,
                    new Vector2(50, 50),
                    Color.DarkGray);

                // player score
                string scoreString = "score: " + this.gameScore + " (" + this.maxScore[this.difficulty] + ")";
                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    scoreString,
                    new Vector2(VirtualScreenWidth - this.gameScoreFont.MeasureString(scoreString).X - 50, 50),
                    Color.DarkGray);
            }

            // game over
            else if (this.GameScreen.Equals(GameScreen.Game) && this.GameState == GameState.GameOver)
            {
                this.GraphicsDevice.Clear(Color.White);
                // speed
                string speedString = "speed: " + this.currentSpeed + "/" + this.maxSpeed;
                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    speedString,
                    new Vector2(50, 50),
                    Color.Black);

                // player score
                string scoreString = "score: " + this.gameScore + " (" + this.maxScore[this.difficulty] + ")";
                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    scoreString,
                    new Vector2(VirtualScreenWidth - this.gameScoreFont.MeasureString(scoreString).X - 50, 50),
                    Color.Black);

                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    "game over",
                     new Vector2(
                    (VirtualScreenWidth - this.gameScoreFont.MeasureString("game over").X) / 2,
                        600),
                    Color.Black);

                this.spriteBatch.DrawString(
                    this.gameScoreFont,
                    "double tap to restart",
                    new Vector2(
                        (VirtualScreenWidth - this.gameScoreFont.MeasureString("double tap to restart").X) / 2,
                        660),
                        Color.Black);
            }

            this.spriteBatch.End();
            base.Draw(gameTime);
        }

        private Obstacle CreateRandomObstacle()
        {
            Obstacle newObstacle = new Obstacle(Color.Black, Color.White, this.obstacleTextureBlack, this.obstacleTextureWhite, new Rectangle(0, 0, VirtualScreenWidth, VirtualScreenHeight));
            newObstacle.Size = new Vector2(this.obstacleSize, this.obstacleSize);
            newObstacle.Position = new Vector2(VirtualScreenWidth + this.randomGenerator.Next(this.obstacleSize * 5), this.randomGenerator.Next(VirtualScreenHeight - this.obstacleSize));
            newObstacle.Speed = (this.currentSpeed * this.dificultyMultiplier) + this.randomGenerator.Next(this.speedVariation);
            newObstacle.IsActive = true;
            if (this.randomGenerator.Next(0, 2) == 1)
            {
                newObstacle.SwitchColour();
            }

            return newObstacle;
        }

        private void ResetGame()
        {
            this.gameScore = 0;
            this.currentSpeed = this.baseSpeed;
            this.obstacles.Clear();
            this.player.Position = new Vector2(100, 515);

            // Create obstacles
            for (int i = 0; i < numberOfObstacles; i++)
            {
                Obstacle newObstacle = CreateRandomObstacle();
                this.obstacles.Add(newObstacle);
            }
        }
    }
}
