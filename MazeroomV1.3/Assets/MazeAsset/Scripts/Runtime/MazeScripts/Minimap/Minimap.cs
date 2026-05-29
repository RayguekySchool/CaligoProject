
using UnityEngine;
using UnityEngine.UI;

namespace MazeAsset.MazeGenerator
{
    internal class Minimap: MonoBehaviour
    {
        [SerializeField] internal MinimapChoiceEnum minimapShow;
        [SerializeField] internal Camera PlayerCamera;
        [SerializeField] internal Sprite player;
        [SerializeField, Range(0.5f, .8f)] internal float playerSize;
        [SerializeField] internal GameObject imageRootFromCanvas;
        [SerializeField] [Range(0.1f, 1)]internal float widthOfWall;
        [SerializeField] [Range(8, 30)]internal int sizeOfCell;
        [SerializeField] internal Sprite elevatorIconUp;
        [SerializeField] internal Sprite elevatorIconDown;
        [SerializeField] internal Sprite elevatorIconBoth;
        [SerializeField] internal Color colorForNotVisitedCell;
        [SerializeField] internal Color colorForVisitedCell;
    }
}
