using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldofView))]
public class FieldofViewEditor : MonoBehaviour
{
    private void OnSceneGUI()
    {
        FieldofView fov = (FieldofView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360);
    }
}
