using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cloo;
using fluid_simulation;
using SFML.Window;

namespace abc
{
    class GPU
    {

        private static ComputeContext context;
        private static ComputeProgram program;
        private static ComputeKernel kernel;
        private static ComputeCommandQueue queue;
        private static ComputeEventList eventList;

        private static float[] input_X;
        private static float[] output_Z;

        private static ComputeBuffer<float> a;
        private static ComputeBuffer<float> z;


        public static void Init(Environments env)
        {
            //Console.WriteLine("\nStarted run on GPU");

            int size_X = env.particles.Length;

            string sourceName = @"./Kernel.cl";
            string clProgramSource = File.ReadAllText(sourceName);

            ComputePlatform platform = ComputePlatform.Platforms[0];

            ComputeContextPropertyList properties = new ComputeContextPropertyList(platform);
            context = new ComputeContext(platform.Devices, properties, null, IntPtr.Zero);

            ComputeDevice computer = platform.Devices[0];

            program = new ComputeProgram(context, clProgramSource);
            try
            {
                program.Build(platform.Devices, null, null, IntPtr.Zero);
            }
            catch
            {
                string buildLog = program.GetBuildLog(computer);
                Console.WriteLine($"Build log:\n{buildLog}");
            }

            kernel = program.CreateKernel("Interact");

            queue = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
            eventList = new ComputeEventList();

            input_X = new float[env.particles.Length * 5];
            int i = 0;
            foreach (GasParticle particle in env.particles)
            {
                input_X[i + 0] = (float)particle.x;
                input_X[i + 1] = (float)particle.y;
                input_X[i + 2] = (float)particle.vx;
                input_X[i + 3] = (float)particle.vy;

                i += 5;
            }
            output_Z = new float[env.particles.Length * 2];

            a = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadOnly | ComputeMemoryFlags.CopyHostPointer, input_X);
            z = new ComputeBuffer<float>(context, ComputeMemoryFlags.WriteOnly | ComputeMemoryFlags.CopyHostPointer, output_Z);

            kernel.SetMemoryArgument(0, a);
            kernel.SetValueArgument(1, size_X);
            kernel.SetValueArgument(2, (float)Environments.environmentFriction);
            kernel.SetMemoryArgument(3, z);

            //Console.WriteLine("Stopped run on GPU\n");
        }

        public static void Run(Environments env)
        {
            input_X = new float[env.particles.Length * 5];
            int i = 0;
            foreach (GasParticle particle in env.particles)
            {
                input_X[i + 0] = (float)particle.x;
                input_X[i + 1] = (float)particle.y;
                input_X[i + 2] = (float)particle.vx;
                input_X[i + 3] = (float)particle.vy;
                input_X[i + 3] = (float)particle.range;
                input_X[i + 3] = (float)particle.type;

                i += 5;
            }

            output_Z = new float[env.particles.Length * 2];

            queue.WriteToBuffer<float>(input_X, a, false, eventList);
            queue.WriteToBuffer<float>(output_Z, z, false, eventList);

            //Stopwatch sw1 = new Stopwatch();
            //sw1.Start();

            queue.Execute(kernel, null, new long[] { env.particles.Length, env.particles.Length }, null, eventList);

            queue.ReadFromBuffer(z, ref output_Z, false, eventList);

            //sw1.Stop();
            //Console.Write($"GPUcalc: {sw1.ElapsedMilliseconds}\n");
            queue.Finish();

            int j = 0;
            foreach (GasParticle particle in env.particles)
            {
                particle.vx += (double)output_Z[j];
                particle.vy += (double)output_Z[j + 1];
                j += 2;
            }

            eventList.Clear();
        }
    }
}