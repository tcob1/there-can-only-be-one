using UnityEngine;
using UnityEngine.InputSystem;


public class TimetravelerInputs : MonoBehaviour
{

    public float chargeMod = 250f;

    private InputAction chargeTTAction;
    private InputAction chargeTTScroll;
    private bool chargingTT;
    private float scrollCharge;
    
    private float currCharge;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chargingTT = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        scrollCharge = chargeTTScroll.ReadValue<Vector2>().y;
        if (chargingTT)
        {
            
            currCharge += (chargeMod * scrollCharge * Time.deltaTime * Mathf.Sqrt(Mathf.Abs(currCharge) + .1f));
            if (currCharge > 43200) currCharge = 43200;
            if (currCharge < -43200) currCharge = -43200;
            print("Charge: " + (int) currCharge);
            
        }

        TimeHub.Instance.updateClock(TimeHub.Instance.getTime());
    }

    void FixedUpdate()
    {
        
    }

    void OnEnable()
    {
        chargeTTAction = InputSystem.actions.FindAction("Timetravel");
        chargeTTAction.Enable();
        chargeTTAction.started += OnTimetravelStarted;
        chargeTTAction.canceled += OnTimetravelCanceled;

        chargeTTScroll = InputSystem.actions.FindAction("ChargeTimetravel");
        chargeTTScroll.Enable();

    }

    void OnDisable()
    {
        chargeTTAction.Disable();
        chargeTTScroll.Disable();
    }

    void OnTimetravelStarted(InputAction.CallbackContext context)
    {
        chargingTT = true;

    }

    void OnTimetravelCanceled(InputAction.CallbackContext context)
    {
        
        TimeHub.Instance.timeChange((int) currCharge);
        chargingTT = false;
        currCharge = 0;
        
    }

    
}
