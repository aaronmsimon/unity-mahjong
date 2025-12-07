using System.Collections.Generic;
using MJ.Core;

namespace MJ.Game
{
    public enum TurnPhase
    {
        Draw,
        Discard
    }

    public sealed class GameState
    {
        public IReadOnlyList<PlayerState> Players => _players;
        private readonly List<PlayerState> _players;

        /// <summary>
        /// The wall of tiles. Top of the wall is at index WallIndex.
        /// </summary>
        public List<TileInstance> Wall { get; }

        /// <summary>
        /// Index of the next tile to draw from the wall.
        /// </summary>
        public int WallIndex { get; set; }

        /// <summary>
        /// Discards per player. Index matches SeatIndex.
        /// </summary>
        public IReadOnlyList<List<TileInstance>> DiscardsByPlayer => _discardsByPlayer;
        private readonly List<List<TileInstance>> _discardsByPlayer;

        /// <summary>
        /// The seat index of the dealer (0–3).
        /// </summary>
        public int DealerSeat { get; set; }

        /// <summary>
        /// Whose turn it is currently (0–3).
        /// </summary>
        public int CurrentSeat { get; set; }

        /// <summary>
        /// Current turn phase (Draw / Discard).
        /// </summary>
        public TurnPhase TurnPhase { get; set; }

        public GameState(List<PlayerState> players, List<TileInstance> wall)
        {
            _players = players;
            Wall = wall;
            WallIndex = 0;

            _discardsByPlayer = new List<List<TileInstance>>(players.Count);
            for (int i = 0; i < players.Count; i++)
            {
                _discardsByPlayer.Add(new List<TileInstance>());
            }
        }

        public PlayerState GetPlayer(int seatIndex) => _players[seatIndex];

        public List<TileInstance> GetDiscardsFor(int seatIndex) => _discardsByPlayer[seatIndex];
    }
}
