using System;

namespace fluid_simulation
{
    class GasParticle
    {
        public double x;
        public double y;
        public double vx;
        public double vy;
        public double range;
        public int type;

        public GasParticle(double x, double y, double range, int type)
        {
            this.x = x;
            this.y = y;
            this.range = range;
            this.type = type;
         }

        public void Move()
        {
            this.x += vx;
            this.y += vy;
        }

        public double interaction(double dist)
        {
            return range * (Math.Cos((1 / range) * Math.PI * dist) + 1);
        }
    }
}
