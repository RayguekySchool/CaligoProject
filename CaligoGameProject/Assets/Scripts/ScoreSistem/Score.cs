using UnityEngine;

public class Score : MonoBehaviour
{

    public Transform player;

    void Update()
    {
        Debug.Log(player.position.z);
    }
}
