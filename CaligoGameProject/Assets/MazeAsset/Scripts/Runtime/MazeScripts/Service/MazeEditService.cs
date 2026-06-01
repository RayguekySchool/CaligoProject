using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MazeAsset.MazeGenerator
{
    internal class MazeEditService
    {
        internal void StartEditMode(MazeGeneratorManager mazeGeneratorManager, string nameRoot)
        {
            var manager = mazeGeneratorManager;
            manager.editMode = true;

            if (manager.rootElevator != null)
                manager.rootElevator.SetActive(false);

            manager.rootEdit = MazeObjectHelper.RecreateRoot(nameRoot);
            manager.editObjectManager = new GameObjectManager();

            if (manager.GetComponent<MazeVisibleChunk>().VisibleAllMaze)
                manager.GenerateFullMaze(edit: true);
            else
                manager.GenerateMazeWithPosition(edit: true);
        }


        internal void StopEditMode(MazeGeneratorManager mazeGeneratorManager)
        {
            var manager = mazeGeneratorManager;
            manager.editMode = false;

#if UNITY_EDITOR
            manager.moveTool = null;
#endif
            if (manager.rootElevator != null)
                manager.rootElevator.SetActive(true);

            if (manager.rootEdit != null)
                Object.DestroyImmediate(manager.rootEdit);
            manager.editMode = false;
            manager.editObjectManager = null;
        }


        internal void HandleMouseClick(Vector2 mousePosition, MazeGeneratorManager mazeGeneratorManager, MoveGOFromChunkToOtherChunk moveTool)
        {
#if UNITY_EDITOR
            var manager = mazeGeneratorManager;
            var mazeColor = manager.GetComponent<MazeColor>();
            moveTool ??= new MoveGOFromChunkToOtherChunk(
                mazeColor.editMaterial,
                mazeColor.elevatorMaterial,
                mazeColor.mazeMaterial,
                manager.scaler,
                manager.shapeCell,
                manager.wallPrefab.transform.localScale.z
            );
            if (manager.root == null) return;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (!Physics.Raycast(ray, out RaycastHit hit)) return;
            GameObject hitObject = hit.collider.gameObject;
            Mesh mesh = manager.shapeCell == ShapeCellEnum.Hexagon
                ? new HexagonCreate().CreateMeshHexagon(manager.wallPrefab.transform.localScale.z, 1)
                : manager.wallPrefab.GetComponent<MeshFilter>().sharedMesh;

            (var newParent, var position, var name) = moveTool.ChangeWallVisibility(
                hitObject,
                manager.wallsObjectManager,
                manager.elevatorObjectManager,
                manager.editObjectManager,
                manager.mazeData.floors,
                manager.InterfaceService.GetElevatorPlatform(),
                mesh,
                manager.mazeData
            );

            if (newParent != null)
            {
                hitObject.transform.SetParent(newParent.transform);
            }
            if(position != Vector3Int.back)
                mazeGeneratorManager.UpdateMinimap(position, name);

#endif
        }
    }
}
