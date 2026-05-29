using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    public static class MazeObjectHelper
    {
        public static GameObject RecreateRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
                Object.DestroyImmediate(existing);

            return new GameObject(name);
        }

        public static GameObject FindObjectByName(Transform root, string name)
        {
            var t = root.Find(name);
            if (t == null)
            {
                Debug.LogWarning($"FindObjectByName: Objekt s názvem '{name}' nebyl nalezen v '{root.name}'");
                return null;
            }
            return t.gameObject;
        }
    }
}