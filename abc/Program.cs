using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using Color = SFML.Graphics.Color;
using System.Diagnostics;

namespace fluid_simulation
{
    class Program
    {
        public const int WINDOW_WIDTH = 500;
        public const int WINDOW_HEIGHT = 500;

        // 1: draw all particles
        // 2: draw attribute in resolution
        private const int DrawStyle = 3;

        // If DrawStyle = 2
        // 1: particles in pixel
        // 2: average velocity in pixel
        private const int drawType = 2;


        public const int FPS_LIMIT = 200;
        public static long FRAMETIME = 1000 / FPS_LIMIT;

        private static RenderWindow window;
        private static byte[] windowBuffer;

        private static Texture windowTexture;
        private static Sprite windowSprite;

        static void Main()
        {
            Environment env = new Environment((int)Math.Pow(50, 2));

            window = new RenderWindow(new VideoMode(WINDOW_WIDTH, WINDOW_HEIGHT), "Computational fluid dynamics", Styles.Default);
            window.Closed += new EventHandler(OnClose);

            windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
            
            windowTexture = new Texture(WINDOW_WIDTH, WINDOW_HEIGHT);
            windowTexture.Update(windowBuffer);

            windowSprite = new Sprite(windowTexture);

            Console.WriteLine("init complete");

            long elapsed_time = 0;
            while (window.IsOpen)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                window.DispatchEvents();

                env.Attract();
                Console.WriteLine("calculated");
                env.Move();
                Console.WriteLine("moved");

                window.Clear();
                DrawWindow(env);
                window.Display();

                stopwatch.Stop();
                elapsed_time = stopwatch.ElapsedMilliseconds;

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

        static void DrawWindow(Environment env)
        {
            if (DrawStyle == 1)
            {
                windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
                DrawParticles1(env);
                windowTexture.Update(windowBuffer);
                window.Draw(windowSprite);
            }
            else if (DrawStyle == 2)
            {
                DrawParticles2(env);
            }
            else if (DrawStyle == 3)
            {
                windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
                DrawParticles3(env);
                windowTexture.Update(windowBuffer);
                window.Draw(windowSprite);
            }

            Console.WriteLine("drawn");
        }
        
        
        
        /*
        static void DrawEnvironment(Environment env)
        {
            DrawParticles1(env);
        }
        */

        static void DrawParticles1(Environment env)
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

        static void DrawParticles2(Environment env)
        {
            int resolution_x = 40;
            int resolution_y = 40;

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
                        RectangleShape square = new RectangleShape(new Vector2f(resolution_pixel_x, resolution_pixel_y));
                        square.Position = new Vector2f(x * resolution_pixel_x, y * resolution_pixel_y);
                        //square.setSize = new Vector2f(resolution_pixel_x, resolution_pixel_y);

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

                        square.FillColor = _1020toRGBscaleColor(colorshade);
                        window.Draw(square);
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


                        RectangleShape square = new RectangleShape(new Vector2f(resolution_pixel_x, resolution_pixel_y));
                        square.Position = new Vector2f(x * resolution_pixel_x, y * resolution_pixel_y);
                        //square.setSize = new Vector2f(resolution_pixel_x, resolution_pixel_y);

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

                        
                        /*if (squareAmount > 0)
                            Console.WriteLine((double)averageSpeedSum / (double)squareAmount);*/

                        int colorshade = squareAmount > 0 ? (int)(1020 * ((double)averageSpeedSum / (double)squareAmount / colorContrast)) : 0;

                        square.FillColor = _1020toRGBscaleColor(colorshade);
                        window.Draw(square);
                    }
                }
            }
        }

        static void DrawParticles3(Environment env)
        {
            /*
            //for (int pixel_x = 0; pixel_x < WINDOW_WIDTH; pixel_x++)
            Parallel.For(0, WINDOW_WIDTH, pixel_x =>
            {
                for (int pixel_y = 0; pixel_y < WINDOW_HEIGHT; pixel_y++)
                {
                    foreach (GasParticle particle in env.particles)
                    {
                        int x = (int)(particle.x * WINDOW_WIDTH);
                        int y = (int)(particle.y * WINDOW_HEIGHT);
                        double range = (1 / particle.range * WINDOW_HEIGHT);

                        if (x < 0 || x >= WINDOW_WIDTH || y < 0 || y >= WINDOW_HEIGHT)
                            continue;

                        if (Math.Pow(x - pixel_x, 2) + Math.Pow(y - pixel_y, 2) > Math.Pow(range, 2))
                            continue;

                        int index = (pixel_y * WINDOW_WIDTH + pixel_x) * 4;

                        windowBuffer[index] = 255;
                        windowBuffer[index + 1] = 0;
                        windowBuffer[index + 2] = 0;
                        windowBuffer[index + 3] = 255;
                    }
                }
            });*/


            foreach (GasParticle particle in env.particles)
            {
                int x = (int)(particle.x * WINDOW_WIDTH);
                int y = (int)(particle.y * WINDOW_HEIGHT);
                double range = (1 / particle.range * WINDOW_HEIGHT);

                for (int pixel_x = -(int)range; pixel_x < (int)range; pixel_x++)
                {
                    for (int pixel_y = -(int)range; pixel_y < (int)range; pixel_y++)
                    {
                        if (x + pixel_x < 0 || x + pixel_x >= WINDOW_WIDTH || y + pixel_y < 0 || y + pixel_y >= WINDOW_HEIGHT)
                            continue;

                        if (Math.Pow(pixel_x - x, 2) + Math.Pow(pixel_y - y, 2) > Math.Pow(range, 2))
                            continue;

                        int index = ((y + pixel_y) * WINDOW_WIDTH + (x + pixel_x)) * 4;

                        windowBuffer[index] = 255;
                        windowBuffer[index + 1] = 0;
                        windowBuffer[index + 2] = 0;
                        windowBuffer[index + 3] = 255;
                    }
                }
            }

        }




        static Color _1020toRGBscaleColor(int colorshade)
        {
            int Red = colorshade <= 510 ? 0 : (colorshade >= 765 ? 255 : colorshade - 510);
            int Green = colorshade <= 255 ? colorshade : (colorshade < 765 ? 255 : 255 - (colorshade - 765));
            int Blue = colorshade <= 255 ? 255 : (255 - (colorshade - 255) < 0 ? 0 : 255 - (colorshade - 255));
            return new Color((byte)Red, (byte)Green, (byte)Blue);
        }

        static void OnClose(object sender, EventArgs e)
        {
            // Close the window when OnClose event is received
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }
    }
}
    