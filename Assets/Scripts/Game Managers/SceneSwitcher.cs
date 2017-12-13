using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneSwitcher : MonoBehaviour {

    public Animator faderAnimator;
    public GameObject loadingScreen;
    public TMP_Text progressText;
    public Slider progressSlider;

    public static SceneSwitcher Active;

    public enum Scene { MainMenu, Space, Planet };
    public static Scene scene;

    public static System.Action OnSceneLoaded;

    void Awake() {
        if (Active != null) {
            Destroy(gameObject);
        }
        else {
            DontDestroyOnLoad(gameObject);
            Active = this;

            switch(SceneManager.GetActiveScene().name) {
                case ("MainMenu"):
                    scene = Scene.MainMenu;
                    break;
                case ("Space"):
                    scene = Scene.Space;
                    break;
                case ("Planet"):
                    scene = Scene.Planet;
                    break;
            }
        }
    }

    #region Public Functions
    public virtual void MainMenu(bool fade = false) {
        scene = Scene.MainMenu;
        if (fade) {
            faderAnimator.gameObject.SetActive(true);
            faderAnimator.SetInteger("Scene ID", 0);
            Fade();
        }
        else {
            faderAnimator.gameObject.SetActive(false);
            StartCoroutine(LoadSceneAsync(0));
        }
    }
    public virtual void Play(GameManager.World.Realm realm, bool fade = false) {
        scene = Scene.Space;
        if (fade) {
            faderAnimator.gameObject.SetActive(true);
            faderAnimator.SetInteger("Scene ID", (int)realm);
            Fade();
        }
        else {
            faderAnimator.gameObject.SetActive(false);
            StartCoroutine(LoadSceneAsync((int)realm));
        }
    }

    public virtual void Quit(bool fade = false) {
        if (fade) {
            faderAnimator.gameObject.SetActive(true);
            faderAnimator.SetInteger("Scene ID", -1);
            Fade();
        }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    IEnumerator LoadSceneAsync(int buildIndex) {

        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);

        loadingScreen.SetActive(true);

        while (!operation.isDone) {
            float percent = operation.progress / 0.9f * 100f;
            if (progressText != null) {
                progressText.text = percent.ToString() + "%";
            }
            if(progressSlider != null) {
                progressSlider.value = percent / 100;
            }
            yield return null;
        }

        loadingScreen.SetActive(false);
    }

    protected void Fade() {
        faderAnimator.ResetTrigger("Fade To White");
        faderAnimator.ResetTrigger("Fade To Black");
        faderAnimator.SetTrigger("Fade To Black");
    }

    void OnLevelLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
        faderAnimator.Rebind();
        if(OnSceneLoaded != null) {
            OnSceneLoaded.Invoke();
        }
    }

    void OnEnable() {
        SceneManager.sceneLoaded += OnLevelLoaded;
    }
    void OnDisable() {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }
#endregion
}
