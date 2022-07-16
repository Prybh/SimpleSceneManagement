using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneLoaderComponent : MonoBehaviour
{
    [SerializeField] private SceneReference scene;

    public void LoadScene()
    {
        LoadScene(additive: false);
    }

    public void LoadSceneAdditive()
    {
        LoadScene(additive: true);
    }

    public void LoadScene(bool additive)
    {
        SceneManager.LoadScene(scene.ScenePath, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
    }
}
