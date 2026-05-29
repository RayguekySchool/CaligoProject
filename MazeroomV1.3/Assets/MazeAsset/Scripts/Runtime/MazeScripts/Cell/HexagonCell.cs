using System;
using System.Collections.Generic;
using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    public class HexagonCell : ICell
    {
        ShapeCellEnum ICell.GetCellType => ShapeCellEnum.Hexagon;

        float ICell.GetHeightChunk(int chunkSize, UnityEngine.Vector2 scaler)
        {
            return (float)(chunkSize * MathF.Sqrt(3) * scaler.x);
        }

        float ICell.GetWidthChunk(int chunkSize, UnityEngine.Vector2 scaler)
        {
            return (float)(chunkSize * 1.5f * scaler.x);
        }

        Vector2 ICell.GetSizeCell(UnityEngine.Vector2 scaler)
        {
            return new Vector2(1.5f * scaler.x, MathF.Sqrt(3) * scaler.x);
        }

        List<DataOfMaze> ICell.GetWallsList(List<WallsStateEnum[,]> mazeArray, bool removeWalls)
        {
            var hexScript = new AlgorithmusForHexagon(mazeArray);
            return hexScript.GetWallsList(removeWalls);
        }

        Vector2Int ICell.GetIndexInMap(Vector3 playerPos, Vector2 scaler)
        {
            var sizeOfCell = (this as ICell).GetSizeCell (scaler);
            return new Vector2Int((int)(playerPos.x / sizeOfCell.x), (int)(playerPos.z / sizeOfCell.y));
        }
    }
}
