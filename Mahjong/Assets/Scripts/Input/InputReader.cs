using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace MJ.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Mahjong/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IDebugActions, GameInput.IGameplayActions
    {
        // Debug
        public event UnityAction startNewGameEvent;
        public event UnityAction nextTurnEvent;
        public event UnityAction printHandsEvent;
        public event UnityAction printGameStateEvent;

        // Gameplay
        public event UnityAction clickEvent;

        private GameInput gameInput;

        private void OnEnable()
        {
            if (gameInput == null)
            {
                gameInput = new GameInput();
                gameInput.Debug.SetCallbacks(this);
                gameInput.Gameplay.SetCallbacks(this);
            }

            EnableGameplayInput();
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

        // Gameplay Events
        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed) {
                clickEvent?.Invoke();
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Debug.Log($"Mouse Screen Position: {mousePos}");
            }
        }

        // Enable/Disable Action Maps

        public void EnableDebugInput()
        {
            gameInput.Debug.Enable();
        }

        public void EnableGameplayInput()
        {
            gameInput.Gameplay.Enable();
        }

        public void DisableAllInput()
        {
            gameInput.Debug.Disable();
            gameInput.Gameplay.Disable();
        }
    }
}
