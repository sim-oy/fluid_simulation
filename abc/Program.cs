using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Color = SFML.Graphics.Color;
using System.Diagnostics;
using System.Reflection;
using abc;
using OpenCL;
using System.Linq;
using System.Drawing;

namespace fluid_simulation
{
    class Program
    {
        public const int WINDOW_WIDTH = 500;
        public const int WINDOW_HEIGHT = 500;

        // 1: draw all particles as positions
        // 2: draw attribute in resolution
        // 3: draw all particles as areas of effect
        private const int DrawStyle = 1;

        // If DrawStyle = 2
        // 1: particles in pixel
        // 2: average velocity in pixel
        private const int drawType = 2;
        // 0: no blur
        // 1: blur
        private const bool blur = true;

        // only odd numbers
        private static double[] blurring = GaussCurvaMatrix(11);

        // If DrawStyle = 2
        private static int resolution_x = roundNextUp(25, WINDOW_WIDTH);
        private static int resolution_y = roundNextUp(25, WINDOW_HEIGHT);

        public const int FPS_LIMIT = -1;
        public static long FRAMETIME = 1000 / FPS_LIMIT;

        private static RenderWindow window;
        private static byte[] windowBuffer;

        private static Texture windowTexture;
        private static Sprite windowSprite;

        static void Main()
        {
            Console.WriteLine("start");

            Environments env = new Environments((int)Math.Pow(50, 2));

            window = new RenderWindow(new VideoMode(WINDOW_WIDTH, WINDOW_HEIGHT), "Computational fluid dynamics", Styles.Default);
            window.Closed += new EventHandler(OnClose);

            windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
            
            windowTexture = new Texture(WINDOW_WIDTH, WINDOW_HEIGHT);
            windowTexture.Update(windowBuffer);

            windowSprite = new Sprite(windowTexture);

            window.Clear();
            DrawWindow(env);
            window.Display();

            try
            {
                if (AcceleratorDevice.HasGPU)
                {
                    GPU.Init(env);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Init");

            long elapsed_time = 0;
            while (window.IsOpen)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                window.DispatchEvents();

                env.Interact();
                //GPU.Run(env);
                //Console.WriteLine("calculated");
                env.Move();
                //Console.WriteLine("moved");

                window.Clear();
                DrawWindow(env);
                window.Display();

                stopwatch.Stop();
                elapsed_time = stopwatch.ElapsedMilliseconds;
                if (FPS_LIMIT < 0)
                    continue;
                if (elapsed_time < FRAMETIME)
                {
                    Thread.Sleep((int)(FRAMETIME - elapsed_time));
                }
                else if (elapsed_time > 2000.0)
                {
                    Console.WriteLine((double)elapsed_time / 1000.0);
                }
            } 
        }

        static void DrawWindow(Environments env)
        {
            windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
            switch (DrawStyle)
            {
                case 1:
                    DrawParticles1(env);
                    break;
                case 2:
                    DrawParticles2(env);
                    break;
                case 3:
                    DrawParticles3(env);
                    break;
            }
            if (blur)
            {
                blurWindow();
            }
            windowTexture.Update(windowBuffer);
            window.Draw(windowSprite);

            //Console.WriteLine("drawn");
        }
        
        
        
        /*
        static void DrawEnvironment(Environment env)
        {
            DrawParticles1(env);
        }
        */

        static void DrawParticles1(Environments env)
        {
            foreach (GasParticle particle in env.particles)
            {
                /*CircleShape circ = new CircleShape(2);
                circ.Position = new Vector2f((float)(particle.x * WINDOW_WIDTH), (float)(particle.y * WINDOW_HEIGHT));
                circ.FillColor = new Color(0xff, 0xff, 0xff);
                window.Draw(circ);*/
                if (particle.x < 0 || particle.x >= 1.0 || particle.y < 0 || particle.y >= 1.0)
                    continue;

                int x = (int)(particle.x * WINDOW_WIDTH);
                int y = (int)(particle.y * WINDOW_HEIGHT);

                int index = (y * WINDOW_WIDTH + x) * 4;

                windowBuffer[index] = 255;
                windowBuffer[index + 1] = 255;
                windowBuffer[index + 2] = 255;
                windowBuffer[index + 3] = 255;
            }
        }

        static void DrawParticles2(Environments env)
        {
            int resolution_pixel_x = WINDOW_WIDTH / resolution_x;
            int resolution_pixel_y = WINDOW_HEIGHT / resolution_y;

            int visibleAmount = 0;

            if (drawType == 1)
            {
                int colorContrast = 25;

                for (int y = 0; y < resolution_y; y++)
                {
                    for (int x = 0; x < resolution_x; x++)
                    {
                        int squareAmount = 0;
                        foreach (GasParticle particle in env.particles)
                        {
                            if (particle.x < 0 || particle.x >= 1.0 || particle.y < 0 || particle.y >= 1.0)
                                continue;

                            if (particle.x * (double)resolution_x < (double)(x) ||
                                particle.x * (double)resolution_x >= (double)(x + 1) ||
                                particle.y * (double)resolution_y < (double)(y) ||
                                particle.y * (double)resolution_y >= (double)(y + 1))
                                continue;

                            squareAmount += 1;
                            visibleAmount += 1;
                        }

                        int colorshade = (int)(1020 * (squareAmount >= colorContrast ? 1 : ((double)squareAmount / (double)colorContrast)));

                        for (int pixel_y = y * resolution_pixel_y; pixel_y < y * resolution_pixel_y + resolution_pixel_y; pixel_y++)
                        {
                            for (int pixel_x = x * resolution_pixel_x; pixel_x < x * resolution_pixel_x + resolution_pixel_x; pixel_x++)
                            {

                                int index = (pixel_y * WINDOW_WIDTH + pixel_x) * 4;

                                Color color = _1020toRGBscaleColor(colorshade);

                                windowBuffer[index] = color.R;
                                windowBuffer[index + 1] = color.G;
                                windowBuffer[index + 2] = color.B;
                                windowBuffer[index + 3] = 255;
                            }
                        }
                    }
                }
            }

            else if (drawType == 2)
            {
                double colorContrast = 0.003;

                for (int y = 0; y < resolution_y; y++)
                {
                    for (int x = 0; x < resolution_x; x++)
                    {

                        int squareAmount = 0;
                        double averageSpeedSum = 0;
                        foreach (GasParticle particle in env.particles)
                        {
                            if (particle.x < 0 || particle.x >= 1.0 || particle.y < 0 || particle.y >= 1.0)
                                continue;

                            if (particle.x * (double)resolution_x < (double)(x) ||
                                particle.x * (double)resolution_x >= (double)(x + 1) ||
                                particle.y * (double)resolution_y < (double)(y) ||
                                particle.y * (double)resolution_y >= (double)(y + 1))
                                continue;

                            averageSpeedSum += Math.Sqrt(particle.vx * particle.vx + particle.vy * particle.vy);
                            squareAmount += 1;
                            visibleAmount += 1;
                        }

                        int colorshade = squareAmount > 0 ? (int)(1020 * ((double)averageSpeedSum / (double)squareAmount / colorContrast)) : 0;

                        for (int pixel_y = y * resolution_pixel_y; pixel_y < y * resolution_pixel_y + resolution_pixel_y; pixel_y++)
                        {
                            for (int pixel_x = x * resolution_pixel_x; pixel_x < x * resolution_pixel_x + resolution_pixel_x; pixel_x++)
                            {
                                int index = (pixel_y * WINDOW_WIDTH + pixel_x) * 4;

                                Color color = _1020toRGBscaleColor(colorshade);

                                windowBuffer[index] = color.R;
                                windowBuffer[index + 1] = color.G;
                                windowBuffer[index + 2] = color.B;
                                windowBuffer[index + 3] = 255;
                            }
                        }
                    }
                }
            }
        }

        static void DrawParticles3(Environments env)
        {
            //foreach (GasParticle particle in env.particles)
            Parallel.ForEach(env.particles, particle =>
            {
                
                int x = (int)(particle.x * WINDOW_WIDTH);
                int y = (int)(particle.y * WINDOW_HEIGHT);
                double range = (particle.range * WINDOW_HEIGHT) * 0.5;

                for (int pixel_x = -(int)range; pixel_x < (int)range; pixel_x++)
                {
                    for (int pixel_y = -(int)range; pixel_y < (int)range; pixel_y++)
                    {
                        if (x + pixel_x < 0 || x + pixel_x >= WINDOW_WIDTH || y + pixel_y < 0 || y + pixel_y >= WINDOW_HEIGHT)
                            continue;

                        if (Math.Pow(pixel_x, 2) + Math.Pow(pixel_y, 2) > Math.Pow(range, 2))
                            continue;

                        int index = ((y + pixel_y) * WINDOW_WIDTH + (x + pixel_x)) * 4;

                        int _1020color = 5 * (1 + windowBuffer[index] + windowBuffer[index + 1] + windowBuffer[index + 2] - 255);

                        Color colorRGB = _1020toRGBscaleColor(_1020color);

                        windowBuffer[index] = colorRGB.R;
                        windowBuffer[index + 1] = colorRGB.G;
                        windowBuffer[index + 2] = colorRGB.B;
                        windowBuffer[index + 3] = 255;
                        
                    }
                }
            });
        }

        static void blurWindow()
        {
            byte[] newwindowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];

            int blurringsize = (int)Math.Sqrt((double)blurring.Length);

            //for (int y = 0; y < WINDOW_HEIGHT; y++)
            Parallel.For(0, WINDOW_HEIGHT, y =>
            {
                for (int x = 0; x < WINDOW_WIDTH; x++)
                {
                    int blur_sum = 0;

                    for (int blur_y = -blurringsize / 2; blur_y < blurringsize / 2 + 1; blur_y++)
                    {
                        for (int blur_x = -blurringsize / 2; blur_x < blurringsize / 2 + 1; blur_x++)
                        {
                            if ((x + blur_x) < 0 || (x + blur_x) >= WINDOW_WIDTH || (y + blur_y) < 0 || (y + blur_y) >= WINDOW_HEIGHT)
                                continue;
                            int i = ((y + blur_y) * WINDOW_WIDTH + (x + blur_x)) * 4;
                            int _1020 = RGBscaleto1020Color(new Color(windowBuffer[i], windowBuffer[i + 1], windowBuffer[i + 2]));

                            blur_sum += (int)(_1020 * blurring[(blur_y + blurringsize / 2) * blurringsize + (blur_x + blurringsize / 2)]);
                        }
                    }
                    //f[x, y] = exp(-x * x - y * y); (f(-1, -1) + f(0, -1) + f(1, -1) + f(-1, 0) + f(0, 0) + f(1, 0) + f(-1, 1) + f(0, 1) + f(1, 1)) * n = 1

                    int index = (y * WINDOW_WIDTH + x) * 4;

                    Color colorRGB = _1020toRGBscaleColor(blur_sum);

                    newwindowBuffer[index] = colorRGB.R;
                    newwindowBuffer[index + 1] = colorRGB.G;
                    newwindowBuffer[index + 2] = colorRGB.B;
                    newwindowBuffer[index + 3] = 255;
                }
            });

            newwindowBuffer.CopyTo(windowBuffer, 0);
        }



