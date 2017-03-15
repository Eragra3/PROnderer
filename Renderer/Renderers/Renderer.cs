using MathNet.Numerics;
using MathNet.Spatial.Euclidean;
using Renderer.Helpers;
using Renderer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Renderer
{
    public abstract class WavefrontRenderer
    {
        protected readonly int height;
        protected readonly int width;

        protected WavefrontObj model;

        static WavefrontRenderer()
        {
            Control.UseNativeMKL();
            if (!Control.TryUseNativeMKL())
            {
                Trace.Fail("Couldn't start MKL", "Renderer may work much slower");
            }
        }

        protected WavefrontRenderer(int height, int width)
        {
            this.height = height;
            this.width = width;
        }

        public void SetModel(WavefrontObj model)
        {
            this.model = model ?? throw new ArgumentException("Faces cannot be null", nameof(model));
        }

        public virtual int[] RenderFrame(SceneSettings settings)
        {
            switch (settings.RenderingMode)
            {
                case RenderingMode.Normal:
                    return RenderNormal(settings);
                case RenderingMode.Wireframe:
                    return RenderWireframe(settings);
                case RenderingMode.Test:
                    return RenderTestScreen(settings);
                default:
                    throw new ArgumentException("Invalid rendering mode", nameof(settings.RenderingMode));
            }
        }

        public abstract int[] RenderNormal(SceneSettings settings);

        public virtual int[] RenderWireframe(SceneSettings settings)
        {
            CameraSettings camera = settings.CameraSettings;

            int[] frame = new int[this.height * this.width];
            for (int i = 0; i < frame.Length; i++)
            {
                frame[i] = Colors.WHITE;
            }

            foreach (Triangle triangle in this.model.Triangles)
            {
                DrawLine(triangle.Vertex1.Vertex, triangle.Vertex2.Vertex, camera, frame);
                DrawLine(triangle.Vertex2.Vertex, triangle.Vertex3.Vertex, camera, frame);
                DrawLine(triangle.Vertex3.Vertex, triangle.Vertex1.Vertex, camera, frame);
            }

            return frame;
        }

        public virtual int[] RenderTestScreen(SceneSettings settings)
        {
            var frame = new int[this.height * this.width];

            var n = 0;
            //Pbgra32 for some reason accepts argb format (?)
            var g = Colors.GREEN;
            var b = Colors.BLUE;
            var w = Colors.WHITE;

            var pixels = this.width * this.height / 5;

            for (; n < pixels; n++)
            {
                frame[n] = Colors.RED;
            }
            for (; n < pixels * 2; n++)
            {
                frame[n] = g;
            }
            for (; n < pixels * 3; n++)
            {
                frame[n] = b;
            }
            for (; n < pixels * 4; n++)
            {
                frame[n] = w;
            }
            for (; n < pixels * 5; n++)
            {
                frame[n] = Colors.BLACK;
            }

            return frame;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawLine(int[] frame, int color, Point2D p1, Point2D p2)
        {
            double x0 = p1.X;
            double y0 = p1.Y;
            double x1 = p2.X;
            double y1 = p2.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }

            double dX = x1 - x0;
            double dY = Math.Abs(y1 - y0);
            double err = dX / 2;
            double yStep = y0 < y1 ? 1 : -1;
            double y = y0;

            for (double x = x0; x <= x1; x++)
            {
                double tempX = x;
                double tempY = y;
                if (steep)
                {
                    Swap(ref tempX, ref tempY);
                }
                int index = JaggedToSingle(tempX, tempY);
                if (index < 0 || index >= frame.Length)
                    return;
                frame[index] = color;

                err -= dY;
                if (err < 0)
                {
                    y += yStep;
                    err += dX;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DrawLine(Vector3D u, Vector3D v, CameraSettings cameraSettings, int[] frame)
        {
            var scaledU = u.ScaleBy(cameraSettings.Zoom);
            var scaledV = v.ScaleBy(cameraSettings.Zoom);

            double x0 = scaledU.X + this.width / 2;
            double y0 = scaledU.Z + this.height / 2;
            double x1 = scaledV.X + this.width / 2;
            double y1 = scaledV.Z + this.height / 2;

            if ((x0 < 0 || x0 >= this.width
                || y0 < 0 || y0 >= this.height)
                && (x1 < 0 || x1 >= this.width
                || y1 < 0 || y1 >= this.height))
            {
                return;
            }

            var p0 = new Point2D(x0, y0);
            var p1 = new Point2D(x1, y1);

            DrawLine(frame, Colors.BLACK, p0, p1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int JaggedToSingle(double x, double y)
        {
            return JaggedToSingle((int)x, (int)y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int JaggedToSingle(int x, int y)
        {
            int inlineX = x;
            int inlineY = y * this.width;
            return inlineX + inlineY;
        }
    }
}
