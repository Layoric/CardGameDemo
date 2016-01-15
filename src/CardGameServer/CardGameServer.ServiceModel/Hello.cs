using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CardGameServer.ServiceModel.Types;
using ServiceStack;

namespace CardGameServer.ServiceModel
{
    [Route("/hello/{Name}")]
    public class Hello : IReturn<HelloResponse>
    {
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    public class CreateGameInstance
    {
        public string PlayerOneId { get; set; }
        public string PlayerDisplayName { get; set; }
    }

    public class CreateGameInstanceResponse
    {
        public string GameId { get; set; }
    }

    [Route("/blackjack/games/{GameId}/join", Verbs = "PUT")]
    public class PlayerJoinGame
    {
        public string PlayerId { get; set; }
        public string PlayerDisplayerName { get; set; }
        public string GameId { get; set; }
    }

    public class PlayerJoinGameResponse
    {
        
    }

    [Route("/blackjack/games/{GameId}", Verbs = "GET")]
    public class GetGameState
    {
        public string GameId { get; set; }
    }

    public class GetGameStateResponse
    {
        public CardGame Game { get; set; }
    }

    [Route("/blackjack/games/{GameId}/hit")]
    public class HitMe
    {
        public string GameId { get; set; }
        public string PlayerId { get; set; }
    }

    public class HitMeResponse
    {
        
    }

    [Route("/blackjack/games")]
    public class GetGameList
    {
        
    }

    public class GetGameListResponse
    {
        public AllGames AllGames { get; set; }
    }

    [Route("/ready", Verbs = "POST")]
    public class CreatePlayerWaiting
    {
        public string PlayerDisplayName { get; set; }
    }

    public class CreatePlayerWaitingResponse
    {
        public string PlayerId { get; set; }
        public string GameId { get; set; }
    }

}
