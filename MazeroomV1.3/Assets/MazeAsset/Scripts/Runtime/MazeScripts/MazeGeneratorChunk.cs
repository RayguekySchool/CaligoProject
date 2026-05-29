using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal class MazeGeneratorChunk
    {
        private readonly MazeGeneratorManager mazeManager;

        internal MazeGeneratorChunk(MazeGeneratorManager mazeGeneratorManager)
        {
            this.mazeManager = mazeGeneratorManager;
        }

        internal void GenerateChunk(int x, int y, int z, bool edit)
        {
            var wallVisibility = edit ? WallVisibilityStatus.VisibleInEditMode : WallVisibilityStatus.VisibleInNormalMode;
            var rootForRender = edit ? mazeManager.rootEdit : mazeManager.root;
            var rootForElevator = edit ? null : mazeManager.rootElevator;

            var chunks = mazeManager.InterfaceService.GetChunkRenderer().RenderChunk(
                x, z, y,
                wallVisibility,
                mazeManager.mazeData,
                rootForRender,
                rootForElevator,
                mazeManager.makeRoof,
                mazeManager.InterfaceService.GetElevatorPlatform(),
                mazeManager.wallPrefab
            );

            AddChunkToManagers(chunks, x, y, z, edit);
        }

        internal void CreateChunkAtPosition(Vector3 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;

            bool chunkWasGenerated = mazeManager.wallsObjectManager.VisibleObject(x, y, z);
            mazeManager.elevatorObjectManager.VisibleObject(x, y, z);

            if (!chunkWasGenerated)
            {
                var chunks = mazeManager.InterfaceService.GetChunkRenderer().RenderChunk(
                    x, z, y,
                    WallVisibilityStatus.VisibleInNormalMode,
                    mazeManager.mazeData,
                    mazeManager.root,
                    mazeManager.rootElevator,
                    mazeManager.makeRoof,
                    mazeManager.InterfaceService.GetElevatorPlatform(),
                    mazeManager.wallPrefab
                );

                AddChunkToManagers(chunks, x, y, z, edit: false);
            }

            if (mazeManager.editMode)
            {
                bool chunkWasGenEdit = mazeManager.editObjectManager.VisibleObject(x, y, z);

                if (!chunkWasGenEdit)
                {
                    var chunksEdit = mazeManager.InterfaceService.GetChunkRenderer().RenderChunk(
                        x, z, y,
                        WallVisibilityStatus.VisibleInEditMode,
                        mazeManager.mazeData,
                        mazeManager.rootEdit,
                        null,
                        mazeManager.makeRoof,
                        mazeManager.InterfaceService.GetElevatorPlatform(),
                        mazeManager.wallPrefab
                    );

                    AddChunkToManagers(chunksEdit, x, y, z, edit: true);
                }
            }
        }

        private void AddChunkToManagers((GameObject, GameObject) chunks, int x, int y, int z, bool edit)
        {
            if (edit)
            {
                mazeManager.editObjectManager.AddGameObject(chunks.Item1, x, y, z);
            }
            else
            {
                mazeManager.wallsObjectManager.AddGameObject(chunks.Item1, x, y, z);
                mazeManager.elevatorObjectManager.AddGameObject(chunks.Item2, x, y, z);
            }
        }
    }
}
