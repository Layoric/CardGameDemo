using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGameServer.ServiceModel.Types
{
    public class CardGame
    {
        public string Id { get; set; }
        public string PlayerOneId { get; set; }
        public string PlayerTwoId { get; set; }

        public string TurnPlayerId { get; set; }

        public GamePlayer PlayerOne { get; set; } 
        public GamePlayer PlayerTwo { get; set; }

        public List<PlayingCard> Deck { get; set; }

        public List<CardGameEvent> Events { get; set; }

        public bool Finished { get; set; }
    }

    public class GamePlayer
    {
        public string DisplayName { get; set; }
        public List<PlayingCard> Hand { get; set; }
    }

    public class PlayingCard
    {
        public string Suit { get; set; }
        public string Value { get; set; }
    }

    public class CardGameEvent
    {
        public string ClientId { get; set; }
        public string GameId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }

    public enum CardSuit
    {
        Clubs,
        Hearts,
        Spades,
        Diamonds
    }

    public enum CardValue
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public class GameSummary
    {
        public string GameId { get; set; }
        public bool HasPlayerOne { get; set; }
        public bool HasPlayerTwo { get; set; }
        public bool Finished { get; set; }
    }

    public class AllGames
    {
        public List<GameSummary> Games { get; set; }
    }

    public static class CardFactory
    {
        public static PlayingCard Create(CardValue val, CardSuit suit)
        {
            return new PlayingCard { Suit = suit.ToString(), Value = val.ToString() };
        }

        public static bool IsValid(this PlayingCard playingCard)
        {
            var values = Enum.GetNames(typeof(CardValue));
            var suits = Enum.GetNames(typeof(CardSuit));
            return values.Any(val => val == playingCard.Value) && suits.Any(suit => suit == playingCard.Suit);
        }

        public static List<PlayingCard> CardDealersDeck()
        {
            var deck = new List<PlayingCard>();
            var values = Enum.GetNames(typeof(CardValue));
            var suits = Enum.GetNames(typeof(CardSuit));
            for (int i = 0; i < 5; i++)
            {
                foreach (var value in values)
                {
                    deck.AddRange(suits.Select(suit => new PlayingCard { Suit = suit, Value = value }));
                }
            }
            
            return deck;
        }

        public static List<PlayingCard> Deal(this List<PlayingCard> dealersDeck, int numberOfCards)
        {
            List<int> removed = new List<int>();
            List<PlayingCard> result = new List<PlayingCard>(numberOfCards);
            var rand = new Random(Guid.NewGuid().GetHashCode());
            lock (dealersDeck)
            {
                int count = 0;
                while (true)
                {
                    var possibleIndex = rand.Next(0, dealersDeck.Count - 1);
                    if (!removed.Contains(possibleIndex))
                    {
                        removed.Add(possibleIndex);
                        result.Add(dealersDeck.ElementAt(possibleIndex));
                        dealersDeck.RemoveAt(possibleIndex);
                        count++;
                    }
                    if (count == numberOfCards) break;
                }
            }
            return result;
        }

        public static void HitMe(this GamePlayer player, List<PlayingCard> deck)
        {
            player.Hand.AddRange(deck.Deal(1));
        }
    }
}
