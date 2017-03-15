using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pronderer
{
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        private int[] bits;
        public int[] Bits
        {
            get => bits;
            internal set
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    bits[i] = value[i];
                }
            }
        }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(
                width,
                height,
                width * 4,
                PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject()
                );
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
