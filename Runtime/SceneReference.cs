/* 
 * This code is greatly inspired by some awesome projects :
 * 
 * https://github.com/JohannesMP/unity-scene-reference
 * https://github.com/Tymski/SceneReference
 * https://github.com/NibbleByte/UnitySceneReference
 * 
 */

using System;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[Serializable]
public class SceneReference : ISerializationCallbackReceiver
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset m_SceneAsset;

#pragma warning disable 0414 // Never used warning - will be used by SerializedProperty.
    // Used to dirtify the data when needed upon displaying in the inspector.
    // Otherwise the user will never get the changes to save (unless he changes any other field of the object / scene).
    [SerializeField] private bool m_IsDirty;
#pragma warning restore 0414
#endif // UNITY_EDITOR


    [SerializeField] private string m_ScenePath = string.Empty;
    public string ScenePath
    {
        get
        {
#if UNITY_EDITOR
            AutoUpdateReference();
#endif

            return m_ScenePath;
        }

        set
        {
            m_ScenePath = value;

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(m_ScenePath))
            {
                m_SceneAsset = null;
                return;
            }

            m_SceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(m_ScenePath);
            if (m_SceneAsset == null)
            {
                Debug.LogError($"Setting {nameof(SceneReference)} to {value}, but no scene could be located there.");
            }
#endif
        }
    }
    public string SceneName => Path.GetFileNameWithoutExtension(ScenePath);

    public bool IsEmpty => string.IsNullOrEmpty(ScenePath);

    public SceneReference() 
    {
    }

    public SceneReference(string scenePath)
    {
        ScenePath = scenePath;
    }

    public SceneReference(SceneReference other)
    {
        m_ScenePath = other.m_ScenePath;

#if UNITY_EDITOR
        m_SceneAsset = other.m_SceneAsset;
        m_IsDirty = other.m_IsDirty;

        AutoUpdateReference();
#endif
    }

    public override string ToString()
    {
        return m_ScenePath;
    }


    [Obsolete("Needed for the editor, don't use it in runtime code!", true)]
    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        AutoUpdateReference();
#endif
    }

    [Obsolete("Needed for the editor, don't use it in runtime code!", true)]
    public void OnAfterDeserialize()
    {
#if UNITY_EDITOR
        // OnAfterDeserialize is called in the deserialization thread so we can't touch Unity API.
        // Wait for the next update frame to do it.
        EditorApplication.update += OnAfterDeserializeHandler;
#endif
    }


#if UNITY_EDITOR
    private void OnAfterDeserializeHandler()
    {
        EditorApplication.update -= OnAfterDeserializeHandler;
        AutoUpdateReference();
    }

    private void AutoUpdateReference()
    {
        if (m_SceneAsset == null)
        {
            if (string.IsNullOrEmpty(m_ScenePath))
                return;

            SceneAsset foundAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(m_ScenePath);
            if (foundAsset)
            {
                m_SceneAsset = foundAsset;
                m_IsDirty = true;

                if (!Application.isPlaying)
                {
                    // NOTE: This doesn't work for scriptable objects, hence the m_IsDirty.
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }
        else
        {
            string foundPath = AssetDatabase.GetAssetPath(m_SceneAsset);
            if (string.IsNullOrEmpty(foundPath))
                return;

            if (foundPath != m_ScenePath)
            {
                m_ScenePath = foundPath;
                m_IsDirty = true;

                if (!Application.isPlaying)
                {
                    // NOTE: This doesn't work for scriptable objects, hence the m_IsDirty.
                    EditorSceneManager.MarkAllScenesDirty();
                }
            }
        }
    }
#endif // UNITY_EDITOR

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneReference))]
    [CanEditMultipleObjects]
    internal class SceneReferencePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isDirtyProperty = property.FindPropertyRelative("m_IsDirty");
            if (isDirtyProperty.boolValue)
            {
                isDirtyProperty.boolValue = false;
                // This will force change in the property and make it dirty.
                // After the user saves, he'll actually see the changed changes and commit them.
            }

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            const float buildSettingsWidth = 20f;
            const float padding = 2f;

            Rect assetPos = position;
            assetPos.width -= buildSettingsWidth + padding;

            Rect buildSettingsPos = position;
            buildSettingsPos.x += position.width - buildSettingsWidth + padding;
            buildSettingsPos.width = buildSettingsWidth;

            var sceneAssetProperty = property.FindPropertyRelative("m_SceneAsset");
            bool hadReference = sceneAssetProperty.objectReferenceValue != null;

            EditorGUI.PropertyField(assetPos, sceneAssetProperty, new GUIContent());

            string guid = string.Empty;
            int indexInSettings = -1;

            if (sceneAssetProperty.objectReferenceValue)
            {
                long localId;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sceneAssetProperty.objectReferenceValue, out guid, out localId))
                {
                    indexInSettings = Array.FindIndex(EditorBuildSettings.scenes, s => s.guid.ToString() == guid);
                }
            }
            else if (hadReference)
            {
                property.FindPropertyRelative("m_ScenePath").stringValue = string.Empty;
            }

            GUIContent settingsContent = indexInSettings != -1
                ? new GUIContent("-", "Scene is already in the Editor Build Settings. Click here to remove it.")
                : new GUIContent("+", "Scene is missing in the Editor Build Settings. Click here to add it.")
                ;

            Color prevBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = indexInSettings != -1 ? Color.red : Color.green;

            if (GUI.Button(buildSettingsPos, settingsContent, EditorStyles.miniButtonRight) && sceneAssetProperty.objectReferenceValue)
            {
                if (indexInSettings != -1)
                {
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.RemoveAt(indexInSettings);

                    EditorBuildSettings.scenes = scenes.ToArray();

                }
                else
                {
                    var newScenes = new EditorBuildSettingsScene[] {
                        new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAssetProperty.objectReferenceValue), true)
                    };

                    EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(newScenes).ToArray();
                }
            }

            GUI.backgroundColor = prevBackgroundColor;

            EditorGUI.EndProperty();
        }
    }
#endif // UNITY_EDITOR
}
