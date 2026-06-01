using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MazeAsset.MazeGenerator
{
    internal abstract class MinimapBase : IMinimap
    {
        const string RootParentName = "RootForMinimap";
        protected const string RootFloorName = "floors";
        protected const string RootVerWallName = "verticalWalls";
        protected const string RootHorWallName = "horizontalWalls";
        int lastFloor;
        float playerSize;
        protected int sizeOfCell;
        GameObject canvasRoot;
        protected ICell cell;
        protected MazeDimensionsService dimensionsService;
        protected Transform rootMinimap;
        MinimapElevatorService iconService;
        MinimapUpdaterService updaterService;

        protected Sprite floorSprite;
        protected Sprite wallSprite;
        protected Sprite playerSprite;
        protected float widthOfWall;
        protected List<GameObject> imagesRootMinimap;
        GameObject curentFloor;
        protected bool visibleAll;
        protected Color colorForVisited;
        protected Color colorForNotVisited;
        protected GameObject player;
        private PositionCalculatorService calculatorService;
        private readonly MazeGeneratorManager coroutineHost;
        MinimapChoiceEnum minimapChoiceEnum;
        protected HashSet<string> coroutineChunksPassed = new();

        protected MinimapBase(MazeGeneratorManager coroutineHost, ICell cell, MazeDimensionsService dimensionsService)
        {
            this.coroutineHost = coroutineHost;
            this.cell = cell;
            this.dimensionsService = dimensionsService;
        }

        protected private bool IsFloorIndexValid(int floor, List<GameObject> imagesRootMinimap)
        {
            return floor >= 0 && floor < imagesRootMinimap.Count;
        }

        protected PositionCalculatorService GetPositionCalculatorService()
        {
            if (calculatorService == null)
                calculatorService = new PositionCalculatorService();
            return calculatorService;
        }

        protected void TryDestroy(Transform root, string name)
        {

            var minimapWall = MazeObjectHelper.FindObjectByName(root, name);
            if (minimapWall != null)
            {
                GameObject.DestroyImmediate(minimapWall.gameObject);
            }
        }
        public virtual void InitData(MazeData mazeData, Minimap settings, int floorToMinimap, Vector2Int playerPosition)
        {
            imagesRootMinimap = new List<GameObject>(mazeData.floors.Count);
            for (int i = 0; i < mazeData.floors.Count; i++)
            {
                imagesRootMinimap.Add(null);
            }

            SaveSettings(settings);
            InitRoot();
            if (canvasRoot == null)
            {
                return;
            }
            AddPlayerIcon();
            GenerateSprites();
            (this as IMinimap).ChangeFloorMinimap(floorToMinimap, mazeData, playerPosition);
        }

        private void AddPlayerIcon()
        {
            player = new GameObject("player");
            player.transform.SetParent(canvasRoot.transform, false);
            player.AddComponent<Image>().sprite = playerSprite;
            player.GetComponent<RectTransform>().sizeDelta = Vector2.one * (sizeOfCell * playerSize);
        }


        void ChangeColor(Transform root, string name, Vector3Int positionIndex, MazeData mazeData)
        {
            var rootFloor = MazeObjectHelper.FindObjectByName(root, RootFloorName);
            if (rootFloor == null) return;
            var minimapFloor = MazeObjectHelper.FindObjectByName(rootFloor.transform, name);
            if (minimapFloor != null)
            {
                minimapFloor.GetComponent<Image>().color = colorForVisited;
                var position = minimapFloor.GetComponent<RectTransform>().anchoredPosition;
                var elevatorDir = mazeData.ElevatorMove(positionIndex.x, positionIndex.y, positionIndex.z);
                AddElevator(root, elevatorDir, name, position);
            }
        }
        void SaveSettings(Minimap settings)
        {
            minimapChoiceEnum = settings.minimapShow;
            playerSize = settings.playerSize;
            playerSprite = settings.player;
            visibleAll = settings.minimapShow == MinimapChoiceEnum.VisibleAllAfterStart;
            canvasRoot = settings.imageRootFromCanvas;
            widthOfWall = settings.widthOfWall;
            sizeOfCell = settings.sizeOfCell;
            var elevatorBothSprite = settings.elevatorIconBoth;
            var elevatorDownSprite = settings.elevatorIconDown;
            var elevatorUpSprite = settings.elevatorIconUp;
            iconService = new MinimapElevatorService(elevatorUpSprite, elevatorDownSprite, elevatorBothSprite, sizeOfCell);
            updaterService = new MinimapUpdaterService(iconService);
            colorForNotVisited = settings.colorForNotVisitedCell;
            colorForVisited = settings.colorForVisitedCell;
        }

        void InitRoot()
        {
            if (canvasRoot == null)
            {
                Debug.LogError("Canvas  not set");
                return;
            }
            var existing = canvasRoot.transform.Find(RootParentName);
            if (existing != null)
                GameObject.DestroyImmediate(existing.gameObject);
            existing = canvasRoot.transform.Find("player");
            if (existing != null)
                GameObject.DestroyImmediate(existing.gameObject);

            var rootObj = new GameObject(RootParentName, typeof(RectTransform));

            rootObj.transform.SetParent(canvasRoot.transform, false);
            rootMinimap = rootObj.transform;
        }

        void IMinimap.HandleStepToUnvisitedCell(GameObject cellObject, MazeData mazeData, bool visited, Vector2Int playerPos)
        {
            (var position, var name) = GetPositionCalculatorService().CalculatePositionWithName(cellObject);
            if (name != "_f" && name != "_e")
            {
                Debug.LogError("Cell is not valid");
                return;
            }
            if (lastFloor != position.y)
            {
                lastFloor = position.y;

                (this as IMinimap).ChangeFloorMinimap(lastFloor, mazeData, playerPos);
            }

            coroutineHost.StartCoroutine(WaitForMinimapReady(position.y, () =>
            {
                var root = imagesRootMinimap[position.y];
                if (root == null) return;

                UpdatePositionMap(position, root.transform);

                if (visited || minimapChoiceEnum == MinimapChoiceEnum.VisibleAllAfterStart)
                    return;

                ChangeColor(root.transform, $"{position.x},{position.y},{position.z},_mf", position, mazeData);

                var rootVer = MazeObjectHelper.FindObjectByName(root.transform, RootVerWallName)?.transform;
                var rootHor = MazeObjectHelper.FindObjectByName(root.transform, RootHorWallName)?.transform;

                if (rootVer != null)
                    DestroyVerticalWall(position, rootVer, mazeData);

                if (rootHor != null)
                    DestroyHorizontalWall(position, rootHor, mazeData);
            }));
        }

        private IEnumerator WaitForMinimapReady(int floorIndex, Action onReady)
        {
            string floorKey = $"{RootFloorName}_{floorIndex}";
            string horWallKey = $"{RootHorWallName}_{floorIndex}";
            string verWallKey = $"{RootVerWallName}_{floorIndex}";

            while (!(coroutineChunksPassed.Contains(floorKey) &&
                     coroutineChunksPassed.Contains(horWallKey) &&
                     coroutineChunksPassed.Contains(verWallKey)))
            {
                if (!coroutineChunksPassed.Contains(floorKey))
                    Debug.LogWarning($"[Minimap] Floor render not yet ready for floor {floorIndex}");
                if (!coroutineChunksPassed.Contains(horWallKey))
                    Debug.LogWarning($"[Minimap] Horizontal walls not yet ready for floor {floorIndex}");
                if (!coroutineChunksPassed.Contains(verWallKey))
                    Debug.LogWarning($"[Minimap] Vertical walls not yet ready for floor {floorIndex}");
                yield return null;
            }

            onReady?.Invoke();
        }



        protected Image CreateImage(Vector3 position, Sprite sprite, string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);

            var img = obj.AddComponent<Image>();
            img.sprite = sprite;

            var rectTransform = img.rectTransform;
            rectTransform.anchoredPosition = position; 
            rectTransform.sizeDelta = Vector2.one * sizeOfCell;
            return img;
        }
        protected void AddElevator(Transform parent, DirectionMoveEnum elevatorDir, string name, Vector2 position)
        {
            iconService.AddElevator(parent, elevatorDir, name, position);
        }
        protected RectTransform InstantiateWall(Vector3 pos, Quaternion rot, string name, Transform parent)
        {
            var img = CreateImage(pos, wallSprite, name, parent);
            var rectTransformImg = img.rectTransform;
            rectTransformImg.rotation = rot;
            rectTransformImg.localScale += Vector3.left * (0.98f - (widthOfWall / 12));
            return rectTransformImg;
        }

        void IMinimap.ChangeFloorMinimap(int floor, MazeData mazeData, Vector2Int playerPos)
        {
            int chunkSize = 100;
            if (curentFloor != null) curentFloor.SetActive(false);
            if (floor >= imagesRootMinimap.Count || floor < 0) return;

            if (imagesRootMinimap[floor] != null)
            {
                imagesRootMinimap[floor].SetActive(true);
                curentFloor = imagesRootMinimap[floor];
                return;
            }

            var _rootMinimap = new GameObject($"{floor}", typeof(RectTransform));
            _rootMinimap.transform.SetParent(rootMinimap, false);
            var rootFloor = new GameObject(RootFloorName, typeof(RectTransform));
            var rootVerWalls = new GameObject(RootVerWallName, typeof(RectTransform));
            var rootHorWalls = new GameObject(RootHorWallName, typeof(RectTransform));

            rootFloor.transform.SetParent(_rootMinimap.transform, false);
            rootVerWalls.transform.SetParent(_rootMinimap.transform, false);
            rootHorWalls.transform.SetParent(_rootMinimap.transform, false);
            imagesRootMinimap[floor] = _rootMinimap;
            curentFloor = _rootMinimap;
            var (width, height) = dimensionsService.GetWidthAndHeightFromFloor(floor, mazeData);
            lastFloor = floor;
            coroutineHost.StartCoroutine(RenderFloorInChunks(width, height, mazeData, floor, rootFloor.transform, chunkSize, playerPos));
            coroutineHost.StartCoroutine(RenderHorizontalWallsInChunks(width, height, mazeData, floor, rootHorWalls.transform, chunkSize, playerPos));
            coroutineHost.StartCoroutine(RenderVerticalWallsInChunks(width, height, mazeData, floor, rootVerWalls.transform, chunkSize, playerPos));
        }
        void IMinimap.HandleRotation(Vector3 rotationInDegrees)
        {
            if (player == null) return;
            float yRotation = rotationInDegrees.y;
            player.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, -yRotation);
        }

        void IMinimap.UpdateMinimap(Vector3Int position, string name, MazeData mazeData)
        {
            if (minimapChoiceEnum == MinimapChoiceEnum.VisibleAfterPlayerVisit) return;
            switch (name)
            {
                case "_f":
                    updaterService.UpdateFloor(position, mazeData, imagesRootMinimap, RootFloorName);
                    break;
                case "_e":
                    updaterService.UpdateElevator(position, mazeData, imagesRootMinimap, RootFloorName);
                    break;
                case "_v":
                    UpdateVertical(position, mazeData);
                    break;
                case "_h":
                    UpdateHorizontal(position, mazeData);
                    break;
            }
        }

        protected abstract void UpdateVertical(Vector3Int position, MazeData mazeData);
        protected abstract void UpdateHorizontal(Vector3Int position, MazeData mazeData);


        protected abstract void UpdatePositionMap(Vector3Int position, Transform rootCurentFloor);

        protected abstract IEnumerator RenderFloorInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int playerPos);
        protected abstract IEnumerator RenderHorizontalWallsInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int playerPos);
        protected abstract IEnumerator RenderVerticalWallsInChunks(int w, int h, MazeData mazeData, int floorIndex, Transform parent, int chunkSize, Vector2Int playerPos);
        protected abstract void DestroyVerticalWall(Vector3Int position, Transform root, MazeData mazeData);
        protected abstract void DestroyHorizontalWall(Vector3Int position, Transform root, MazeData mazeData);
        protected abstract void GenerateSprites();
    }
}
