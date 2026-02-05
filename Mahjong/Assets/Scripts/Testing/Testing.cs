using System.Collections.Generic;
using UnityEngine;
using MJ.Core.Tiles;

namespace MJ.Testing
{
    public class Testing : MonoBehaviour
    {
        [SerializeField] private TileDefinitionSO tileDefinition;
        [SerializeField] private TileView tileView;
        
        private void Start() {
            Debug.Log("-= TESTING =-");
            Debug.Log("create tile view");

            TileInstance tileInstance = new TileInstance(1, tileDefinition);
            tileView.Bind(tileInstance);
        }
    }
}
