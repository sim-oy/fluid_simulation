﻿using System;
using System.Threading.Tasks;

namespace fluid_simulation
{
    class Environment
    {
        //public int boundary_len = Program.WINDOW_WIDTH * 2 + Program.WINDOW_HEIGHT * 2 + 4;
        public int boundary_len = 0;

        public GasParticle[] particles;
        const int Walldensity = 20;

        public Environment(int particleAmount)
        {

            //particles = new GasParticle[particleAmount + Program.WINDOW_WIDTH * Walldensity * 2 + Program.WINDOW_HEIGHT * Walldensity * 2 + 4];
            particles = new GasParticle[particleAmount];

            Random rng = new Random();

            int i = 0;
            //particles
            for (; i < particleAmount; i++)
            {
                particles[i] = new GasParticle(rng.NextDouble() * 0.1, rng.NextDouble() * 0.1, 30);
            }

            //MakeBoundary(i);

            //particles[particleAmount] = new GasParticle(0, 0, 0.0000001);
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
            Parallel.For(0, particles.Length - boundary_len, i =>
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

                //boundary
                if (particles[i].x < 0)
                    particles[i].vx = Math.Abs(particles[i].vx);
                else if (particles[i].x > 1.0)
                    particles[i].vx = -Math.Abs(particles[i].vx);
                else if (particles[i].y < 0)
                    particles[i].vy = Math.Abs(particles[i].vy);
                else if (particles[i].y > 1.0)
                    particles[i].vy = -Math.Abs(particles[i].vy);
            });
        }

        public void MakeBoundary(int i)
        {
            double x_pixel = 1 / Convert.ToDouble(Program.WINDOW_WIDTH * Walldensity);
            double y_pixel = 1 / Convert.ToDouble(Program.WINDOW_HEIGHT * Walldensity);

            double xstart1 = 0 - x_pixel;
            double ystart1 = 1 + y_pixel;
            double xstart2 = 0 - x_pixel;
            double ystart2 = 0 - y_pixel;

            int k = i;
            for (int j = 0; i < (Program.WINDOW_WIDTH * Walldensity + 2) * 2 + k; i++)
            {
                particles[i] = new GasParticle(xstart1 + x_pixel * j, ystart1, 0.01);
                i++;
                particles[i] = new GasParticle(xstart2 + x_pixel * j, ystart2, 0.01);
                j++;
            }
            xstart1 = 0 - x_pixel;
            ystart1 = 0;
            xstart2 = 1 + x_pixel;
            ystart2 = 0;

            k = i;
            for (int j = 0; i < Program.WINDOW_HEIGHT * Walldensity * 2 + k; i++)
            {
                particles[i] = new GasParticle(xstart1, ystart1 + y_pixel * j, 0.01);
                i++;
                particles[i] = new GasParticle(xstart2, ystart2 + y_pixel * j, 0.01);
                j++;
            }
        }
    }
}