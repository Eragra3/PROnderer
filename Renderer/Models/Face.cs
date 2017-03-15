using System;

namespace Renderer.Models
{
    /// <summary>
    /// Face is always a triangle.
    /// Triangulate your object before turning into faces
    /// </summary>
    public class Triangle
    {
        public FaceVertex Vertex1;
        public FaceVertex Vertex2;
        public FaceVertex Vertex3;

        internal Triangle ScaleBy(double scale)
        {
            var scaledV1 = new FaceVertex
            {
                Normal = this.Vertex1.Normal.ScaleBy(scale),
                Texture = this.Vertex1.Texture,
                Vertex = this.Vertex1.Vertex.ScaleBy(scale)
            };
            var scaledV2 = new FaceVertex
            {
                Normal = this.Vertex2.Normal.ScaleBy(scale),
                Texture = this.Vertex2.Texture,
                Vertex = this.Vertex2.Vertex.ScaleBy(scale)
            };
            var scaledV3 = new FaceVertex
            {
                Normal = this.Vertex3.Normal.ScaleBy(scale),
                Texture = this.Vertex3.Texture,
                Vertex = this.Vertex3.Vertex.ScaleBy(scale)
            };
            var scaled = new Triangle
            {
                Vertex1 = scaledV1,
                Vertex2 = scaledV2,
                Vertex3 = scaledV3
            };
            return scaled;
        }
    }
}