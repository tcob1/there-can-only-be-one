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
        yield return new WaitForSeconds(deathScreenDelay);
        //UIManager.Instance.ShowDeathScreen();
    }

    private void HandleRespawn()
    {
        // drop items and reset time
        TimeHub.Instance.timeChange((int)-TimeHub.Instance.getTime());

        // re-enable player
        playerMovement.enabled = true;
        mouseLook.enabled = true;
        playerInteractions.enabled = true;
        playerRenderer.enabled = true;

        playerMovement.gameObject.layer = LayerMask.NameToLayer("Player");

        fists.SetActive(true);

        // lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // switch cameras back
        vcamPlayer.gameObject.SetActive(true);
        vcamDeath.gameObject.SetActive(false);

        // hide death screen
        //UIManager.Instance.HideDeathScreen();
    }
}