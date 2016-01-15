using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardGameServer.ServiceModel;
using CardGameServer.ServiceModel.Types;
using ServiceStack;

namespace CardGameServer.ServiceInterface
{
    public class BlackJackService : Service
    {
        private const string AllGamesSummaries = "ALL_GAMES";
        private const string NextGame = "NEXT_GAME";

        private CreateGameInstanceResponse CreateGame(CreateGameInstance request)
        {
            var result = new CreateGameInstanceResponse();
            var game = new CardGame();
            game.Id = Guid.NewGuid().ToString();
            game.Deck = CardFactory.CardDealersDeck();
            game.Events = new List<CardGameEvent>();
            game.Events.Add(new CardGameEvent
            {
                ClientId = "SERVER",
                GameId = game.Id,
                Message = "Game Created at " + DateTime.UtcNow.ToString("F"),
                Type = "SYSTEM"
            });

            Cache.Set(game.Id, game,TimeSpan.FromHours(1));
            var allGames = Cache.Get<AllGames>(AllGamesSummaries);
            if (allGames == null)
            {
                allGames = new AllGames { Games = new List<GameSummary>() };
            }
            if(allGames.Games == null) allGames.Games = new List<GameSummary>();

            allGames.Games.Add(new GameSummary { GameId = game.Id });
            Cache.Set(AllGamesSummaries, allGames);
            JoinGame(new PlayerJoinGame
            {
                GameId = game.Id,
                PlayerId = request.PlayerOneId,
                PlayerDisplayerName = request.PlayerDisplayName
            });

            result.GameId = game.Id;
            return result;
        }

        private PlayerJoinGameResponse JoinGame(PlayerJoinGame request)
        {
            var result = new PlayerJoinGameResponse();
            var game = Cache.Get<CardGame>(request.GameId);
            var allGames = Cache.Get<AllGames>(AllGamesSummaries);
            var gameSummary = allGames.Games.Single(x => x.GameId == request.GameId);
            if (game.PlayerOneId != null && game.PlayerTwoId != null)
                throw HttpError.Conflict("Game full");

            if (game.PlayerOneId == null)
            {
                game.PlayerOneId = request.PlayerId;
                game.PlayerOne = new GamePlayer
                {
                    DisplayName = request.PlayerDisplayerName,
                    Hand = game.Deck.Deal(2)
                };
                gameSummary.HasPlayerOne = true;
            }
            else
            {
                game.PlayerTwoId = request.PlayerId;
                game.PlayerTwo = new GamePlayer
                {
                    DisplayName = request.PlayerDisplayerName,
                    Hand = game.Deck.Deal(2)
                };
                gameSummary.HasPlayerTwo = true;
            }
            
            Cache.Set(AllGamesSummaries, allGames);

            game.Events.Add(new CardGameEvent
            {
                ClientId = request.PlayerId,
                GameId = request.GameId,
                Message = "Player {0} has entered the game.".Fmt(request.PlayerDisplayerName),
                Type = "JOIN"
            });

            Cache.Set(request.GameId, game,TimeSpan.FromHours(1));
            return result;
        }

        public GetGameStateResponse Get(GetGameState request)
        {
            var game = Cache.Get<CardGame>(request.GameId);
            if (game == null) throw HttpError.NotFound("Game not found");
            return new GetGameStateResponse
            {
                Game = game
            };
        }

        public HitMeResponse Put(HitMe request)
        {
            var game = Cache.Get<CardGame>(request.GameId);
            if (game == null) throw HttpError.NotFound("Game not found");
            if (game.PlayerOneId != request.PlayerId &&
                game.PlayerTwoId != request.PlayerId)
                throw HttpError.NotFound("Player not found");

            var player = game.PlayerOneId == request.PlayerId ? game.PlayerOne : game.PlayerTwo;
            player.HitMe(game.Deck);

            game.Events.Add(new CardGameEvent
            {
                ClientId = request.PlayerId,
                GameId = request.GameId,
                Message = "Player {0} hit".Fmt(player.DisplayName),
                Type = "HIT"
            });

            Cache.Set(request.GameId, game, TimeSpan.FromHours(1));
            var result = new HitMeResponse();
            return result;
        }

        public GetGameListResponse Get(GetGameList request)
        {
            var games = Cache.Get<AllGames>(AllGamesSummaries);
            return new GetGameListResponse
            {
                AllGames = games
            };
        }

        public CreatePlayerWaitingResponse Post(CreatePlayerWaiting request)
        {
            string playerId = Request.GetSessionId();

            var nextGame = Cache.Get<string>(NextGame);
            try
            {
                if (nextGame == null)
                {
                    var response = CreateGame(new CreateGameInstance
                    {
                        PlayerOneId = playerId
                    });
                    nextGame = response.GameId;
                    Cache.Set(NextGame, response.GameId);
                }
                else
                {
                    JoinGame(new PlayerJoinGame
                    {
                        GameId = nextGame,
                        PlayerId = playerId,
                        PlayerDisplayerName = request.PlayerDisplayName
                    });
                    Cache.Set<string>(NextGame, null);
                }
            }
            catch (Exception e)
            {
                Cache.Set<string>(NextGame, null);
                throw;
            }

            return new CreatePlayerWaitingResponse
            {
                GameId = nextGame
            };
        }
    }

    public class WaitingPlayers
    {
        public List<string> Waiting { get; set; } 
    }
}
