using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
namespace MazeAsset.MazeGenerator.Editor
{
    [CustomEditor(typeof(MazeGeneratorManager))]
    [InitializeOnLoad]
    public class MazeGeneratorManagerEditor : UnityEditor.Editor
    {
        private static MazeGeneratorManager mazeController;
        private static bool EditMode = true;

        private List<Vector2Int> previousFloorDataList;
        private List<float> previousImageScaler;
        private List<Texture2D> previousImageList;

        static MazeGeneratorManagerEditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (mazeController != null && mazeController.editMode)
                {
                    mazeController.StopEditMode();
                }
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditMode = true;
            }
            else if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditMode = false;
            }
        }



        private static void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                HandleMouseClick(e.mousePosition);

                HandleUtility.Repaint();
            }
        }


        private static void HandleMouseClick(Vector2 mousePosition)
        {
            if (mazeController == null) return;
            if (mousePosition == null || !mazeController.editMode) return;
            mazeController.HandleMouseClick(mousePosition);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        public override void OnInspectorGUI()
        {

            var GOtargets = ((MazeGeneratorManager)target).gameObject;
            var targetsScript = GOtargets.GetComponents<MazeGeneratorManager>();

            if (targetsScript.Length > 1)
            {
                mazeController = null;
                Debug.LogError("Must be only one script MazeGeneratorManager");
                return;
            }
            mazeController = (MazeGeneratorManager)target;

            if (previousFloorDataList == null)
                previousFloorDataList = new List<Vector2Int>(mazeController.floorDataList);
            if (previousImageScaler == null)
                previousImageScaler = new List<float>(mazeController.imageScalers);
            if (previousImageList == null)
                previousImageList = new List<Texture2D>(mazeController.imageForGenerateMaze);
            DrawDefaultInspector();
            if (mazeController.methodGenerate == MethodGenerateEnum.Image)
            {
                int listSize = EditorGUILayout.IntSlider("Number of Images", mazeController.imageForGenerateMaze.Count, 1, 12);
                if (mazeController.sizeArray != listSize)
                {
                    mazeController.ChangeSizeArrayInMazeColor(listSize);
                    EditorUtility.SetDirty(mazeController);
                }
                if (mazeController.imageForGenerateMaze == null)
                {
                    mazeController.imageForGenerateMaze = new List<Texture2D>();
                }
                if (mazeController.imageScalers == null)
                {
                    mazeController.imageScalers = new List<float>(new float[mazeController.imageForGenerateMaze.Count]);
                }

                while (listSize > mazeController.imageForGenerateMaze.Count)
                {
                    mazeController.imageForGenerateMaze.Add(null);
                    mazeController.imageScalers.Add(1.0f);
                }
                while (listSize < mazeController.imageForGenerateMaze.Count)
                {
                    mazeController.imageForGenerateMaze.RemoveAt(mazeController.imageForGenerateMaze.Count - 1);
                    mazeController.imageScalers.RemoveAt(mazeController.imageScalers.Count - 1);
                }
                if (mazeController.imageScalers.Count != mazeController.imageForGenerateMaze.Count)
                {
                    mazeController.imageScalers = new List<float>(new float[mazeController.imageForGenerateMaze.Count]);
                }

                for (int i = 0; i < mazeController.imageForGenerateMaze.Count; i++)
                {
                    mazeController.imageForGenerateMaze[i] = (Texture2D)EditorGUILayout.ObjectField($"Image {i + 1}", mazeController.imageForGenerateMaze[i], typeof(Texture2D), false);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Scale", GUILayout.Width(50));
                    mazeController.imageScalers[i] = EditorGUILayout.Slider(mazeController.imageScalers[i], 0.1f, 5.0f);
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(GetSize(mazeController.imageForGenerateMaze[i], mazeController.imageScalers[i]) , GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }

                if (!ListsAreEqual(previousImageList, mazeController.imageForGenerateMaze))
                {
                    EditorUtility.SetDirty(mazeController);
                    previousImageList = new List<Texture2D>(mazeController.imageForGenerateMaze);
                }
                if (!ListsAreEqual(previousImageScaler, mazeController.imageScalers))
                {
                    EditorUtility.SetDirty(mazeController);
                    previousImageScaler = new List<float>(mazeController.imageScalers);
                }
            }

            else if (mazeController.methodGenerate == MethodGenerateEnum.DefaultShape)
            {
                int minX = 1, maxX = 1000;
                int minY = 1, maxY = 1000;
                int listSize = EditorGUILayout.IntSlider("Number of Floors", mazeController.floorDataList.Count, 1, 12);
                if (mazeController.sizeArray != listSize)
                {
                    mazeController.ChangeSizeArrayInMazeColor(listSize);
                    EditorUtility.SetDirty(mazeController);
                }
                while (listSize > mazeController.floorDataList.Count)
                {
                    mazeController.floorDataList.Add(new Vector2Int());
                }
                while (listSize < mazeController.floorDataList.Count)
                {
                    mazeController.floorDataList.RemoveAt(mazeController.floorDataList.Count - 1);
                }


                for (int i = 0; i < mazeController.floorDataList.Count; i++)
                {
                    var floorData = mazeController.floorDataList[i];
                    EditorGUILayout.LabelField($"Floor {i + 1}", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Width", GUILayout.Width(50));
                    floorData.x = Mathf.Clamp(EditorGUILayout.IntSlider(floorData.x, minX, maxX), minX, maxX);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Height", GUILayout.Width(50));
                    floorData.y = Mathf.Clamp(EditorGUILayout.IntSlider(floorData.y, minY, maxY), minY, maxY);
                    EditorGUILayout.EndHorizontal();
                    mazeController.floorDataList[i] = floorData;
                }

                if (!ListsAreEqual(previousFloorDataList, mazeController.floorDataList))
                {
                    EditorUtility.SetDirty(mazeController);
                    previousFloorDataList = new List<Vector2Int>(mazeController.floorDataList);
                }
            }

            if (EditMode)
            {
                if (mazeController.editMode)
                {
                    if (GUILayout.Button("Stop edit mode"))
                    {
                        mazeController.StopEditMode();
                    }
                }
                else
                {
                    if (mazeController.mazeData != null && mazeController.mazeData.floors.Count > 0 && GUILayout.Button("RECALCULATE MAZE"))
                    {
                        mazeController.GenerateMaze(false, false);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    if (GUILayout.Button("GENERATE WALL"))
                    {
                        mazeController.GenerateMaze();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    if (GUILayout.Button("GENERATE WALL - with destroy Walls"))
                    {
                        mazeController.GenerateMaze(true);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                    if (GUILayout.Button("Start edit mode"))
                    {
                        mazeController.StartEditMode();
                    }
                }
            }
        }

        private bool ListsAreEqual<T>(List<T> list1, List<T> list2)
        {
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    return false;
            }
            return true;
        }

        private string GetSize(Texture2D texture2D, float scaler)
        {
            if (texture2D == null) return "Texture not set";

            if (!texture2D.isReadable)
                return($"Input texture is not must be readable. Check 'Readable");
            return $"W: {Mathf.CeilToInt(texture2D.width * scaler)}, H:{Mathf.CeilToInt (texture2D.height * scaler)}"; 
        }
    }
}