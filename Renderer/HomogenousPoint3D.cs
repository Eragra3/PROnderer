using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
    public class HomogenousPoint3D : DenseVector
    {
        public HomogenousPoint3D(Vector3D vector) : base(4)
        {
            this[0] = vector.X;
            this[1] = vector.Y;
            this[2] = vector.Z;
            this[3] = 1.0;
        }

        public HomogenousPoint3D(Point3D point) : base(4)
        {
            this[0] = point.X;
            this[1] = point.Y;
            this[2] = point.Z;
            this[3] = 1.0;
        }
    }
}
