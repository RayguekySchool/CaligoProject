using UnityEngine;


namespace MazeAsset.MazeGenerator
{
    internal class MazeChunkVisibilityService
    {
        private readonly MazeGeneratorManager manager;

        internal MazeChunkVisibilityService(MazeGeneratorManager manager)
        {
            this.manager = manager;
        }

        internal void UpdateChunksWithPlayer(Vector3 positionPlayer)
        {
            if (manager.GetDimensionsService == null) manager.ReInitInterface();

            float widthChunk = manager.InterfaceService.GetCell().GetWidthChunk(manager.ChunkSize, manager.scaler);
            float heightChunk = manager.InterfaceService.GetCell().GetHeightChunk(manager.ChunkSize, manager.scaler);
            float chunkHeightY = manager.scaler.y;
            MazeVisibleChunk mazeVisibleChunk = manager.GetComponent<MazeVisibleChunk>();

            if (!Application.isPlaying)
            {
                UpdateMinimapFloor(positionPlayer);
            }

            if (mazeVisibleChunk.VisibleAllMaze) return;

            int visible = mazeVisibleChunk.VisibleTerrain;
            int visibleFloor = mazeVisibleChunk.VisibleFloor;

            HandleYChunks(positionPlayer, visible, visibleFloor, widthChunk, heightChunk, chunkHeightY, mazeVisibleChunk);
            HandleXChunks(positionPlayer, visible, visibleFloor, widthChunk, heightChunk, chunkHeightY, mazeVisibleChunk);
            HandleZChunks(positionPlayer, visible, visibleFloor, widthChunk, heightChunk, chunkHeightY, mazeVisibleChunk);
        }

        private void UpdateMinimapFloor(Vector3 playerPos)
        {
            int newFloorMinimap = (int)((playerPos.y + manager.scaler.y / 2) / manager.scaler.y);
            if (manager.centerYMinimap != newFloorMinimap)
            {
                manager.InterfaceService.GetMinimap().ChangeFloorMinimap(newFloorMinimap, manager.mazeData,
                    manager.InterfaceService.GetCell().GetIndexInMap(playerPos, manager.scaler));
                manager.centerYMinimap = newFloorMinimap;
            }
        }

        private void HandleYChunks(Vector3 playerPos, int visible, int visibleFloor, float widthChunk, float heightChunk, float chunkHeightY, MazeVisibleChunk mazeVisibleChunk)
        {
            int newY = (int)(playerPos.y / chunkHeightY);
            if (manager.centerChunk.y == newY) return;

            int yDestruct = newY < manager.centerChunk.y
                ? manager.centerChunk.y + visibleFloor
                : manager.centerChunk.y - visibleFloor;
            int yCreate = newY < manager.centerChunk.y
                ? manager.centerChunk.y - visibleFloor - 1
                : manager.centerChunk.y + visibleFloor + 1;

            int startX = (int)(mazeVisibleChunk.transformPlayer.position.x / widthChunk) - visible;
            int startZ = (int)(mazeVisibleChunk.transformPlayer.position.z / heightChunk) - visible;
            manager.centerChunk.y = newY;

            ReplaceChunksInY(yDestruct, startX, startZ, visible);
            CreateChunksInY(yCreate, startX, startZ, visible);
        }

        private void ReplaceChunksInY(int yDestruct, int startX, int startZ, int visible)
        {
            if (yDestruct < manager.mazeData.floors.Count && yDestruct >= 0)
            {
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(yDestruct, manager.mazeData);
                for (int x = startX; x <= startX + 2 * visible; x++)
                {
                    for (int z = startZ; z <= startZ + 2 * visible; z++)
                    {
                        if (x < 0 || z < 0 || x >= maxX || z >= maxZ) continue;
                        manager.DestroyChunkAtPosition(new Vector3(x, yDestruct, z));
                    }
                }
            }
        }

        private void CreateChunksInY(int yCreate, int startX, int startZ, int visible)
        {
            if (yCreate < manager.mazeData.floors.Count && yCreate >= 0)
            {
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(yCreate, manager.mazeData);
                for (int x = startX; x <= startX + 2 * visible; x++)
                {
                    for (int z = startZ; z <= startZ + 2 * visible; z++)
                    {
                        if (x < 0 || z < 0 || x >= maxX || z >= maxZ) continue;
                        manager.CreateChunkAtPosition(new Vector3(x, yCreate, z));
                    }
                }
            }
        }

