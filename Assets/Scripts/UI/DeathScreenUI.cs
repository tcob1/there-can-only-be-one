using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DeathScreenUI : MonoBehaviour
{
    public void OnNewBranch()
    {
        GameManager.Instance.Respawn();
    }

    public void OnMainMenu()
    {
        SFXManager.Instance.StopLoopingMusic();
        SceneManager.LoadScene("MainMenu");
    }
}