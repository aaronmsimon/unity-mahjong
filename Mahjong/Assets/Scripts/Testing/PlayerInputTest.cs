using UnityEngine;
using MJ.Input;
using MJ.GameFlow;

namespace MJ.Testing
{
    public class PlayerInputTest : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private GameFlowController gameFlowController;

        private void OnEnable() {
            inputReader.startNewGameEvent += gameFlowController.StartGame;
            inputReader.nextTurnEvent += gameFlowController.NextTurn;
            inputReader.printHandsEvent += gameFlowController.DebugPrintAllHands;
            inputReader.printGameStateEvent += gameFlowController.DebugPrintGameState;
        }

        private void Start() {
            inputReader.EnableDebugInput();
        }

        private void OnDisable() {
            inputReader.startNewGameEvent -= gameFlowController.StartGame;
            inputReader.nextTurnEvent -= gameFlowController.NextTurn;
            inputReader.printHandsEvent -= gameFlowController.DebugPrintAllHands;
            inputReader.printGameStateEvent -= gameFlowController.DebugPrintGameState;
        }
    }
}
