using System.Collections.Generic;
using UnityEngine;

namespace MazeAsset.MazeGenerator
{
    public class ImageGenerationStrategy : IMazeGenerationStrategy
    {
        private List<Texture2D> _images;
        private List<float> _imageScaler;

        public ImageGenerationStrategy(List<Texture2D> images, List<float> imageScaler)
        {
            _images = images;
            _imageScaler = imageScaler;
        }


        MazeData IMazeGenerationStrategy.InitializeData()
        {
            var mazeData = new MazeData();
            mazeData.mazeArray = new List<WallsStateEnum[,]>();

            for (int i = 0; i < _images.Count; i++ )
            {
                var image = _images[i];
                var scale = _imageScaler[i];
                if (scale != 1)
                    image = TextureScaler.ResizeTexture(image, scale); 
                var mazeArray = ImgToMazeConvertor.GenerateMazeFromAlpha(image, scale);
                if (mazeArray != null)
                {
                    mazeData.mazeArray.Add(mazeArray);
                }
            }
            return mazeData;
        }
    }
}