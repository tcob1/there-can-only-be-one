using UnityEngine;

public class Fists : Usable
{
    [SerializeField] private Inventory inventory;

    protected override void Start()
    {
        //there is no base.Start() because fists aren't a world item
        isHeld = true;
        layerMask = ~LayerMask.GetMask("InteractableDetector", "Player");
        inventory.OnInventoryChanged += UpdateVisibility;
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        inventory.OnInventoryChanged -= UpdateVisibility;
    }

    private void UpdateVisibility()
    {
        gameObject.SetActive(!inventory.HasItemInHand());
    }

    private void LateUpdate()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 10f);
    }
}