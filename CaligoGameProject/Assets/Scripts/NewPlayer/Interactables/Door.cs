using System.Collections;
using UnityEngine;

public class Door : Interactable
{
    private bool isOpen = false;
    [SerializeField] private bool canBeInteractedWith = true;
    private Animator anim;

    [Header("Auto Close Settings")]
    [SerializeField] private float autoCloseDelay = 3f;
    [SerializeField] private float autoCloseDistance = 3f;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnFocus()
    {

    }

    public override void OnInteract()
    {
        if (canBeInteractedWith)
        {
            isOpen = !isOpen;

            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = FirstPersonController.instance.transform.position - transform.position;
            float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

            anim.SetFloat("dot", dot);
            anim.SetBool("isOpen", isOpen);

            StartCoroutine(AutoClose());
        }
    }

    public override void OnLoseFocus()
    {

    }

    private IEnumerator AutoClose()
    {
        while (isOpen)
        {
            yield return new WaitForSeconds(autoCloseDelay);

            if (Vector3.Distance(transform.position, FirstPersonController.instance.transform.position) > autoCloseDistance)
            {
                isOpen = false;
                anim.SetBool("isOpen", isOpen);
            }
        }
    }

    private void Animator_LockInteraction()
    {
        canBeInteractedWith = false;
    }

    private void Animator_UnlockInteraction()
    {
        canBeInteractedWith = true;
    }
}