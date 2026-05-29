
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal class MazeGenerationService : IMazeGenerationService
    {
        private IMazeGenerationStrategy _generationStrategy;
        private ICell _cellShape;
        internal MazeData MazeData { get; private set; }

        MazeData IMazeGenerationService.MazeData => MazeData;

        internal MazeGenerationService(IMazeGenerationStrategy generationStrategy, ICell cellShape)
        {
            _generationStrategy = generationStrategy;
            _cellShape = cellShape;
        }

        bool IMazeGenerationService.Initialize()
        {
            MazeData = _generationStrategy.InitializeData();
            var checker = new CheckerConector();
            checker.CheckIfMazeConnected(MazeData, _cellShape, true);
            return MazeData != null;
        }

        void IMazeGenerationService.GenerateData(bool removeWalls, bool generateFullMaze)
        {
            MazeData.floors = _cellShape.GetWallsList(MazeData.mazeArray, removeWalls);
        }

        void IMazeGenerationService.GenerateFullMaze(MazeData mazeData, MazeDimensionsService dimensionsService,
            MazeGeneratorChunk mazeGeneratorChunk, bool edit)
        {
            for (int y = 0; y < mazeData.floors.Count; y++)
            {
                (int maxChunkX, int maxChunkZ) = dimensionsService.GetMaxChunks(y, mazeData);
                for (int z = 0; z < maxChunkZ; z++)
                {
                    for (int x = 0; x < maxChunkX; x++)
                    {
                        mazeGeneratorChunk.GenerateChunk(x, y, z, edit);
                    }
                }
            }
        }

        (Vector3Int, int) IMazeGenerationService.GenerateMazeWithPosition(MazeVisibleChunk visibleChunk,  ICell cell, 
            int ChunkSize, Vector2 scaler, MazeData mazeData, MazeGeneratorChunk mazeGeneratorChunk,
            MazeDimensionsService dimensionsService, bool edit)
        {
            Vector3Int centerChunk = Vector3Int.one;
            if (visibleChunk == null || visibleChunk.transformPlayer == null) return (Vector3Int.back, -1);

            float widthChunk = cell.GetWidthChunk(ChunkSize, scaler);
            float heightChunk = cell.GetHeightChunk(ChunkSize, scaler);
            Vector3 positionPlayer = visibleChunk.transformPlayer.transform.position;

            int visible = visibleChunk.VisibleTerrain;
            int visibleFloor = visibleChunk.VisibleFloor;

            int startX = (int)(positionPlayer.x / widthChunk) - visible;
            int startZ = (int)(positionPlayer.z / heightChunk) - visible;
            int startY = (int)(positionPlayer.y / scaler.y) - visibleFloor;

            for (int numberFloor = startY; numberFloor <= startY + (2 * visibleFloor); numberFloor++)
            {
                if (numberFloor >= mazeData.floors.Count || numberFloor < 0) continue;

                (int maxChunkX, int maxChunkZ) = dimensionsService.GetMaxChunks(numberFloor, mazeData);
                for (int x = startX; x <= startX + (2 * visible); x++)
                {
                    for (int z = startZ; z <= startZ + (2 * visible); z++)
                    {
                        if (x >= maxChunkX || z >= maxChunkZ || x < 0 || z < 0) continue;

                        mazeGeneratorChunk.GenerateChunk(x, numberFloor, z, edit);
                    }
                }
            }
            int centerYMinimap = (int)((positionPlayer.y + scaler.y / 2) / scaler.y);
            centerChunk.x = (int)(positionPlayer.x / widthChunk);
            centerChunk.z = (int)(positionPlayer.z / heightChunk);
            centerChunk.y = (int)(positionPlayer.y / scaler.y);
            return (centerChunk, centerYMinimap);
        }
    }
}