        static Color _1020toRGBscaleColor(int colorshade)
        {
            if (colorshade > 1020)
                return new Color((byte)255, (byte)0, (byte)0);

            int Red = colorshade <= 510 ? 0 : (colorshade > 765 ? 255 : colorshade - 510);
            int Green = colorshade <= 255 ? colorshade : (colorshade < 765 ? 255 : 1020 - colorshade);
            int Blue = colorshade <= 255 ? 255 : (510 > colorshade ? 510 - colorshade : 0);
            return new Color((byte)Red, (byte)Green, (byte)Blue);
        }
        static int RGBscaleto1020Color(Color color)
        {
            int colorshade = 0;
            if (color.B == 255)
            {
                colorshade = color.G;
            }
            else if (color.B < 255 && color.G == 255 && color.B != 0)
            {
                colorshade = 510 - color.B;
            }
            else if (color.R >= 0 && color.G == 255)
            {
                colorshade = 510 + color.R;
            }
            else if (color.R == 255)
            {
                colorshade = 1020 - color.G;
            }
            return colorshade;
        }

        static int roundNextUp(int x, int res)
        {
            if (res % x == 0)
                return x;
            while (res % x != 0)
                x++;
            Console.WriteLine($"rounded up to {x}");
            return x;
        }

        static double[] GaussCurvaMatrix(int size)
        {
            double[] referencematrix = new double[25] {
            0.00296902, 0.0133062, 0.0219382, 0.0133062, .00296902,
            0.0133062, 0.0596343, 0.0983203, 0.0596343, 0.0133062,
            0.0219382, 0.0983203, 0.162103, 0.0983203, 0.0219382,
            0.0133062, 0.0596343, 0.0983203, 0.0596343, 0.0133062,
            0.00296902, 0.0133062, 0.0219382, 0.0133062, 0.00296902};

            double _sum = f(0, 0, size);
            for (int i = 0; i < size / 2 + 1; i++)
            {
                for (int j = i; j < size / 2 + 1; j++)
                {
                    //Console.WriteLine($"{i}, {j}");
                    if (i + j == 0)
                    {
                        continue;
                    }
                    else if ((i == j) || (i == 0 || j == 0))
                    {
                        _sum += 4 * f(i, j, size);
                    }
                    else
                    {
                        _sum += 8 * f(i, j, size);
                    }
                }
            }

            double[] matrix = new double[size * size];
            for (int y = -size / 2; y < size / 2 + 1; y++)
            {
                for (int x = -size / 2; x < size / 2 + 1; x++)
                {
                    //Console.WriteLine($"{((double)(size / 2 + 1) * 0.5)}  a");
                    matrix[(y + size / 2) * size + (x + size / 2)] = realf((double)x, (double)y, _sum, size);
                    //Console.Write($"{Math.Round(matrix[(y + size / 2) * size + (x + size / 2)], 4)}\t");
                }
                //Console.Write("\n");
            }
            Console.WriteLine($"{matrix.Sum()}");

            /*
            double[] referencematrix = new double[25] {
            0.00296902, 0.0133062, 0.0219382, 0.0133062, .00296902,
            0.0133062, 0.0596343, 0.0983203, 0.0596343, 0.0133062,
            0.0219382, 0.0983203, 0.162103, 0.0983203, 0.0219382,
            0.0133062, 0.0596343, 0.0983203, 0.0596343, 0.0133062,
            0.00296902, 0.0133062, 0.0219382, 0.0133062, 0.00296902};
            */

            return matrix;

            static double f(double x, double y, int size)
            {
                return Math.Exp(-(x / ((double)(size / 2 + 1) * 0.5)) * (x / ((double)(size / 2 + 1) * 0.5)) - (y / ((double)(size / 2 + 1) * 0.5)) * (y / ((double)(size / 2 + 1) * 0.5)));
            }
            static double realf(double x, double y, double _sum, int size)
            {
                return f(x, y, size) * 1 / _sum * 10;
            }
        }

