using UnityEngine;

public class Interactabletest : Interactable
{
    public override void OnFocus()
    {
        print("LOOKING AT" + gameObject.name);
    }

    public override void OnInteract()
    {
        print("INTERACTING WITH" + gameObject.name);
    }

    public override void OnLoseFocus()
    {
        print("STOPPED LOOKING AT" + gameObject.name);
    }
}
