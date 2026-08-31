using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public State currentState;

    private void FixedUpdate()
    {
        HandleStateMachine();
    }

    private void HandleStateMachine()
    {
        State nextState;

        if (currentState != null)
        {
            nextState = currentState.Tick();

            if (nextState != null)
            {
                currentState = nextState;
            }
        }
    }
}