        private void HandleXChunks(Vector3 playerPos, int visible, int visibleFloor, float widthChunk, float heightChunk, float chunkHeightY, MazeVisibleChunk mazeVisibleChunk)
        {
            int newX = (int)(playerPos.x / widthChunk);
            if (manager.centerChunk.x == newX) return;

            int xDestruct = newX < manager.centerChunk.x
                ? manager.centerChunk.x + visible
                : manager.centerChunk.x - visible;
            int xCreate = newX < manager.centerChunk.x
                ? manager.centerChunk.x - visible - 1
                : manager.centerChunk.x + visible + 1;

            manager.centerChunk.x = newX;
            int startY = (int)(mazeVisibleChunk.transformPlayer.position.y / chunkHeightY) - visibleFloor;
            int _z = (int)(playerPos.z / heightChunk);

            ReplaceChunksInX(xDestruct, startY, _z, visibleFloor);
            CreateChunksInX(xCreate, startY, _z, visibleFloor);
        }

        private void ReplaceChunksInX(int xDestruct, int startY, int _z, int visibleFloor)
        {
            for (int y = startY; y <= startY + 2 * visibleFloor; y++)
            {
                if (y < 0 || y >= manager.mazeData.floors.Count) continue;
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(y, manager.mazeData);
                for (int x = -visibleFloor; x <= visibleFloor; x++)
                {
                    int z = _z + x;
                    if (z < 0 || z >= maxZ || xDestruct < 0 || xDestruct >= maxX) continue;
                    manager.DestroyChunkAtPosition(new Vector3(xDestruct, y, z));
                }
            }
        }

        private void CreateChunksInX(int xCreate, int startY, int _z, int visibleFloor)
        {
            for (int y = startY; y <= startY + 2 * visibleFloor; y++)
            {
                if (y < 0 || y >= manager.mazeData.floors.Count) continue;
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(y, manager.mazeData);
                for (int x = -visibleFloor; x <= visibleFloor; x++)
                {
                    int z = _z + x;
                    if (z < 0 || z >= maxZ || xCreate < 0 || xCreate >= maxX) continue;
                    manager.CreateChunkAtPosition(new Vector3(xCreate, y, z));
                }
            }
        }

        private void HandleZChunks(Vector3 playerPos, int visible, int visibleFloor, float widthChunk, float heightChunk, float chunkHeightY, MazeVisibleChunk mazeVisibleChunk)
        {
            int newZ = (int)(playerPos.z / heightChunk);
            if (manager.centerChunk.z == newZ) return;

            int zDestruct = newZ < manager.centerChunk.z
                ? manager.centerChunk.z + visible
                : manager.centerChunk.z - visible;
            int zCreate = newZ < manager.centerChunk.z
                ? manager.centerChunk.z - visible - 1
                : manager.centerChunk.z + visible + 1;

            manager.centerChunk.z = newZ;
            int startY = (int)(mazeVisibleChunk.transformPlayer.position.y / chunkHeightY) - visibleFloor;
            int _x = (int)(playerPos.x / widthChunk);

            ReplaceChunksInZ(zDestruct, startY, _x, visibleFloor);
            CreateChunksInZ(zCreate, startY, _x, visibleFloor);
        }

        private void ReplaceChunksInZ(int zDestruct, int startY, int _x, int visibleFloor)
        {
            for (int y = startY; y <= startY + 2 * visibleFloor; y++)
            {
                if (y < 0 || y >= manager.mazeData.floors.Count) continue;
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(y, manager.mazeData);
                for (int x = -visibleFloor; x <= visibleFloor; x++)
                {
                    int finalX = _x + x;
                    if (finalX < 0 || finalX >= maxX || zDestruct < 0 || zDestruct >= maxZ) continue;
                    manager.DestroyChunkAtPosition(new Vector3(finalX, y, zDestruct));
                }
            }
        }

        private void CreateChunksInZ(int zCreate, int startY, int _x, int visibleFloor)
        {
            for (int y = startY; y <= startY + 2 * visibleFloor; y++)
            {
                if (y < 0 || y >= manager.mazeData.floors.Count) continue;
                (int maxX, int maxZ) = manager.GetDimensionsService.GetMaxChunks(y, manager.mazeData);
                for (int x = -visibleFloor; x <= visibleFloor; x++)
                {
                    int finalX = _x + x;
                    if (finalX < 0 || finalX >= maxX || zCreate < 0 || zCreate >= maxZ) continue;
                    manager.CreateChunkAtPosition(new Vector3(finalX, y, zCreate));
                }
            }
        }
    }
}