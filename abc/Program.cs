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

            Environment env = new Environment(100);


            window = new RenderWindow(new VideoMode(WINDOW_WIDTH, WINDOW_HEIGHT), "Computational fluid dynamics", Styles.Default);a
            window.Closed += new EventHandler(OnClose);

            windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];

            Texture windowTexture = new Texture(WINDOW_WIDTH, WINDOW_HEIGHT);
            windowTexture.Update(windowBuffer);

            Sprite windowSprite = new Sprite(windowTexture);

            while (window.IsOpen)
            {
                window.DispatchEvents();

                env.Attract();
                env.Move();

                window.Clear();
                //windowBuffer = new byte[WINDOW_WIDTH * WINDOW_HEIGHT * 4];
                DrawEnvironment(env);
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
            int resolution_x = 125;
            int resolution_y = 125;
            for (int y = 0; y < resolution_y; y++)
            {
                for (int x = 0; x < resolution_x; x++)
                {
                    RectangleShape square = new RectangleShape();
                    square.Position = new Vector2f(((int)(x * (WINDOW_WIDTH / resolution_x)), ((int)(y *(WINDOW_HEIGHT / resolution_y)))));
                    square.Size = new Vector2f(WINDOW_WIDTH / resolution_x, WINDOW_HEIGHT / resolution_y);
                    square.FillColor = new Color(0xff, 0xff, 0xff);
                    window.Draw(square);Convert.
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
