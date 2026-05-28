using UnityEngine;
using UnityEngine.InputSystem;

public class TTParticles : MonoBehaviour
{
    private InputAction chargeTTAction;
    private ParticleSystem ps;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
    }

    void OnEnable()
    {
        chargeTTAction = InputSystem.actions.FindAction("Timetravel");
        chargeTTAction.Enable();
        chargeTTAction.started += OnTimetravelStarted;
        chargeTTAction.canceled += OnTimetravelCanceled;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTimetravelStarted(InputAction.CallbackContext context)
    {
        if (TimeHub.Instance.isTimetravelReady()) {ps.Play();}
        

    }

    void OnTimetravelCanceled(InputAction.CallbackContext context)
    {
        
        ps.Stop();
        
    }

    void OnDisable()
    {
        chargeTTAction.Disable();
    }

}
