using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace MazeAsset.MazeGenerator
{
    internal class InterfaceService : IInterfaceService
    {
        private MazeGeneratorManager mazeGeneratorManager;
        private ICell cellShape;
        private IChunkRenderer chunkRenderer;
        private IMinimap minimap;
        private IElevatorPlatform elevatorPlatform;
        private IMazeGenerationService mazeGenerationService;

        public InterfaceService(MazeGeneratorManager mazeGeneratorManager)
        {
            this.mazeGeneratorManager = mazeGeneratorManager;
        }

        ICell IInterfaceService.GetCell()
        {
            if (cellShape == null)
                (this as IInterfaceService).ReInitInterface();
            return cellShape;
        }

        IChunkRenderer IInterfaceService.GetChunkRenderer()
        {
            if (chunkRenderer == null)
                (this as IInterfaceService).ReInitInterface();
            return chunkRenderer;
        }

        IElevatorPlatform IInterfaceService.GetElevatorPlatform()
        {
            if (elevatorPlatform == null)
                (this as IInterfaceService).ReInitInterface();
            return elevatorPlatform;
        }

        IMazeGenerationService IInterfaceService.GetGenerationService()
        {
            if (mazeGenerationService == null)
            {
                mazeGenerationService = (mazeGeneratorManager.methodGenerate == MethodGenerateEnum.Image) ?
    new MazeGenerationService(new ImageGenerationStrategy(mazeGeneratorManager.imageForGenerateMaze
        , mazeGeneratorManager.imageScalers), (this as IInterfaceService).GetCell()) :
    new MazeGenerationService(new RandomGenerationStrategy(mazeGeneratorManager.floorDataList), 
    (this as IInterfaceService).GetCell());
            }
            return mazeGenerationService;
        }

        IMinimap IInterfaceService.GetMinimap()
        {
            if (minimap == null)
                (this as IInterfaceService).ReInitInterface();
            return minimap;
        }

        void IInterfaceService.Initialize()
        {
            var mazeColor = mazeGeneratorManager.GetComponent<MazeColor>();
            var materialMaze = mazeColor.mazeMaterial;
            var materialElevator = mazeColor.elevatorMaterial;
            var editMaterial = mazeColor.editMaterial;

            if (mazeGeneratorManager.shapeCell == ShapeCellEnum.Square)
            {
                cellShape = new SquareCell();
                minimap = new SquareMinimapImage(mazeGeneratorManager, cellShape, mazeGeneratorManager.GetDimensionsService);
                elevatorPlatform = new SquarePlatformElevator(mazeGeneratorManager, mazeGeneratorManager.textInteraction,
                    new AnimatorElevatorService(mazeGeneratorManager.scaler.y, mazeGeneratorManager.scaler.x, ShapeCellEnum.Square));
                chunkRenderer = new SquareChunkRenderer(new ObjectInstantiatorService(), mazeGeneratorManager.GetDimensionsService, materialMaze, materialElevator, editMaterial,
                    mazeGeneratorManager.scaler);
            }
            else
            {
                cellShape = new HexagonCell();
                minimap = new HexMinimapImage(mazeGeneratorManager, cellShape, mazeGeneratorManager.GetDimensionsService );
                elevatorPlatform = new HexagonPlatformElevator(mazeGeneratorManager, mazeGeneratorManager.textInteraction,
                    new AnimatorElevatorService(mazeGeneratorManager.scaler.y, mazeGeneratorManager.scaler.x, ShapeCellEnum.Hexagon));
                chunkRenderer = new HexagonChunkRenderer(new ObjectInstantiatorService(), mazeGeneratorManager.GetDimensionsService, materialMaze, materialElevator, editMaterial,
                    mazeGeneratorManager.scaler, mazeGeneratorManager.wallPrefab.transform.localScale.z);
            }
            mazeGenerationService = (mazeGeneratorManager.methodGenerate == MethodGenerateEnum.Image) ?
                new MazeGenerationService(new ImageGenerationStrategy(mazeGeneratorManager.imageForGenerateMaze
                    , mazeGeneratorManager.imageScalers), cellShape) :
                new MazeGenerationService(new RandomGenerationStrategy(mazeGeneratorManager.floorDataList), cellShape);
        }

        void IInterfaceService.ReInitInterface()
        {
            Vector3 playerPos = mazeGeneratorManager.GetComponent<MazeVisibleChunk>().transformPlayer.position;
            var mazeColor = mazeGeneratorManager.GetComponent<MazeColor>();
            var materialMaze = mazeColor.mazeMaterial;
            var materialElevator = mazeColor.elevatorMaterial;
            var editMaterial = mazeColor.editMaterial;
            var finder = new ElevatorFinderController();
            elevatorPlatform = finder.Search(mazeGeneratorManager.rootElevator, mazeGeneratorManager.shapeCell);
            if (elevatorPlatform == null)
            {
                if (mazeGeneratorManager.shapeCell == ShapeCellEnum.Square)
                {
                    elevatorPlatform = new SquarePlatformElevator(mazeGeneratorManager, mazeGeneratorManager.textInteraction,
                        new AnimatorElevatorService(mazeGeneratorManager.scaler.y, mazeGeneratorManager.scaler.x, ShapeCellEnum.Square));
                }
                else
                {
                    elevatorPlatform = new HexagonPlatformElevator(mazeGeneratorManager, mazeGeneratorManager.textInteraction,
                        new AnimatorElevatorService(mazeGeneratorManager.scaler.y, mazeGeneratorManager.scaler.x, ShapeCellEnum.Hexagon));
                }
            }
            else
            {
                elevatorPlatform.ReinitData();
            }
            if (mazeGeneratorManager.shapeCell == ShapeCellEnum.Square)
            {
                cellShape = new SquareCell();
                minimap = new SquareMinimapImage(mazeGeneratorManager, cellShape, mazeGeneratorManager.GetDimensionsService);
                chunkRenderer = new SquareChunkRenderer(new ObjectInstantiatorService(), mazeGeneratorManager.GetDimensionsService, materialMaze, materialElevator, editMaterial,
                    mazeGeneratorManager.scaler);
            }
            else
            {
                cellShape = new HexagonCell();
                minimap = new HexMinimapImage(mazeGeneratorManager, cellShape, mazeGeneratorManager.GetDimensionsService);
                chunkRenderer = new HexagonChunkRenderer(new ObjectInstantiatorService(), mazeGeneratorManager.GetDimensionsService, materialMaze, materialElevator, editMaterial,
                    mazeGeneratorManager.scaler, mazeGeneratorManager.wallPrefab.transform.localScale.z);
            }
            var indexInMap = cellShape.GetIndexInMap(playerPos, mazeGeneratorManager.scaler);
            minimap.InitData(mazeGeneratorManager.mazeData, mazeGeneratorManager.GetComponent<Minimap>(), mazeGeneratorManager.centerYMinimap, indexInMap);

        }
    }
}


