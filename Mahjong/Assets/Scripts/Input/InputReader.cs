using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace MJ.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Mahjong/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IDebugActions
    {
        // Debug
        public event UnityAction startNewRoundEvent;
        public event UnityAction printHandsEvent;
        public event UnityAction printWallCountEvent;
        public event UnityAction printDiscardsEvent;
        public event UnityAction printTurnInfoEvent;
        public event UnityAction printCurrentHandEvent;
        public event UnityAction<Vector2> discardEvent;

        private GameInput gameInput;
        private Vector2 position;

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
        public void OnStartNewRound(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                startNewRoundEvent?.Invoke();
        }

        public void OnPrintHands(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printHandsEvent?.Invoke();
        }

        public void OnPrintWallCount(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printWallCountEvent?.Invoke();
        }

        public void OnPrintDiscards(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printDiscardsEvent?.Invoke();
        }

        public void OnPrintTurnInfo(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printTurnInfoEvent?.Invoke();
        }

        public void OnPrintCurrentHand(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                printCurrentHandEvent?.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context) {
            position = context.ReadValue<Vector2>();
        }

        public void OnDiscard(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                discardEvent?.Invoke(position);
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
