using System.Collections.Generic;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    internal class MinimapUpdaterService
    {
        private readonly MinimapElevatorService iconService;

        public MinimapUpdaterService(MinimapElevatorService iconService)
        {
            this.iconService = iconService;
        }

        public void UpdateFloor(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            if (IsFloorIndexValid(position.y, imagesRootMinimap))
            {
                UpdateCurrentFloor(position, mazeData, imagesRootMinimap, rootFloorName);
                UpdateLowerFloor(position, mazeData, imagesRootMinimap, rootFloorName);
            }
        }

        public void UpdateElevator(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            if (!IsFloorIndexValid(position.y, imagesRootMinimap)) return;

            RemoveElevatorFromCurrentFloor(position, mazeData, imagesRootMinimap, rootFloorName);
            UpdateLowerFloorElevator(position, mazeData, imagesRootMinimap, rootFloorName);
            UpdateUpperFloorElevator(position, mazeData, imagesRootMinimap, rootFloorName);
        }

        private bool IsFloorIndexValid(int floor, List<GameObject> imagesRootMinimap)
        {
            return floor >= 0 && floor < imagesRootMinimap.Count;
        }

        private void RemoveElevatorFromCurrentFloor(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            var rootCurrent = imagesRootMinimap[position.y];
            if (rootCurrent == null) return;

            var floorRoot = rootCurrent.transform.Find(rootFloorName);
            if (floorRoot == null) return;

            var elevatorObj = floorRoot.Find($"{position.x},{position.y},{position.z},_mfe");
            if (elevatorObj != null)
            {
                GameObject.DestroyImmediate(elevatorObj.gameObject);
            }

            var elevatorDir = mazeData.ElevatorMove(position.x, position.y, position.z);
            AddElevator(floorRoot, elevatorDir, $"{position.x},{position.y},{position.z},_mf", GetFloorPosition(position, imagesRootMinimap, rootFloorName));
        }

        private void UpdateLowerFloorElevator(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            int lowerFloor = position.y - 1;
            if (!IsFloorIndexValid(lowerFloor, imagesRootMinimap)) return;

            var lower = imagesRootMinimap[lowerFloor];
            if (lower == null) return;

            var floorRoot = lower.transform.Find(rootFloorName);
            if (floorRoot == null) return;

            var elevatorObj = floorRoot.Find($"{position.x},{lowerFloor},{position.z},_mfe");
            if (elevatorObj != null)
            {
                GameObject.DestroyImmediate(elevatorObj.gameObject);
            }

            var elevatorDir = mazeData.ElevatorMove(position.x, lowerFloor, position.z);
            var pos = GetFloorPosition(new Vector3Int(position.x, lowerFloor, position.z), imagesRootMinimap, rootFloorName);
            AddElevator(floorRoot, elevatorDir, $"{position.x},{lowerFloor},{position.z},_mf", pos);
        }

        private void UpdateUpperFloorElevator(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            int upperFloor = position.y + 1;
            if (!IsFloorIndexValid(upperFloor, imagesRootMinimap)) return;

            var upper = imagesRootMinimap[upperFloor];
            if (upper == null) return;

            var floorRoot = upper.transform.Find(rootFloorName);
            if (floorRoot == null) return;

            var elevatorObj = floorRoot.Find($"{position.x},{upperFloor},{position.z},_mfe");
            if (elevatorObj != null)
            {
                GameObject.DestroyImmediate(elevatorObj.gameObject);
            }

            var elevatorDir = mazeData.ElevatorMove(position.x, upperFloor, position.z);
            var pos = GetFloorPosition(new Vector3Int(position.x, upperFloor, position.z), imagesRootMinimap, rootFloorName);
            AddElevator(floorRoot, elevatorDir, $"{position.x},{upperFloor},{position.z},_mf", pos);
        }

        private Vector2 GetFloorPosition(Vector3Int position, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            var floorObj = imagesRootMinimap[position.y]
                .transform.Find(rootFloorName)
                ?.Find($"{position.x},{position.y},{position.z},_mf");

            return floorObj != null
                ? floorObj.GetComponent<RectTransform>().anchoredPosition
                : Vector2.zero;
        }

        private void AddElevator(Transform parent, DirectionMoveEnum elevatorDir, string name, Vector2 position)
        {
            iconService.AddElevator(parent, elevatorDir, name, position);
        }

        private void UpdateCurrentFloor(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            var rootCurrent = imagesRootMinimap[position.y];
            if (rootCurrent == null) return;

            var floorRoot = rootCurrent.transform.Find(rootFloorName);
            if (floorRoot == null) return;

            var floorObj = floorRoot.Find($"{position.x},{position.y},{position.z},_mf");
            if (floorObj == null) return;

            var pos = floorObj.GetComponent<RectTransform>().anchoredPosition;
            var elevatorDir = mazeData.ElevatorMove(position.x, position.y, position.z);
            AddElevator(floorRoot, elevatorDir, $"{position.x},{position.y},{position.z},_mf", pos);
        }

        private void UpdateLowerFloor(Vector3Int position, MazeData mazeData, List<GameObject> imagesRootMinimap, string rootFloorName)
        {
            int lowerFloor = position.y - 1;
            if (!IsFloorIndexValid(lowerFloor, imagesRootMinimap)) return;

            var lower = imagesRootMinimap[lowerFloor];
            if (lower == null) return;

            var floorRoot = lower.transform.Find(rootFloorName);
            if (floorRoot == null) return;

            var elevatorObj = floorRoot.Find($"{position.x},{lowerFloor},{position.z},_mfe");
            if (elevatorObj != null)
            {
                GameObject.DestroyImmediate(elevatorObj.gameObject);
            }

            var elevatorDir = mazeData.ElevatorMove(position.x, lowerFloor, position.z);
            var pos = GetFloorPosition(new Vector3Int(position.x, lowerFloor, position.z), imagesRootMinimap, rootFloorName);
            AddElevator(floorRoot, elevatorDir, $"{position.x},{lowerFloor},{position.z},_mf", pos);
        }
    }
}
