using System;
using System.Threading.Tasks;

namespace fluid_simulation
{
    class Environment
    {
        private const double environmentFriction = 0.9999;

        public GasParticle[] particles;

        public Environment(int particleAmount)
        {

            //particles = new GasParticle[particleAmount + Program.WINDOW_WIDTH * Walldensity * 2 + Program.WINDOW_HEIGHT * Walldensity * 2 + 4];
            particles = new GasParticle[particleAmount];

            Random rng = new Random();

            //particles

            /*
            for (int y = 0; y < Math.Sqrt(particleAmount); y++)
            {
                for (int x = 0; x < Math.Sqrt(particleAmount); x++)
                {
                    particles[y * (int)Math.Sqrt(particleAmount) + x] = new GasParticle(0.3 + (x / Math.Sqrt(particleAmount) * 0.1) + rng.NextDouble() * 0.00001, 0.3 + (y / Math.Sqrt(particleAmount) * 0.1) + rng.NextDouble() * 0.00001, 100);
                }
            }
            */

            for (int y = 0; y < Math.Sqrt(particleAmount); y++)
            {
                for (int x = 0; x < Math.Sqrt(particleAmount); x++)
                {
                    particles[y * (int)Math.Sqrt(particleAmount) + x] = new GasParticle(0 + (x / Math.Sqrt(particleAmount) * 1) + rng.NextDouble() * 0.00001, 0 + (y / Math.Sqrt(particleAmount) * 1) + rng.NextDouble() * 0.00001, 200);
                }
            }

            /*
            for (int i = 0; i < particleAmount; i++)
            {
                particles[i] = new GasParticle(rng.NextDouble(), rng.NextDouble(), 200);
            }
            */
        }

        public void Move()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Move();
            }
            //Console.WriteLine(particles[1200].x);
            //Console.WriteLine(particles[1200].y);
        }

        public void Attract()
        {
            Parallel.For(0, particles.Length, i =>
            {
                double sumX = 0, sumY = 0;
                for (int j = 0; j < particles.Length; j++)
                {
                    if (i == j)
                        continue;

                    double distanceX = particles[j].x - particles[i].x;
                    double distanceY = particles[j].y - particles[i].y;

                    double x2_y2 = distanceX * distanceX + distanceY * distanceY;

                    if (x2_y2 >= (1 / particles[i].range) * (1 / particles[i].range))
                        continue;

                    double dist = Math.Pow(x2_y2, 0.5);

                    //suuntavektorit
                    double sx = distanceX / dist;
                    double sy = distanceY / dist;

                    double f_xy = particles[i].interaction(dist);

                    double multiplier = 0.0001;

                    sumX += -sx * f_xy * multiplier;
                    sumY += -sy * f_xy * multiplier;

                    if (particles[i].x < 0 || particles[i].x >= 1.0 || particles[i].y < 0 || particles[i].y >= 1.0)
                        continue;

                }

                particles[i].vx += sumX;
                particles[i].vy += sumY;

                particles[i].vy += 0.00001;

                //boundary
                if (particles[i].x < 0)
                    particles[i].vx = Math.Abs(particles[i].vx);
                else if (particles[i].x > 1.0)
                    particles[i].vx = -Math.Abs(particles[i].vx);
                else if (particles[i].y < 0)
                    particles[i].vy = Math.Abs(particles[i].vy);
                else if (particles[i].y > 1.0)
                    particles[i].vy = -Math.Abs(particles[i].vy);

                particles[i].vx *= environmentFriction;
                particles[i].vy *= environmentFriction;
            });
        }
    }
}