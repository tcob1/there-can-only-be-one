using UnityEngine;

public abstract class Holdable : MonoBehaviour
{
    [Header("Hold Settings")]
    public Transform rightHold; // assign the player’s right-hand hold position replace later with a PlayerHoldManager because we cant alloc rightHold at runtime
    //public Transform leftHold will be added later

    protected bool isHeld;

    private void Start()
    {
        if (transform.parent == rightHold)
        {
            isHeld = true;
        }
        else
        {
            isHeld = false;
        }
    }

    //we can call these when player raycasts onto holdable objects
    public virtual void OnHold()
    {
        isHeld = true;
        transform.SetParent(rightHold);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public virtual void OnRelease()
    {
        isHeld = false;
        //maybe addforce too if we want
        transform.SetParent(null);
    }
}