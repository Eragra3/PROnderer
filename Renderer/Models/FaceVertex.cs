using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Models
{
    public class FaceVertex
    {
        public Vector3D Vertex;

        //texture coordinate, not yet implemented
        public Texture Texture;

        public Vector3D Normal;
    }
}
