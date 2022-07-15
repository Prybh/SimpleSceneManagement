using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoaderComponent : MonoBehaviour
{
    [SerializeField] private SceneReference scene;

    public void LoadScene(LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(scene.ScenePath, loadSceneMode);
    }
}
