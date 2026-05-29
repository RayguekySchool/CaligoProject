using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal interface IMinimap
    {
        void InitData(MazeData mazeData, Minimap settings, int floorToMinimapbool, Vector2Int playerPos);
        void ChangeFloorMinimap(int floor, MazeData mazeData, Vector2Int playerPos);
        void HandleStepToUnvisitedCell(GameObject cellObject, MazeData mazeData, bool visited, Vector2Int playerPos);
        void HandleRotation(Vector3 rotationInDegrees);
        void UpdateMinimap(Vector3Int position, string name, MazeData mazeData);
    }
}
