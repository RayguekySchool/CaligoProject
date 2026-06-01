using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    public class ElevatorInteractionHex : MonoBehaviour
    {
        [SerializeField] HexagonPlatformElevator platformReference;
        internal HexagonPlatformElevator GetPlatform()
        {
            return platformReference;
        }
        internal void SetPlatform(HexagonPlatformElevator platform)
        {
            platformReference = platform;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                platformReference.Interact(this.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                platformReference.StopInteract(this.gameObject);
            }
        }
    }
}
