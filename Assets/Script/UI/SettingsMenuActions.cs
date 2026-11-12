using UnityEngine;
using UnityEngine.SceneManagement;

// Actions for the settings menu buttons. Hides buttons that don't apply in this scene.
public class SettingsMenuActions : MonoBehaviour
{
    public SummonedMenu menu;
    public GameObject recenterButton;
    public GameObject backButton;
    public string mainMenuScene = "MainMenu";

    private PlaceInFrontOfUser placement;

    private void OnEnable()
    {
        if (placement == null) placement = FindObjectOfType<PlaceInFrontOfUser>();
        if (recenterButton != null) recenterButton.SetActive(placement != null);
        if (backButton != null) backButton.SetActive(SceneManager.GetActiveScene().name != mainMenuScene);
    }

    public void Close()
    {
        if (menu != null) menu.Close();
    }

    public void Recenter()
    {
        if (placement != null) placement.SnapToHead();
        Close();
    }

    public void BackToMainMenu() => SceneManager.LoadScene(mainMenuScene);

    public void QuitApp()
    {
#if UNITY_EDITOR
        // Application.Quit is ignored in the Editor, so stop Play mode instead.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}