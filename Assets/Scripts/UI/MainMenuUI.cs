using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main";

    private void Start()
    {
        SFXManager.Instance.PlayLoopingMusic("Anticipation", 0f, 0.8f);
    }

    public void OnPlay()
    {
        SFXManager.Instance.StopLoopingMusic();
        SceneManager.LoadScene(gameSceneName);
        
    }

    public void OnQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}