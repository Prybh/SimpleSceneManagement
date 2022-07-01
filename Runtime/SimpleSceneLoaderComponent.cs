using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoaderComponent : MonoBehaviour
{
    [SerializeField] private SceneReference scene;

    public void LoadScene()
    {
        SceneManager.LoadScene(scene.ScenePath);
    }
}
