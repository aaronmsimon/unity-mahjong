using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace MJ.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Mahjong/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IDebugActions
    {
        // Debug
        public event UnityAction startNewGameEvent;
        public event UnityAction nextTurnEvent;
        public event UnityAction printHandsEvent;
        public event UnityAction printGameStateEvent;

        private GameInput gameInput;

        private void OnEnable()
        {
            if (gameInput == null)
            {
                gameInput = new GameInput();
                gameInput.Debug.SetCallbacks(this);
            }

            // EnableDebugInput();
        }

        private void OnDisable()
        {
            DisableAllInput();
        }

        // Debug Events
        public void OnStartNewGame(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                startNewGameEvent?.Invoke();
        }

        public void OnNextTurn(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                nextTurnEvent?.Invoke();
        }

        public void OnPrintHands(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printHandsEvent?.Invoke();
        }

        public void OnPrintGameState(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printGameStateEvent?.Invoke();
        }

        // Enable/Disable Action Maps

        public void EnableDebugInput()
        {
            gameInput.Debug.Enable();
        }

        public void DisableAllInput()
        {
            gameInput.Debug.Disable();
        }
    }
}
