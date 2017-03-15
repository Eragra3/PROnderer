using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using Renderer.Helpers;
using Renderer.Models;
using System;
using static Renderer.Helpers.GeometryHelpers;

namespace Renderer
{
    public class XozRenderer : WavefrontRenderer, IRenderer
    {
        public XozRenderer(int height, int width) : base(height, width)
        {
        }

        public override int[] RenderNormal(SceneSettings settings)
        {
            if (this.model == null)
            {
                throw new InvalidOperationException("Model is not initialized");
            }

            double[] zBuffer = new double[this.height * this.width];

            int[] frame = new int[this.height * this.width];
            for (int i = 0; i < frame.Length; i++)
            {
                frame[i] = Colors.WHITE;
            }

            int widthOffset = this.width / 2;
            int heightOffset = this.height / 2;

            int viewportMinX = -widthOffset;
            int viewportMinY = -heightOffset;
            int viewportMaxX = widthOffset;
            int viewportMaxY = heightOffset;

            //projection matrix
            var M = Transformation3DMatrix.GetOrthographicProjectionMatrix(settings.CameraSettings);

            foreach (Triangle originalTriangle in this.model.Triangles)
            {
                Triangle triangle = originalTriangle.ScaleBy(settings.Scale);

                var p1 = new HomogenousPoint3D(triangle.Vertex1.Vertex);
                var p2 = new HomogenousPoint3D(triangle.Vertex2.Vertex);
                var p3 = new HomogenousPoint3D(triangle.Vertex3.Vertex);

                Vector<double> projectedP1 = p1 * M;
                Vector<double> projectedP2 = p2 * M;
                Vector<double> projectedP3 = p3 * M;

                Vector<double> s1 = NormalizeHomogenous(projectedP1);
                Vector<double> s2 = NormalizeHomogenous(projectedP2);
                Vector<double> s3 = NormalizeHomogenous(projectedP3);

                double dist1 = p1[1];
                double dist2 = p2[1];
                double dist3 = p3[1];

                int p1Index = JaggedToSingle(s1[0] * this.width + this.width / 2, s1[2] * this.height + this.height / 2);
                int p2Index = JaggedToSingle(s2[0] * this.width + this.width / 2, s2[2] * this.height + this.height / 2);
                int p3Index = JaggedToSingle(s3[0] * this.width + this.width / 2, s3[2] * this.height + this.height / 2);

                if (p1Index >= 0 && p1Index < zBuffer.Length && zBuffer[p1Index] < dist1)
                {
                    zBuffer[p1Index] = dist1;
                    //frame[index] = Colors.GetGray(1 - (dist / observer.Y));
                    frame[p1Index] = Colors.RED;
                }
                if (p2Index >= 0 && p2Index < zBuffer.Length && zBuffer[p2Index] < dist2)
                {
                    zBuffer[p2Index] = dist2;
                    //frame[index] = Colors.GetGray(1 - (dist / observer.Y));
                    frame[p2Index] = Colors.RED;
                }
                if (p3Index >= 0 && p3Index < zBuffer.Length && zBuffer[p3Index] < dist3)
                {
                    zBuffer[p3Index] = dist3;
                    //frame[index] = Colors.GetGray(1 - (dist / observer.Y));
                    frame[p3Index] = Colors.RED;
                }

                continue;

                //find bounding box
                double minX = Math.Floor(MathHelpers.Min(triangle.Vertex1.Vertex.X, triangle.Vertex2.Vertex.X, triangle.Vertex3.Vertex.X));//, viewportMinX);
                double maxX = Math.Ceiling(MathHelpers.Max(triangle.Vertex1.Vertex.X, triangle.Vertex2.Vertex.X, triangle.Vertex3.Vertex.X));//, viewportMaxX);
                double minZ = Math.Floor(MathHelpers.Min(triangle.Vertex1.Vertex.Z, triangle.Vertex2.Vertex.Z, triangle.Vertex3.Vertex.Z));//, viewportMinY);
                double maxZ = Math.Ceiling(MathHelpers.Max(triangle.Vertex1.Vertex.Z, triangle.Vertex2.Vertex.Z, triangle.Vertex3.Vertex.Z));//, viewportMaxY);



                for (int x = (int)minX; x >= minX && x <= maxX; x++)
                {
                    for (int z = (int)minZ; z >= minZ && z <= maxZ; z++)
                    {
                        int inlineX = x + widthOffset;
                        int inlineY = (z + heightOffset) * this.width;

                        int index = inlineX + inlineY;

                        if (index < 0 || index >= zBuffer.Length)
                        {
                            continue;
                        }

                        //for orthogonal projections observer is `infinitely wide`
                        var observer = new Vector3D(x, -200, z);
                        var ray = new Ray3D(observer.ToPoint3D(), UnitVector3D.YAxis);
                        Vector3D? intersection = Raycast(ray, triangle);
                        if (!intersection.HasValue)
                        {
                            continue;
                        }

                        double dist = Math.Abs(observer.Y - intersection.Value.Y);
                        //if object is behind observer, do not render
                        if (intersection.Value.Y < observer.Y)
                        {
                            continue;
                        }

                        if (zBuffer[index] < dist)
                        {
                            zBuffer[index] = dist;
                            //frame[index] = Colors.GetGray(1 - (dist / observer.Y));
                            frame[index] = Colors.RED;
                        }
                    }
                }
            }

            return frame;
        }

        public override int[] RenderWireframe(SceneSettings settings)
        {
            int[] frame = new int[this.height * this.width];
            for (int i = 0; i < frame.Length; i++)
            {
                frame[i] = Colors.WHITE;
            }

            var M = Transformation3DMatrix.GetOrthographicProjectionMatrix(settings.CameraSettings);
            //var S = Transformation3DMatrix.GetScalingMatrix(settings.CameraSettings);

            //M *= S;

            foreach (Vector3D vertex in this.model.Vertices)
            {
                var worldPoint = new HomogenousPoint3D(vertex);

                Vector<double> frustumPoint = worldPoint * M;

                if (!IsInFrustum(frustumPoint))
                {
                    continue;
                }

                Vector<double> frustumPointN = NormalizeHomogenous(frustumPoint);

                double dist = frustumPointN[1];

                int pointIndex = JaggedToSingle(frustumPointN[0] * this.width / 2 + this.width / 2, frustumPointN[2] * this.height / 2 + this.height / 2);

                if (pointIndex >= 0 && pointIndex < frame.Length)
                {
                    frame[pointIndex] = Colors.BLACK;
                }
            }

            return frame;
        }

        private static bool IsInFrustum(Vector<double> point)
        {
            double x = point[0];
            double y = point[1];
            double z = point[2];
            return x >= -1 && x <= 1
                && y >= -1 && y <= 1
                && z >= 0 && z <= 1;
        }

        public override int[] RenderVectors(SceneSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
