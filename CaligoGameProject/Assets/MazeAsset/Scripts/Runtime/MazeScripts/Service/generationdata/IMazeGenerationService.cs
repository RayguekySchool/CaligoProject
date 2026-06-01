using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal interface IMazeGenerationService
    {
        internal MazeData MazeData { get; }
        internal bool Initialize();
        internal void GenerateData(bool removeWalls, bool generateFullMaze);
        internal void GenerateFullMaze(MazeData mazeData, MazeDimensionsService dimensionsService, MazeGeneratorChunk mazeGeneratorChunk , bool edit = false);
        internal (Vector3Int, int) GenerateMazeWithPosition(MazeVisibleChunk visibleChunk, ICell cell,
            int ChunkSize, Vector2 scaler, MazeData mazeData, MazeGeneratorChunk mazeGeneratorChunk,
            MazeDimensionsService dimensionsService, bool edit = false);
    }
}
