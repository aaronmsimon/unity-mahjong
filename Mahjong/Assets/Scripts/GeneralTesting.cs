using System.Collections.Generic;
using UnityEngine;
using MJ.Core;

namespace MJ.Testing
{
    public class GeneralTesting : MonoBehaviour
    {
        private void Awake() {
            List<TileInstance> wall = WallFactory.BuildHKWall();

            foreach (TileInstance tile in wall) {
                Debug.Log($"{tile}");
            }
        }
    }
}
