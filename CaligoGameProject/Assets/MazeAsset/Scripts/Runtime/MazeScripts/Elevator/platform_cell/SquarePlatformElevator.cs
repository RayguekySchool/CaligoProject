using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    [System.Serializable]
    internal class SquarePlatformElevator : ElevatorPlatformBase
    {
        public SquarePlatformElevator(MazeGeneratorManager coroutineOwner, TMP_Text textMeshPro, AnimatorElevatorService animatorService) 
            : base(coroutineOwner, textMeshPro, animatorService)
        {
        }

        protected override void InitData(float width)
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-width / 2, 0, -width / 2),
                new Vector3(width / 2, 0, -width / 2),
                new Vector3(width / 2, 0, width / 2),
                new Vector3(-width / 2, 0, width / 2)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
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
            boxCollider.size = new Vector3(platformMesh.bounds.size.x, platformMesh.bounds.size.y + 1f, platformMesh.bounds.size.z);
            boxCollider.isTrigger = true;
            var interaction = go.AddComponent<ElevatorInteractionSquare>();
            interaction.SetPlatform(this);
            return go;
        }
    }
}