using System.Collections.Generic;
using MJ.Core;
using MJ.Game;

public static class RoundInitializer
{
    /// <summary>
    /// Creates a fresh GameState for a new round:
    /// - Builds and shuffles a full wall
    /// - Creates 4 players (seats 0..3)
    /// - Deals 13 tiles to each, plus 1 extra tile to the dealer (14)
    /// - Sets current turn to the dealer in Discard phase
    ///
    /// Optional seed for deterministic shuffles (useful for testing).
    /// </summary>
    public static GameState StartNewRound(int dealerSeat = 0, int? shuffleSeed = null)
    {
        // 1. Build wall
        List<TileInstance> wall = WallFactory.BuildHKWall(shuffleSeed);

        // 2. Create players
        var players = new List<PlayerState>(4);
        for (int seat = 0; seat < 4; seat++)
        {
            players.Add(new PlayerState(seat));
        }

        // 3. Create GameState
        var state = new GameState(players, wall)
        {
            DealerSeat = dealerSeat,
            CurrentSeat = dealerSeat
        };

        // 4. Deal tiles
        DealInitialHands(state, dealerSeat);

        // 5. Sort hands for readability
        foreach (var player in players)
        {
            player.SortHand();
        }

        // Dealer starts with 14 tiles and must discard, so:
        state.TurnPhase = TurnPhase.Discard;

        return state;
    }

    private static void DealInitialHands(GameState state, int dealerSeat)
    {
        // Classic dealing pattern in code: each player ends up with 13,
        // dealer gets an extra (14th) tile so they can discard first.

        // We'll just do it straightforwardly: 13 tiles each, then 1 extra to dealer.

        // 13 tiles each:
        const int startingTilesNonDealer = 13;
        int numPlayers = state.Players.Count;

        for (int round = 0; round < startingTilesNonDealer; round++)
        {
            for (int seat = 0; seat < numPlayers; seat++)
            {
                DrawOne(state, seat);
            }
        }

        // Extra tile for dealer:
        DrawOne(state, dealerSeat);
    }

    private static void DrawOne(GameState state, int seat)
    {
        var player = state.GetPlayer(seat);
        var tile = state.Wall[state.WallIndex++];
        player.AddToHand(tile);
    }
}
