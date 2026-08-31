using UnityEngine;

public class State : MonoBehaviour
{
    //This is the base class for all statements
    public virtual State Tick()
    {
        return this;
    }
}
