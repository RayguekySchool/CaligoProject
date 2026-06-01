using UnityEngine;
using UnityEngine.UI;

namespace MazeAsset.MazeGenerator
{
    internal class MinimapElevatorService
    {
        private readonly Sprite upSprite;
        private readonly Sprite downSprite;
        private readonly Sprite bothSprite;
        private readonly int cellSize;

        public MinimapElevatorService(Sprite up, Sprite down, Sprite both, int sizeOfCell)
        {
            upSprite = up;
            downSprite = down;
            bothSprite = both;
            cellSize = sizeOfCell;
        }

        public void AddElevator(Transform parent, DirectionMoveEnum elevatorDir, string name, Vector2 position)
        {
            name += "e";
            Sprite selectedSprite = elevatorDir switch
            {
                DirectionMoveEnum.Up => upSprite,
                DirectionMoveEnum.Down => downSprite,
                DirectionMoveEnum.Both => bothSprite,
                _ => null
            };

            if (selectedSprite == null) return;

            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);

            var img = obj.AddComponent<Image>();
            img.sprite = selectedSprite;

            var rectTransform = img.rectTransform;
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = Vector2.one * cellSize;
        }
    }
}
