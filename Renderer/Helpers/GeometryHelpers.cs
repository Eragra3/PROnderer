using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using Renderer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Helpers
{
    public static class GeometryHelpers
    {
        /// <summary>
        /// Returns null if there is no intersection
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3D? Raycast(Ray3D ray, Triangle triangle)
        {
            Vector3D u = triangle.Vertex2.Vertex - triangle.Vertex1.Vertex;
            Vector3D v = triangle.Vertex3.Vertex - triangle.Vertex1.Vertex;
            Vector3D n = u.CrossProduct(v);
            if (n.Length == 0)
            {
                return null;
            }

            UnitVector3D dir = ray.Direction;
            Vector3D w0 = ray.ThroughPoint.ToVector3D() - triangle.Vertex1.Vertex;
            double a = -n * w0;
            double b = n.DotProduct(dir);
            if (a != 0 && Math.Abs(b) < 0.00000001)
            {     // ray is  parallel to triangle plane
                return null;
            }
            double r = a / b;
            if (r < 0.0)
            {
                return null;
            }
            Vector3D intersectionWithPlane = ray.ThroughPoint.ToVector3D() + r * dir;

            Vector3D w = intersectionWithPlane - triangle.Vertex1.Vertex;
            double uv = u * v;
            double wv = w * v;
            double vv = v * v;
            double wu = w * u;
            double uv2 = uv * uv;
            double uu = u * u;
            double uuvv = uu * vv;
            double denominator = uv2 - uuvv;

            double s = (uv * wv - vv * wu) / denominator;
            double t = (uv * wu - uu * wv) / denominator;

            if (s < 0.0 || s > 1.0)
            {
                return null;
            }
            if (t < 0.0 || (s + t) > 1.0)
            {
                return null;
            }

            return intersectionWithPlane;
        }

        public static void DrawLine(int[] frame, Vector3D p1, Vector3D p2)
        {
            throw new NotImplementedException();
            UnitVector3D direction = (p2 - p1).Normalize();

            Point3D origin = p1.ToPoint3D();
            Point3D target = p2.ToPoint3D();
            Point3D newPoint = origin;

            while (newPoint != target)
            {

            }

        }

        public static Vector<double> NormalizeHomogenous(Vector<double> vector)
        {
            Vector<double> vec = vector / vector[3];
            return new DenseVector(new double[] { vec[0], vec[2], vec[2] });
        }
    }
}