        static void OnClose(object sender, EventArgs e)
        {
            // Close the window when OnClose event is received
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }
    }
}

/*
using System.IO;
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Hello, World!");
        for (int i = 0; i < 1021; i++)
        {
            int[] a = _1020toRGBscaleColor(i);
            int b = RGBscaleto1020Color(a);
            int[] c = _1020toRGBscaleColor(b);
            Console.WriteLine($"{c[0]}\t{c[1]}\t{c[2]}");
        }
    }

    static int[] _1020toRGBscaleColor(int colorshade)
    {
        int[] a = new int[3];
        a[2] = colorshade <= 510 ? 0 : (colorshade > 765 ? 255 : colorshade - 510);
        a[1] = colorshade <= 255 ? colorshade : (colorshade < 765 ? 255 : 1020 - colorshade);
        a[0] = colorshade <= 255 ? 255 : (510 > colorshade ? 510 - colorshade : 0);
        return a;
    }

    static int RGBscaleto1020Color(int[] color)
    {
        int colorshade = 0;
        if (color[0] == 255)
        {
            colorshade = color[1];
        }
        else if (color[0] < 255 && color[1] == 255 && color[0] != 0)
        {
            colorshade = 510 - color[0];
        }
        else if (color[2] >= 0 && color[1] == 255)
        {
            colorshade = 510 + color[2];
        }
        else if (color[2] == 255)
        {
            colorshade = 1020 - color[1];
        }
        return colorshade;
    }
}*/