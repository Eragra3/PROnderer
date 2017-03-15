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
        public HomogenousPoint3D(Vector3D point) : base(4)
        {
            double[] values = new double[] { point.X, point.Y, point.Z, 1.0 };
            this.SetValues(values);
        }
        public HomogenousPoint3D(Point3D point) : base(4)
        {
            double[] values = new double[] { point.X, point.Y, point.Z, 1.0 };
            this.SetValues(values);
        }
    }
}
