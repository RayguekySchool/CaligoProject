using UnityEngine;
namespace MazeAsset.MazeGenerator
{
    internal static class TextureScaler
    {
        internal static Texture2D ResizeTexture(Texture2D source, float scaler)
        {
            int newWidth = Mathf.CeilToInt(source.width * scaler);
            int newHeight = Mathf.CeilToInt(source.height * scaler);

            Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int srcX = Mathf.FloorToInt(x / scaler);
                    int srcY = Mathf.FloorToInt(y / scaler);

                    result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                }
            }
            result.Apply();
            return result;
        }
    }
}
