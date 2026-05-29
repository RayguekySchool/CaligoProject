using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MazeAsset.MazeGenerator
{
    [ExecuteInEditMode]
    [Serializable]
    [RequireComponent(typeof(MazeVisibleChunk))]
    [RequireComponent(typeof(MazeColor))]
    [RequireComponent(typeof(MazeScaler))]
    [RequireComponent(typeof(Minimap))]
    public class MazeGeneratorManager : MonoBehaviour
    {
        private const string NameForParent = "RootForChunk";
        private const string NameForElevatorParent = "RootForElevatorChunk";
        private const string NameForParentEdit = "RootForChunkEdit";

        [SerializeField, HideInInspector] internal GameObject root;
        [SerializeField, HideInInspector] internal GameObject rootEdit;
        [SerializeField, HideInInspector] internal GameObject rootElevator;

        [SerializeField] internal TMP_Text textInteraction;
        [SerializeField] internal bool makeRoof;
        [SerializeField, HideInInspector] public MazeData mazeData;
        [SerializeField] internal GameObject wallPrefab;
        [SerializeField] public MethodGenerateEnum methodGenerate;
        [SerializeField, HideInInspector] internal ShapeCellEnum shapeCell;
        [SerializeField] internal ShapeCellEnum shapeMaze;
        [HideInInspector, SerializeField] internal int ChunkSize;
        [Range(2, 150), SerializeField] internal int chunkSize;

        [SerializeField, HideInInspector] public bool editMode;
        [SerializeField, HideInInspector] internal Vector2 scaler;
        [SerializeField, HideInInspector] public List<Texture2D> imageForGenerateMaze;
        [SerializeField, HideInInspector] public List<Vector2Int> floorDataList;
        [SerializeField, HideInInspector] public List<float> imageScalers;
        [SerializeField, HideInInspector] public int sizeArray;
        [SerializeField, HideInInspector] internal int centerYMinimap;
        [SerializeField, HideInInspector] internal Vector3Int centerChunk;

        [SerializeField, HideInInspector] internal GameObjectManager wallsObjectManager;
        [SerializeField, HideInInspector] internal GameObjectManager elevatorObjectManager;
        [SerializeField, HideInInspector] internal GameObjectManager elObjectManager;
        [SerializeField, HideInInspector] internal GameObjectManager editObjectManager;

        private MazeVisibleChunk visibleChunk;
        private MazeDimensionsService dimensionsService;
        MazeGeneratorChunk mazeGeneratorChunk;
        private MazeChunkVisibilityService mazeChunkVisibilityService;
        private IInterfaceService interfaceService;


        internal MazeDimensionsService GetDimensionsService
        {
            get
            {
                if (dimensionsService == null)
                {
                    dimensionsService = new MazeDimensionsService(ChunkSize);
                }
                return dimensionsService;
            }
        }

        internal MazeGeneratorChunk GetMazeGeneratorChunk
        {
            get
            {
                if (mazeGeneratorChunk == null)
                {
                    mazeGeneratorChunk = new MazeGeneratorChunk(this);
                }
                return mazeGeneratorChunk;
            }
        }

        internal IInterfaceService InterfaceService
        {
            get
            {
                if (interfaceService == null)
                {
                    interfaceService = new InterfaceService(this);
                }
                return interfaceService;
            }
        }

        internal MazeChunkVisibilityService GetMazeChunkVisibilityService
        {
            get
            {
                if (mazeChunkVisibilityService == null)
                {
                    mazeChunkVisibilityService = new MazeChunkVisibilityService(this);
                }
                return mazeChunkVisibilityService;
            }
        }

        internal MoveGOFromChunkToOtherChunk moveTool;

        public void ChangeSizeArrayInMazeColor(int ListSize)
        {
            sizeArray = ListSize;
            GetComponent<MazeColor>().UpdateSpecificationArray();
        }

        internal void AddElevator(GameObject obj, int x, int y, int z)
        {
            elObjectManager.AddGameObject(obj, x, y, z);
        }

        internal GameObject GetElevator(int x, int y, int z)
        {
            return elObjectManager.GetGameObject(x, y, z);
        }

        internal bool RemoveElevator(int x, int y, int z)
        {
            return elObjectManager.RemoveGameObject(x, y, z);
        }
        public void GenerateMaze(bool removeWalls = false, bool createNewWals = true)
        {
            this.StopAllCoroutines();
            SaveGenerateData(createNewWals);
            InitRootParent();
            InitData(createNewWals);

            if (createNewWals)
            {
                var mazeGenerationService = InterfaceService.GetGenerationService();
                if (!mazeGenerationService.Initialize()) return;

                mazeGenerationService.GenerateData(removeWalls, visibleChunk.VisibleAllMaze);
                mazeData = mazeGenerationService.MazeData;
                MazeVisibleChunk mazeVisibleChunk = GetComponent<MazeVisibleChunk>();
                Vector3 playerPos = GetComponent<MazeVisibleChunk>().transformPlayer.position;
                var indexMap = InterfaceService.GetCell().GetIndexInMap(playerPos, scaler);
                InterfaceService.GetMinimap().InitData(mazeData, GetComponent<Minimap>(), centerYMinimap, indexMap);
            }
            if (visibleChunk.VisibleAllMaze)
                GenerateFullMaze();
            else
                GenerateMazeWithPosition();
            var minimapsSettings = GetComponent<Minimap>();
        }

        private void SaveGenerateData(bool createNewWals)
        {
            ChunkSize = chunkSize;
            visibleChunk = GetComponent<MazeVisibleChunk>();
            scaler = GetComponent<MazeScaler>().scalerVector;
            if (createNewWals) shapeCell = shapeMaze;
            var mazeColor = GetComponent<MazeColor>();
            mazeColor.hex = shapeCell == ShapeCellEnum.Hexagon;
            mazeColor.scaler = scaler;
            mazeColor.UpdateMaterial();
        }

        internal void GenerateFullMaze(bool edit = false) =>
            InterfaceService.GetGenerationService().GenerateFullMaze(mazeData, GetDimensionsService, GetMazeGeneratorChunk, edit);

        internal void GenerateMazeWithPosition(bool edit = false) =>
            InterfaceService.GetGenerationService().GenerateMazeWithPosition(GetComponent<MazeVisibleChunk>(), InterfaceService.GetCell(), ChunkSize,
                scaler, mazeData, GetMazeGeneratorChunk, GetDimensionsService, edit);

        private void InitRootParent()
        {
            root = MazeObjectHelper.RecreateRoot(NameForParent);
            rootElevator = MazeObjectHelper.RecreateRoot(NameForElevatorParent);
        }

        private void InitData(bool initData = true)
        {
            interfaceService = new InterfaceService(this);
            elevatorObjectManager = new GameObjectManager();
            elevatorObjectManager = new GameObjectManager();
            wallsObjectManager = new GameObjectManager();
            if (initData)
            {
                mazeData?.ClearAllData();
                mazeData = new MazeData();
            }
        }

        public void StartEditMode() =>
            new MazeEditService().StartEditMode(this, NameForParentEdit);

        internal void ReInitInterface() =>
            InterfaceService.ReInitInterface();


        public void StopEditMode()
         => new MazeEditService().StopEditMode(this);

        public void HandleMouseClick(Vector2 mousePosition)
         =>  new MazeEditService().HandleMouseClick(mousePosition, this, moveTool);


        internal void UpdateChunksWithPlayer(Vector3 positionPlayer) =>
            GetMazeChunkVisibilityService.UpdateChunksWithPlayer(positionPlayer);

        internal void DestroyChunkAtPosition(Vector3 position)
        {
            wallsObjectManager.HideObject((int)position.x, (int)position.y, (int)position.z);

            elevatorObjectManager.HideObject((int)position.x, (int)position.y, (int)position.z);

            if (editMode)
            {
                editObjectManager.HideObject((int)position.x, (int)position.y, (int)position.z);
            }
        }

        internal void CreateChunkAtPosition(Vector3 position ) =>
            GetMazeGeneratorChunk.CreateChunkAtPosition(position);

        internal void HandleCellEnter(GameObject cell, bool visited)
        =>  
             InterfaceService.GetMinimap().HandleStepToUnvisitedCell(cell, mazeData, visited, 
                InterfaceService.GetCell().GetIndexInMap(GetComponent<MazeVisibleChunk>().transformPlayer.position, scaler)
                );

        internal void HandleRotation(Vector3 rotationInDegrees)
            => InterfaceService.GetMinimap().HandleRotation( rotationInDegrees);

        internal void UpdateMinimap(Vector3Int position, string name)
       
             => InterfaceService.GetMinimap().UpdateMinimap(position,  name, mazeData);
        
    }
}