using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    public interface ICell
    {
        internal UnityEngine.Vector2 GetSizeCell(UnityEngine.Vector2 scaler);
        internal float GetWidthChunk(int chunkSize, UnityEngine.Vector2 scaler);
        internal float GetHeightChunk(int chunkSize, UnityEngine.Vector2 scaler);
        internal ShapeCellEnum GetCellType { get; }
        internal List<DataOfMaze> GetWallsList(List<WallsStateEnum[,]> mazeArray, bool removeWalls);
        internal Vector2Int GetIndexInMap(Vector3 playerPos, Vector2 scaler);
    }
}
