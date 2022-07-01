using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditionalScenesLoader : MonoBehaviour
{
    [SerializeField] private List<SceneCollection> sceneCollections;
    [SerializeField] private List<SceneReference> otherScenes;
    private Scene currentScene;

    private void Awake()
    {
        currentScene = SceneManager.GetActiveScene();

        if (sceneCollections != null)
        {
            foreach (SceneCollection sceneCollection in sceneCollections)
            {
                if (sceneCollection != null)
                {
                    foreach (SceneReference sceneRef in sceneCollection.Scenes)
                    {
                        SceneManager.LoadSceneAsync(sceneRef.ScenePath, LoadSceneMode.Additive);
                    }
                }
            }
        }
        if (otherScenes != null)
        {
            foreach (SceneReference sceneRef in otherScenes)
            {
                SceneManager.LoadSceneAsync(sceneRef.ScenePath, LoadSceneMode.Additive);
            }
        }

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene unloadedScene)
    {
        if (unloadedScene == currentScene)
        {
            if (sceneCollections != null)
            {
                foreach (SceneCollection sceneCollection in sceneCollections)
                {
                    if (sceneCollection != null)
                    {
                        foreach (SceneReference sceneRef in sceneCollection.Scenes)
                        {
                            SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
                        }
                    }
                }
            }
            if (otherScenes != null)
            {
                foreach (SceneReference sceneRef in otherScenes)
                {
                    SceneManager.UnloadSceneAsync(sceneRef.ScenePath);
                }
            }
        }
    }
}
