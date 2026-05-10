using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Main";
    [SerializeField] private GameObject safe;

    private void Start()
    {
        SFXManager.Instance.PlayLoopingMusic("Anticipation", 0f, 0.8f);

    }

    public void OnPlay()
    {

        Animator animator = safe.GetComponent<Animator>();
        animator.SetTrigger("SpinOpen");
        StartCoroutine(LoadAfterAnimation(animator));
    }

    private IEnumerator LoadAfterAnimation(Animator animator)
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("USE-THIS-ANIMATION"));
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Open"));
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