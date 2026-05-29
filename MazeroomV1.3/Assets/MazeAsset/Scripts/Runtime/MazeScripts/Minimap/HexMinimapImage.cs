using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal class HexMinimapImage : MinimapBase
    {
        public HexMinimapImage(MazeGeneratorManager coroutineHost, ICell cell, MazeDimensionsService dimensionsService)
            : base(coroutineHost, cell, dimensionsService)
        {
        }

        protected override void GenerateSprites()
        {
            floorSprite = GenerateHexagonSprite(100, Color.white);
            wallSprite = GenerateWallSprite(100, Color.black);
        }

        private Sprite GenerateHexagonSprite(int size, Color color)
        {
            return new SpriteGenerator().GenerateHexagonSprite(size, color);
        }

        private Sprite GenerateWallSprite(int size, Color color)
        {
            return new SpriteGenerator().GenerateWallSprite(size, color);
        }

        protected override IEnumerator RenderFloorInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int player)
        {
            var size = cell.GetSizeCell(Vector2.one * sizeOfCell);
            int counter = 0;

            List<Vector2Int> positions = new List<Vector2Int>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    positions.Add(new Vector2Int(x, y));
                }
            }

            positions.Sort((a, b) =>
            {
                float distA = (a - player).sqrMagnitude;
                float distB = (b - player).sqrMagnitude;
                return distA.CompareTo(distB);
            });

            foreach (var pos in positions)
            {
                int x = pos.x;
                int y = pos.y;

                var elevatorDir = mazeData.ElevatorMove(x, floorIndex, y);
                var idx = mazeData.getIndex(x, y, w);

                if (mazeData.GetFloor(floorIndex).floorsWalls[idx] != WallVisibilityStatus.VisibleInNormalMode &&
                    elevatorDir == DirectionMoveEnum.None)
                    continue;

                string name = $"{x},{floorIndex},{y},_mf";
                Vector2 position = CalculateCellPosition(x, y, size);
                var img = CreateImage(position, floorSprite, name, parent);
                img.color = visibleAll ? colorForVisited : colorForNotVisited;

                if (visibleAll)
                    AddElevator(parent, elevatorDir, name, position);

                counter++;
                if (counter >= chunkSize)
                {
                    coroutineChunksPassed.Add($"{RootFloorName}_{floorIndex}");
                    counter = 0;
                    yield return null;
                }
            }
        }




        protected override IEnumerator RenderVerticalWallsInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int playerPosition)
        {
            var playerPos = new Vector2Int(playerPosition.x, playerPosition.y * 2);
            chunkSize += (chunkSize / 2);
            var size = cell.GetSizeCell(Vector2.one * sizeOfCell) / 2;
            int counter = 0;

            var wallData = mazeData.GetFloor(floorIndex).listWallsVertical;
            int mapWidth = mazeData.GetFloor(floorIndex).sizeVertical.x;

            List<(int x, int y, bool isSecondAdd)> wallPositions = new();

            for (int x = 0; x <= w; x++)
            {
                bool isSecondAdd = x % 2 == 1;

                for (int y = 0; y < h * 2 + 1; y++)
                {
                    wallPositions.Add((x, y, isSecondAdd));
                    isSecondAdd = !isSecondAdd;
                }
            }

            wallPositions.Sort((a, b) =>
            {
                float da = (new Vector2(a.x, a.y) - (Vector2)playerPos).sqrMagnitude;
                float db = (new Vector2(b.x, b.y) - (Vector2)playerPos).sqrMagnitude;
                return da.CompareTo(db);
            });
            foreach (var (x, y, isSecondAdd) in wallPositions)
            {
                int index = mazeData.getIndex(x, y, mapWidth);
                var wallType = wallData[index];
                bool add = visibleAll
                    ? wallType == WallVisibilityStatus.VisibleInNormalMode
                    : wallType == WallVisibilityStatus.VisibleInNormalMode || wallType == WallVisibilityStatus.VisibleInEditMode;

                if (add)
                {
                    var pos = GetVerticalWallPosition(x, y, size);
                    int angle = isSecondAdd ? -30 : 30;
                    var rect = InstantiateWall(pos, Quaternion.Euler(0, 0, angle), $"{x},{floorIndex},{y},_mv", parent);
                    rect.localScale += Vector3.down * 0.5f;

                    counter++;
                    if (counter >= chunkSize)
                    {
                        coroutineChunksPassed.Add($"{RootVerWallName}_{floorIndex}");
                        counter = 0;
                        yield return null;
                    }
                }
            }
        }
        protected override IEnumerator RenderHorizontalWallsInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int playerPos)
        {
            var size = cell.GetSizeCell(Vector2.one * sizeOfCell);
            int counter = 0;

            var wallData = mazeData.GetFloor(floorIndex).listWallsHorizontal;
            int mapWidth = mazeData.GetFloor(floorIndex).sizeHorizontal.x;

            List<(int x, int y)> wallPositions = new();

            for (int y = 0; y <= h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    wallPositions.Add((x, y));
                }
            }

            wallPositions.Sort((a, b) =>
            {
                float da = (new Vector2(a.x, a.y) - (Vector2)playerPos).sqrMagnitude;
                float db = (new Vector2(b.x, b.y) - (Vector2)playerPos).sqrMagnitude;
                return da.CompareTo(db);
            });

            foreach (var (x, y) in wallPositions)
            {
                int index = mazeData.getIndex(x, y, mapWidth);
                var wallType = wallData[index];
                bool add = visibleAll
                    ? wallType == WallVisibilityStatus.VisibleInNormalMode
                    : wallType == WallVisibilityStatus.VisibleInNormalMode || wallType == WallVisibilityStatus.VisibleInEditMode;

                if (add)
                {
                    var pos = GetHorizontalWallPosition(x, y, size);
                    var rect = InstantiateWall(pos, Quaternion.Euler(0, 0, 90), $"{x},{floorIndex},{y},_mh", parent);
                    rect.localScale += Vector3.down * 0.5f;

                    counter++;
                    if (counter >= chunkSize)
                    {
                        coroutineChunksPassed.Add($"{RootHorWallName}_{floorIndex}");
                        counter = 0;
                        yield return null;
                    }
                }
            }
        }

        private Vector2 CalculateCellPosition(int x, int y, Vector2 size)
        {
            float xPos = x * size.x / 2;
            float yPos = y * size.y / 2;
            if (x % 2 == 1)
                yPos += size.y / 4;
            return new Vector2(xPos, yPos);
        }

        private Vector3 GetVerticalWallPosition(int x, int y, Vector2 size)
        {
            float xPos = x * size.x - 0.5f * size.x;
            float yPos = y * size.y / 2 - 0.25f * size.y;
            return new Vector3(xPos, yPos);
        }

        private Vector3 GetHorizontalWallPosition(int x, int y, Vector2 size)
        {
            float xPos = x * size.x / 2;
            float yPos = y * size.y / 2 - 0.25f * size.y;
            if (x % 2 == 1)
                yPos += size.y / 4;
            return new Vector3(xPos, yPos);
        }

        protected override void DestroyVerticalWall(Vector3Int position, Transform root, MazeData mazeData)
        {
            int[] dirX = { 0, 0, 1, 1 };
            int[] dirY = position.x % 2 == 0 ? new int[] { 0, 1, 1, 0 } : new int[] { 1, 2, 2, 1 };
            (int x, int y, int z) = (position.x, position.y, position.z);
            for (int indexFor = 0; indexFor < dirX.Count(); indexFor++)
            {
                var index = mazeData.getIndex(x + dirX[indexFor], z * 2 + dirY[indexFor], mazeData.GetFloor(y).sizeVertical.x);
                if (mazeData.GetFloor(y).listWallsVertical[index] == WallVisibilityStatus.VisibleInEditMode)
                {
                    TryDestroy(root.transform, $"{x + dirX[indexFor]},{y},{z * 2 + dirY[indexFor]},_mv");
                }
            }
        }

        protected override void DestroyHorizontalWall(Vector3Int position, Transform root, MazeData mazeData)
        {
            int[] dirX = { 0, 0 };
            int[] dirY = { 0, 1 };
            (int x, int y, int z) = (position.x, position.y, position.z);
            for (int indexFor = 0; indexFor < dirX.Count(); indexFor++)
            {
                var index = mazeData.getIndex(x + dirX[indexFor], z + dirY[indexFor], mazeData.GetFloor(y).sizeHorizontal.x);
                if (mazeData.GetFloor(y).listWallsHorizontal[index] == WallVisibilityStatus.VisibleInEditMode)
                {
                    TryDestroy(root.transform, $"{x + dirX[indexFor]},{y},{z + dirY[indexFor]},_mh");
                }
            }
        }

        protected override void UpdatePositionMap(Vector3Int positionIndex, Transform curentFloorRoot)
        {
            var size = cell.GetSizeCell(Vector2.one * sizeOfCell);
            Vector2 position = CalculateCellPosition(positionIndex.x, positionIndex.z, size);
            curentFloorRoot.GetComponent<RectTransform>().anchoredPosition = -position;
        }

        protected override void UpdateVertical(Vector3Int position, MazeData mazeData)
        {
            if (!IsFloorIndexValid(position.y, imagesRootMinimap)) return;
            var rootCurrent = imagesRootMinimap[position.y];
            if (rootCurrent == null) return;

            var verRoot = rootCurrent.transform.Find(RootVerWallName);
            if (verRoot == null) return;


            var size = cell.GetSizeCell(Vector2.one * sizeOfCell) / 2;

            var wallData = mazeData.GetFloor(position.y).listWallsVertical;
            int mapWidth = mazeData.GetFloor(position.y).sizeVertical.x;


            int index = mazeData.getIndex(position.x, position.z, mapWidth);
            var wallType = wallData[index];
            bool add = wallType == WallVisibilityStatus.VisibleInNormalMode;

            if (add)
            {
                var pos = GetVerticalWallPosition(position.x, position.z, size);
                int angle = (position.x + position.z) % 2 == 1 ? -30 : 30;
                var rect = InstantiateWall(pos, Quaternion.Euler(0, 0, angle), $"{position.x},{position.y},{position.z},_mv", verRoot);
                rect.localScale += Vector3.down * 0.5f;
            }
            else
            {
                var verObje = verRoot.Find($"{position.x},{position.y},{position.z},_mv");
                if (verObje != null)
                {
                    GameObject.DestroyImmediate(verObje.gameObject);
                }
            }
        }

        protected override void UpdateHorizontal(Vector3Int position, MazeData mazeData)
        {
            if (!IsFloorIndexValid(position.y, imagesRootMinimap)) return;
            var rootCurrent = imagesRootMinimap[position.y];
            if (rootCurrent == null) return;

            var horRoot = rootCurrent.transform.Find(RootHorWallName);
            if (horRoot == null) return;


            var size = cell.GetSizeCell(Vector2.one * sizeOfCell);

            var wallData = mazeData.GetFloor(position.y).listWallsHorizontal;
            int mapWidth = mazeData.GetFloor(position.y).sizeHorizontal.x;


            int index = mazeData.getIndex(position.x, position.z, mapWidth);
            var wallType = wallData[index];
            bool add = wallType == WallVisibilityStatus.VisibleInNormalMode;

            if (add)
            {
                var pos = GetHorizontalWallPosition(position.x, position.z, size);
                var rect = InstantiateWall(pos, Quaternion.Euler(0, 0, 90), $"{position.x},{position.y},{position.z},_mh", horRoot);
                rect.localScale += Vector3.down * 0.5f;
            }
            else
            {
                var verObje = horRoot.Find($"{position.x},{position.y},{position.z},_mh");
                if (verObje != null)
                {
                    GameObject.DestroyImmediate(verObje.gameObject);
                }
            }
        }
    }
}
