using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MazeAsset.CustomAttribute.Editor
{
    [InitializeOnLoad]
    public static class RequiredPlayModeBlocker
    {
        static RequiredPlayModeBlocker()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!ValidateRequiredFields())
                {
                    EditorApplication.isPlaying = false;
                }
            }
        }

        private static bool ValidateRequiredFields()
        {
            bool allValid = true;

            foreach (GameObject go in GetAllGameObjectsInOpenScenes())
            {
                foreach (var component in go.GetComponents<MonoBehaviour>())
                {
                    if (component == null) continue;

                    var fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    foreach (var field in fields)
                    {
                        if (field.GetCustomAttribute<Required>() != null)
                        {
                            if (typeof(Object).IsAssignableFrom(field.FieldType))
                            {
                                var value = field.GetValue(component) as Object;
                                if (value == null)
                                {
                                    string path = GetFullGameObjectPath(go);
                                    Debug.LogError($"[Required] '{field.Name}' is not set on component '{component.GetType().Name}' in GameObject: '{path}'", go);

                                    allValid = false;
                                }
                            }
                        }
                    }
                }
            }

            return allValid;
        }

        private static IEnumerable<GameObject> GetAllGameObjectsInOpenScenes()
        {
            var objects = new List<GameObject>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    objects.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
                }
            }

            return objects;
        }

        private static string GetFullGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

    }
}