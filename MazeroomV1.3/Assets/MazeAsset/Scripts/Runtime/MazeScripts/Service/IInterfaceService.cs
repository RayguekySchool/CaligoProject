namespace MazeAsset.MazeGenerator
{
    internal interface IInterfaceService
    {
        internal IMinimap GetMinimap();
        internal ICell GetCell();
        internal IChunkRenderer GetChunkRenderer();
        internal IElevatorPlatform GetElevatorPlatform();
        internal IMazeGenerationService GetGenerationService();
        void Initialize();
        void ReInitInterface();
    }
}


