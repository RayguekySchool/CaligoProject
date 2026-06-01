using System;
using System.Collections.Generic;
using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    public class CellInteract : MonoBehaviour
    {
        internal bool visited = false;

        internal void OnPlayerInteract()
        {

            if (visited) return;

            visited = true;
        }
    }
}
