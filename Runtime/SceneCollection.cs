using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SceneCollection : ScriptableObject
{
    [SerializeField] private List<SceneReference> scenes;

    public List<SceneReference> Scenes => scenes;
}
