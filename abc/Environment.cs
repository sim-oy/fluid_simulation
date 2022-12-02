using System;
using System.Threading.Tasks;

namespace fluid_simulation
{
    class Environments
    {
        public const double environmentFriction = 0.9998;

        public GasParticle[] particles;

        public Environments(int particleAmount)
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
                    particles[y * (int)Math.Sqrt(particleAmount) + x] = new GasParticle(0 + (x / Math.Sqrt(particleAmount) * 0.1) + rng.NextDouble() * 0.00001, 0 + (y / Math.Sqrt(particleAmount) * 0.1) + rng.NextDouble() * 0.00001, 1 / 100.0, 0);
                    //particles[y * (int)Math.Sqrt(particleAmount) + x].vx = rng.NextDouble()*0.01;
                    //particles[y * (int)Math.Sqrt(particleAmount) + x].vy = rng.NextDouble()*0.01;
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
            double momentum = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Move();
                momentum += particles[i].vx * particles[i].vx + particles[i].vy * particles[i].vy;
            }
            //Console.WriteLine($"{momentum}");
        }

        public void Interact()
        {
            Parallel.For(0, particles.Length, i =>
            {
                double sumX = 0, sumY = 0;
                double range = particles[i].range;
                for (int j = 0; j < particles.Length; j++)
                {
                    if (i == j)
                        continue;

                    double distanceX = particles[j].x - particles[i].x;
                    double distanceY = particles[j].y - particles[i].y;

                    if (Math.Abs(distanceX) > range || Math.Abs(distanceY) > range)
                        continue;

                    double x2_y2 = distanceX * distanceX + distanceY * distanceY;

                    if (x2_y2 >= range * range)
                        continue;

                    double dist = Math.Sqrt(x2_y2);
                    double rdist = 1 / (dist + 0.000001);

                    //suuntavektorit
                    double sx = distanceX * rdist;
                    double sy = distanceY * rdist;

                    double f_xy = particles[i].interaction(dist);

                    double timestep = 0.001;

                    sumX += -sx * f_xy * timestep;
                    sumY += -sy * f_xy * timestep;

                    /*
                    if (particles[i].x < 0 || particles[i].x >= 1.0 || particles[i].y < 0 || particles[i].y >= 1.0)
                        continue;
                    */

                    //double particleCollisionFriction = 0.99995;
                    double particleCollisionFriction = 1;
                    particles[i].vx *= particleCollisionFriction;
                    particles[i].vy *= particleCollisionFriction;
                }

                particles[i].vx += sumX;
                particles[i].vy += sumY;

                // gravity
                //particles[i].vy += 0.0001;

                // boundary
                double collisionFriction = 0.5;
                //double collisionFriction = 1;

                if (particles[i].x < 0)
                    particles[i].vx = Math.Abs(particles[i].vx) * collisionFriction;
                else if (particles[i].x > 1.0)
                    particles[i].vx = -Math.Abs(particles[i].vx) * collisionFriction;
                else if (particles[i].y < 0)
                    particles[i].vy = Math.Abs(particles[i].vy) * collisionFriction;
                else if (particles[i].y > 1.0)
                    particles[i].vy = -Math.Abs(particles[i].vy) * collisionFriction;

                particles[i].vx *= environmentFriction;
                particles[i].vy *= environmentFriction;
            });
        }
    }
}