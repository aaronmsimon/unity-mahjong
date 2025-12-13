using System;

namespace MJ.GameLogic
{
    /// <summary>
    /// Represents a single state change in the game
    /// Used for logging, undo/redo, and replay
    /// </summary>
    [Serializable]
    public class GameStateChange
    {
        public DateTime Timestamp { get; private set; }
        public GameState PreviousState { get; private set; }
        public GameState NewState { get; private set; }
        public string ActionDescription { get; private set; }
        public string ActionType { get; private set; }

        public GameStateChange(GameState previousState, GameState newState, string actionType, string description)
        {
            Timestamp = DateTime.Now;
            PreviousState = previousState;
            NewState = newState;
            ActionType = actionType;
            ActionDescription = description;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {ActionType}: {ActionDescription}";
        }
    }
}
