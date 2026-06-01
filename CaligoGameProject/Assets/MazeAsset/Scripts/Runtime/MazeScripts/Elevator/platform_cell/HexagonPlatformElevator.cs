using TMPro;
using UnityEngine;



namespace MazeAsset.MazeGenerator
{
    [System.Serializable]
    internal class HexagonPlatformElevator : ElevatorPlatformBase
    {
        public HexagonPlatformElevator(MazeGeneratorManager coroutineOwner, TMP_Text textMeshPro, AnimatorElevatorService animatorService) 
            : base(coroutineOwner, textMeshPro, animatorService)
        {
        }

        protected override void InitData(float width)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            Vector3[] vertices = new Vector3[7];
            vertices[0] = Vector3.zero;
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI * 2f / 6;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * width;
            }
            mesh.vertices = vertices;

            int[] triangles = new int[18];
            for (int i = 0; i < 6; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 2] = i + 1;
                triangles[i * 3 + 1] = (i + 1) % 6 + 1;
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            platformMesh = mesh;
        }

        public override GameObject GetPlatform(float s)
        {
            var go = new GameObject();
            var meshFilter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshFilter.mesh = platformMesh;
            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = platformMesh;
            meshCollider.convex = true;
            var boxCollider = go.AddComponent<BoxCollider>();
            var inter = this as IElevatorPlatform;
            boxCollider.size = new Vector3(1, (platformMesh.bounds.size.y + inter.HeightBoxForInteract) * animatorService.height, 1.73f);
            boxCollider.center = new Vector3(0, boxCollider.size.y / 2, 0);
            boxCollider.isTrigger = true;
            var interaction = go.AddComponent<ElevatorInteractionHex>();
            interaction.SetPlatform(this);
            return go;
        }

    }
}

