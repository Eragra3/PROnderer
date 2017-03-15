using Renderer;
using System.ComponentModel;
using System.Windows;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.IO;
using Renderer.Models;
using System.Windows.Input;

namespace Pronderer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double SCALE_MWHEEL_SPEED = 0.1;
        private const double SCALE_SHIFT_MAGNIFIER = 10;
        private const double SCALE_CTRL_MAGNIFIER = 0.1;

        private readonly int INT_SIZE = Marshal.SizeOf(typeof(int));

        private IRenderer xozRenderer;
        private BackgroundWorker xozRendererWorker;
        private WriteableBitmap xozBitmapBuffer;
        private int renderFramesInThisSecond;
        private SceneSettings xozSceneSettings;

        private DispatcherTimer fpsCounterTimer;

        public MainWindow()
        {
            InitializeComponent();

            InitializeScreens();

            InitializeRenderers();
            InitializeWorkers();

            InitializeFpsCounters();

            LoadFile("Objects/text.obj");

            StartRendering();

            UpdateLabels();
        }

        private void LoadFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"Path to file is empty", nameof(path));
            }
            if (!File.Exists(path))
            {
                throw new ArgumentException($"File {path} does not exist", nameof(path));
            }

            var extension = Path.GetExtension(path);
            switch (path)
            {
                case string _ when extension == ".obj":
                    var wavefront = WavefrontObj.Open(path);
                    this.xozRenderer.SetModel(wavefront);
                    this.XozLoadedModel.Content = path;
                    break;
                default:
                    throw new ArgumentException($"Extension {extension} is not supported!", nameof(path));
            }
        }

        private void InitializeFpsCounters()
        {
            this.fpsCounterTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            this.fpsCounterTimer.Tick += UpdateFpsCounter;
        }

        private void UpdateFpsCounter(object sender, EventArgs e)
        {
            int fps = Interlocked.Exchange(ref this.renderFramesInThisSecond, 0);
            this.MainFpsCounter.Content = fps;
        }

        private void InitializeScreens()
        {
            this.xozBitmapBuffer = new WriteableBitmap(800, 640,
                 96.0, 96.0, PixelFormats.Pbgra32, null);
            this.MainScreen.Source = this.xozBitmapBuffer;
            Loaded += (x, y) => Keyboard.Focus(this.XozCanvas);
        }

        private void InitializeWorkers()
        {
            this.xozRendererWorker = new BackgroundWorker();

            this.xozRendererWorker.DoWork += async (object sender, DoWorkEventArgs e) =>
            {
                while (!e.Cancel)
                {
                    await Task.Factory.StartNew(() =>
                       {
                           var frame = this.xozRenderer.RenderFrame(this.xozSceneSettings);

                           //temp dummy try/catch to prevent exceptions when stopping app
                           try
                           {
                               this.Dispatcher.Invoke(() =>
                           {
                               this.xozBitmapBuffer.Lock();
                               var bb = this.xozBitmapBuffer.BackBuffer;

                               Int32Rect newFrameRect = new Int32Rect(0, 0, (int)this.xozBitmapBuffer.Width, (int)this.xozBitmapBuffer.Height);
                               this.xozBitmapBuffer.WritePixels(newFrameRect, frame, (int)(this.xozBitmapBuffer.Width * this.INT_SIZE), 0);

                               this.xozBitmapBuffer.AddDirtyRect(newFrameRect);
                               this.xozBitmapBuffer.Unlock();

                               Interlocked.Increment(ref this.renderFramesInThisSecond);
                           });
                           }
                           catch (Exception)
                           {
                           }
                       });
                }
            };
        }

        private void StartRendering()
        {
            this.xozRendererWorker.RunWorkerAsync();

            this.fpsCounterTimer.Start();
        }

        private void InitializeRenderers()
        {
            this.xozRenderer = new PerspectiveRenderer((int)this.xozBitmapBuffer.Height, (int)this.xozBitmapBuffer.Width);
            var xozSceneSettings = new SceneSettings
            {
                ObserverX = 0,
                ObserverY = -100,
                ObserverZ = 5,
                Scale = 1,
                RenderingMode = RenderingMode.Normal,
                CameraSettings = new CameraSettings
                {
                    Near = 7,
                    Far = -100,
                    Left = -50,
                    Right = 50,
                    Top = 50,
                    Bottom = -50,
                    Zoom = 1
                }
            };
            this.xozSceneSettings = xozSceneSettings;
        }

        private void XozCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0)
            {
                return;
            }

            double change = SCALE_MWHEEL_SPEED * (e.Delta / Math.Abs(e.Delta));

            change *= GetMagnification();

            this.xozSceneSettings.CameraSettings.Zoom += change;

            if (this.xozSceneSettings.Scale < 0)
            {
                this.xozSceneSettings.Scale = 0;
            }

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            this.XozRenderingModeValue.Content = this.xozSceneSettings.RenderingMode;

            this.XozScaleValue.Content = this.xozSceneSettings.Scale.ToString("####0.0");
            this.XozNearValue.Content = this.xozSceneSettings.CameraSettings.Near.ToString("####0.0");
            this.XozFarValue.Content = this.xozSceneSettings.CameraSettings.Far.ToString("####0.0");
            this.XozLeftValue.Content = this.xozSceneSettings.CameraSettings.Left.ToString("####0.0");
            this.XozRightValue.Content = this.xozSceneSettings.CameraSettings.Right.ToString("####0.0");
            this.XozTopValue.Content = this.xozSceneSettings.CameraSettings.Top.ToString("####0.0");
            this.XozBottomValue.Content = this.xozSceneSettings.CameraSettings.Bottom.ToString("####0.0");

            this.XozZoomValue.Content = this.xozSceneSettings.CameraSettings.Zoom.ToString("####0.0");
        }

        private void Xoz_KeyDown(object sender, KeyEventArgs e)
        {
            double change = GetMagnification();

            if (e.Key == Key.Left)
            {
                this.xozSceneSettings.CameraSettings.Left += change;
                this.xozSceneSettings.CameraSettings.Right -= change;
            }

            if (e.Key == Key.Right)
            {
                this.xozSceneSettings.CameraSettings.Right += change;
                this.xozSceneSettings.CameraSettings.Left -= change;
            }

            if (e.Key == Key.Up)
            {
                this.xozSceneSettings.CameraSettings.Top += change;
                this.xozSceneSettings.CameraSettings.Bottom -= change;
            }
            if (e.Key == Key.Down)
            {
                this.xozSceneSettings.CameraSettings.Bottom += change;
                this.xozSceneSettings.CameraSettings.Top -= change;
            }
            if (e.Key == Key.Add)
            {
                this.xozSceneSettings.CameraSettings.Far += change;
            }
            if (e.Key == Key.Subtract)
            {
                this.xozSceneSettings.CameraSettings.Far -= change;
            }
            if (e.Key == Key.NumPad9)
            {
                this.xozSceneSettings.CameraSettings.Near += change;
            }
            if (e.Key == Key.NumPad6)
            {
                this.xozSceneSettings.CameraSettings.Near -= change;
            }
            if (e.Key == Key.F1)
            {
                this.xozSceneSettings.RenderingMode = RenderingMode.Normal;
            }
            else if (e.Key == Key.F2)
            {
                this.xozSceneSettings.RenderingMode = RenderingMode.Wireframe;
            }
            else if (e.Key == Key.F3)
            {
                this.xozSceneSettings.RenderingMode = RenderingMode.Vectors;
            }
            else if (e.Key == Key.F5)
            {
                this.xozSceneSettings.RenderingMode = RenderingMode.Test;
            }

            if (e.Key == Key.D1)
            {
                LoadFile("Objects/text.obj");
            }
            else if (e.Key == Key.D2)
            {
                LoadFile("Objects/house.obj");
            }
            else if (e.Key == Key.D3)
            {
                LoadFile("Objects/xyz.obj");
            }
            else if (e.Key == Key.D4)
            {
                LoadFile("Objects/axis.obj");
            }
            else if (e.Key == Key.D5)
            {
                LoadFile("Objects/cube.obj");
            }
            else if (e.Key == Key.D6)
            {
                LoadFile("Objects/offset_cube.obj");
            }
            else if (e.Key == Key.D7)
            {
                LoadFile("Objects/sphere.obj");
            }

            UpdateLabels();
        }

        private double GetMagnification()
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                return SCALE_CTRL_MAGNIFIER;
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                return SCALE_SHIFT_MAGNIFIER;
            }

            return 1;
        }
    }
}
