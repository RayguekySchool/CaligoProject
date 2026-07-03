using TMPro;
using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    public float DetectRange = 10;

    bool isInAngle, isInRange, isNotHiding;

    public GameObject player;
    TMP_Text Rangetext;

    void Start()
    {
        
    }

    private void Update()
    {
        isInAngle = false;
        isInRange = false;
        isNotHiding = false;

        if(Vector3.Distance(transform.position, player.transform.position) <= DetectRange)
        {
            isInRange = true;
        }

        else
        {
            Rangetext.text = "Player is out of range";
            Rangetext.color = Color.green;
        }
    }

}
