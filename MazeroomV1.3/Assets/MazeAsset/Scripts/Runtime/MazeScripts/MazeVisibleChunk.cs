using MazeAsset.CustomAttribute;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    [ExecuteInEditMode]
    internal class MazeVisibleChunk : MonoBehaviour
    {
        [SerializeField]
        internal bool VisibleAllMaze;
        [SerializeField, Required]
        internal Transform transformPlayer;
        [SerializeField, Required]
        internal Camera playerCamera;
        internal Vector3 InitialPosition { get; set; }
        [SerializeField, VisibleIf("VisibleAllMaze", false), Range(1, 10)]
        internal int VisibleTerrain = 1;
        [SerializeField, VisibleIf("VisibleAllMaze", false), Range(2, 10)]
        internal int VisibleFloor = 2;



        private void Update()
        {
            if (!TryGetComponent<MazeGeneratorManager>(out var maze)) return;
            if (playerCamera == null)
            {
                return;
            }
            Vector3 rotationInDegrees = playerCamera.transform.eulerAngles;

            maze.HandleRotation(rotationInDegrees);
            if (transformPlayer == null) return;
            
            Vector3 currentPosition = transformPlayer.position;
            if (currentPosition != InitialPosition)
            {
                InitialPosition = currentPosition;

                maze.UpdateChunksWithPlayer(currentPosition);
                RaycastHit hit;
                if (!Application.isPlaying)
                    return;
                if (Physics.Raycast(currentPosition, Vector3.down, out hit, 1))
                {

                    var cellInteract = hit.collider.GetComponent<CellInteract>();
                    if (cellInteract == null)
                        return;
                    maze.HandleCellEnter(cellInteract.gameObject, cellInteract.visited);
                    cellInteract.OnPlayerInteract();
                }
            }
        }
    }
}
