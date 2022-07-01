using UnityEngine;

public class SimpleQuitComponent : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#endif // UNIT_EDITOR

        Application.Quit();
    }
}

