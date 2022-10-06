using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using Color = SFML.Graphics.Color;

namespace fluid_simulation
{
    class Program
    {
        public const int WINDOW_WIDTH = 500;
        public const int WINDOW_HEIGHT = 500;

        private static RenderWindow window;
        private static byte[] windowBuffer;

        static void Main(string[] args)
        {
            //Console.WriteLine(x_pixel * 1920);
            //Console.WriteLine(y_pixel * 1080);

            Environment env = new Environment(1000);


            window = new RenderWindow(new VideoMode(WINDOW_WIDTH, WINDOW_HEIGHT), "Computational fluid dynamics", Styles.Default);
            window.Closed += new EventHandler(OnClose);

            windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];

            Texture windowTexture = new Texture(WINDOW_WIDTH, WINDOW_HEIGHT);
            windowTexture.Update(windowBuffer);

            Sprite windowSprite = new Sprite(windowTexture);

            Console.WriteLine("init complete");

            while (window.IsOpen)
            {
                window.DispatchEvents();

                env.Attract();
                Console.WriteLine("calculated");
                env.Move();
                Console.WriteLine("moved");

                window.Clear();
                //windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
                DrawEnvironment(env);
                Console.WriteLine("drawn");
                //windowTexture.Update(windowBuffer);
                window.Draw(windowSprite);
                window.Display();
            }
        }

        static void DrawEnvironment(Environment env)
        {
            DrawParticles2(env);
        }

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
            int resolution_x = 100;
            int resolution_y = 100;

            int resolution_pixel_x = WINDOW_WIDTH / resolution_x;
            int resolution_pixel_y = WINDOW_HEIGHT / resolution_y;

            for (int y = 0; y < resolution_y; y++)
            {
                for (int x = 0; x < resolution_x; x++)
                {
                    RectangleShape square = new RectangleShape();
                    square.Position = new Vector2f(x * resolution_pixel_x, y * resolution_pixel_y);
                    square.Size = new Vector2f(resolution_pixel_x, resolution_pixel_y);

                    int squareAmount = 0;
                    foreach (GasParticle particle in env.particles)
                    {
                        if (particle.x < 0 || particle.x >= 1.0 || particle.y < 0 || particle.y >= 1.0)
                            continue;

                        if (particle.x * (double)resolution_x < (double)(x * resolution_pixel_x) ||
                            particle.x * (double)resolution_x >= (double)(x * resolution_pixel_x + resolution_pixel_x) ||
                            particle.y * (double)resolution_y < (double)(y * resolution_pixel_y) ||
                            particle.y * (double)resolution_y >= (double)(y * resolution_pixel_y + resolution_pixel_y))
                            continue;
                        squareAmount += 1;
                    }

                    byte colorshade = (byte)(255 * (squareAmount >= 50 ? 1 : ((double)squareAmount / (double)50)));
                    square.FillColor = new Color(colorshade, colorshade, colorshade);
                    window.Draw(square);
                }
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
