using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerInteractions playerInteractions;
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private GameObject fists;

    [Header("Cameras")]
    [SerializeField] private CinemachineCamera vcamPlayer;
    [SerializeField] private CinemachineCamera vcamDeath;
    [SerializeField] private float deathScreenDelay = 1f;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Transform cameraTarget;

    void Start()
    {
        GameManager.Instance.OnGameOver += HandleGameOver;
        GameManager.Instance.OnRespawn += HandleRespawn;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;
        GameManager.Instance.OnRespawn -= HandleRespawn;
    }

    private void HandleGameOver()
    {
        // disable player
        playerMovement.enabled = false;
        mouseLook.enabled = false;
        playerInteractions.enabled = false;
        playerRenderer.enabled = false;

        playerMovement.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        fists.SetActive(false);

        // unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // switch cameras
        vcamPlayer.gameObject.SetActive(false);
        vcamDeath.gameObject.SetActive(true);

        // wait for cam blend then show UI
        StartCoroutine(ShowDeathScreenAfterDelay());
    }

    private IEnumerator ShowDeathScreenAfterDelay()
    {
        yield return null;

        yield return new WaitUntil(() => !cinemachineBrain.IsBlending);

        yield return new WaitForSeconds(deathScreenDelay);

        UIManager.Instance.ShowTitle("YOU ARE NOT THE ONE");

        yield return new WaitForSeconds(2);

        UIManager.Instance.ShowDeathScreen();
    }

    private void HandleRespawn()
    {
        TimeHub.Instance.timeChange((int)-TimeHub.Instance.getTime());

        // reset rotation for seamless camera transition
        cameraTarget.rotation = vcamDeath.transform.rotation;
        mouseLook.SetRotation(vcamDeath.transform.rotation);

        // switch cameras back
        vcamPlayer.gameObject.SetActive(true);
        vcamDeath.gameObject.SetActive(false);

        // hide death screen
        UIManager.Instance.HideDeathScreen();

        // re-enable player after blend
        StartCoroutine(ReenablePlayerAfterBlend());
    }

    private IEnumerator ReenablePlayerAfterBlend()
    {
        // re-enable renderer immediately
        playerRenderer.enabled = true;

        yield return null;
        yield return new WaitUntil(() => !cinemachineBrain.IsBlending);

        playerMovement.enabled = true;
        mouseLook.enabled = true;
        playerInteractions.enabled = true;
        playerMovement.gameObject.layer = LayerMask.NameToLayer("Player");
        fists.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}