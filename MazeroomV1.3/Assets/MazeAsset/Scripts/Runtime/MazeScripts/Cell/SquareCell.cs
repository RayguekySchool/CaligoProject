using System.Collections.Generic;
using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    public class SquareCell : ICell
    {
        ShapeCellEnum ICell.GetCellType => ShapeCellEnum.Square;

        float ICell.GetHeightChunk(int chunkSize, UnityEngine.Vector2 scaler)
        {
            return (float)(chunkSize * scaler.x);
        }

        Vector2Int ICell.GetIndexInMap(Vector3 playerPos, Vector2 scaler)
        {
            var sizeOfCell = (this as ICell).GetSizeCell(scaler);
            return new Vector2Int((int)(playerPos.x / sizeOfCell.x), (int)(playerPos.z / sizeOfCell.y));
        }

        Vector2 ICell.GetSizeCell(UnityEngine.Vector2 scaler)
        {
            return new Vector2(scaler.x, scaler.x);
        }

        List<DataOfMaze> ICell.GetWallsList(List<WallsStateEnum[,]> mazeArray, bool removeWalls)
        {
            var squareScript = new AlgorithmusForSquare(mazeArray);
            return squareScript.GetWallsList(removeWalls);
        }

        float ICell.GetWidthChunk(int chunkSize, UnityEngine.Vector2 scaler)
        {
            return (float)(chunkSize * scaler.x);
        }
    }
}