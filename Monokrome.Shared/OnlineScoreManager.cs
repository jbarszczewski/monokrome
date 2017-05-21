using Microsoft.WindowsAzure.MobileServices;
using Monokrome.DataModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Monokrome
{
    public static class OnlineScoreManager
    {
        private static MobileServiceClient MobileService = new MobileServiceClient(
             "https://beetrootachievements.azure-mobile.net/",
            "UpStWOosSjUyBRtaFPfxsVJEVDMhLV53");

        public static Player Player;
        public static Game Game;
        public static List<GameVariant> GameVariants;

        public static async Task Initialize(string gameName)
        {
            var games = await MobileService.GetTable<Game>().Where(x => x.Name == gameName).ToEnumerableAsync();
            Game = games.First();
            GameVariants = await MobileService.GetTable<GameVariant>().Where(x => x.GameId == Game.Id).ToListAsync();
        }

        public static async Task<string> GenerateNewUserName()
        {
            bool isNameUnique = false;
            string newUserName = "Player";
            var randomGenerator = new Random();
            IEnumerable<Player> users = await MobileService.GetTable<Player>().ToEnumerableAsync();
            while (!isNameUnique)
            {
                string randomNumber = randomGenerator.Next(100000, 999999).ToString();
                if (users.Any(x => x.Name.Equals(newUserName + randomNumber)))
                    continue;
                newUserName += randomNumber;
                isNameUnique = true;
            }

            return newUserName;
        }

        public static async Task<Score> GetScore(GameVariant gameVariant)
        {
            var scores = await MobileService.GetTable<Score>()
                .Where(x => x.PlayerId == Player.Id && x.GameVariantId == gameVariant.Id)
                .ToEnumerableAsync();
            return scores.FirstOrDefault();
        }

        public static async Task<IEnumerable<Score>> GetTopScores(int numberOfScores, GameVariant gameVariant)
        {
            var scores = new List<Score>();
            var arguments = new Dictionary<string, string>
                                    {
                                        { "quantity", numberOfScores.ToString() },
                                        {
                                            "gamevariantid",
                                            "'"+ gameVariant.Id +"'" // add real id here
                                        }
                                    };
            JToken result = await MobileService.InvokeApiAsync("gettopscores", HttpMethod.Get, arguments);
            foreach (var resultObject in result.ToList())
            {
                var newScore = new Score
                {
                    Id = (string)resultObject["Id"],
                    Value = (string)resultObject["Value"],
                    GameVariantId = gameVariant.Id,
                    PlayerId = (string)resultObject["PlayerId"],
                    DateCreated = resultObject["DateCreated"].Value<DateTime>()
                };

                scores.Add(newScore);
            }

			return scores.OrderByDescending(x => int.Parse(x.Value));
        }

        public static async void UpdateScore(Score updatedScore)
        {
            await MobileService.GetTable<Score>().UpdateAsync(updatedScore);
        }

        public static async void PublishScore(GameVariant gameVariant, string score)
        {
            var scores = await MobileService.GetTable<Score>()
                .Where(x => x.PlayerId == Player.Id && x.GameVariantId == gameVariant.Id)
                .ToEnumerableAsync();
            var scoreEntry = scores.FirstOrDefault();
            if (scoreEntry == null)
            {
                scoreEntry = new Score { PlayerId = Player.Id, GameVariantId = gameVariant.Id, Value = score, DateCreated = DateTime.Now };
                await MobileService.GetTable<Score>().InsertAsync(scoreEntry);
            }
            else
            {
                scoreEntry.Value = score;
                await MobileService.GetTable<Score>().UpdateAsync(scoreEntry);
            }
        }

        public static async Task SetUser(string userName)
        {
            if (userName.Equals(string.Empty))
            {
                userName = await GenerateNewUserName();
                var user = new Player
                {
                    Name = userName,
                    DateCreated = DateTime.Now,
                    DeviceIdentifier = string.Empty,
                    OSName = string.Empty,
                    Password = string.Empty
                };

                await MobileService.GetTable<Player>().InsertAsync(user);
                Player = user;
                return;
            }

            var users = await MobileService.GetTable<Player>().Where(x => x.Name == userName).ToEnumerableAsync();
            if (users.Any())
                Player = users.FirstOrDefault();
            else
            {
                var user = new Player
                {
                    Name = userName,
                    DateCreated = DateTime.Now,
                    DeviceIdentifier = string.Empty,
                    OSName = string.Empty,
                    Password = string.Empty
                };

                await MobileService.GetTable<Player>().InsertAsync(user);
                Player = user;
            }
        }
    }
}
