
float f1(float dist, float range) {
    return 1 / range * (cos(range * M_PI * dist) + 1);
}

kernel void Interact(global float* input_X, int size_X, float environmentFriction, global float* output_Z)
{

    int i = get_global_id(0);

    //printf("o: %d\t %d\n", i, j);

    //printf(""o: %0.20f\t%0.20f\n"", input_X[i * 2], input_X[i * 2 + 1]);
    
    float xi = input_X[i * 5];
    float yi = input_X[i * 5 + 1];
    float range = input_X[i * 5 + 4];

    float sumX = 0, sumY = 0;

    for (int j = 0; j < size_X; j++)
    {
        if (i == j)
            continue;

        float distanceX = input_X[j * 5] - xi;
        float distanceY = input_X[j * 5 + 1] - yi;
        
        if (abs(distanceX) > range || abs(distanceY) > range)
            continue;

        float x2_y2 = distanceX * distanceX + distanceY * distanceY;

        if (x2_y2 >= range * range)
            continue;

        float dist = sqrt(x2_y2);

        //suuntavektorit
        float sx = distanceX / dist;
        float sy = distanceY / dist;

        double f_xy = f1(dist, range);

        float timestep = 0.001;

        sumX += -sx * f_xy * timestep;
        sumY += -sy * f_xy * timestep;

        /*
        //double particleCollisionFriction = 0.998;
        double particleCollisionFriction = 1;
        particles[i].vx *= particleCollisionFriction;
        particles[i].vy *= particleCollisionFriction;*/
    }

    output_Z[i * 2] += input_X[i * 5 + 2] + sumX;
    output_Z[i * 2 + 1] += input_X[i * 5 + 3] + sumY;

    // gravity
    //particles[i].vy += 0.0001;

    // boundary
    //double collisionFriction = 0.5;
    float collisionFriction = 1;

    if (xi < 0)
        output_Z[i * 2] = abs(xi) * collisionFriction;
    else if (xi > 1.0)
        output_Z[i * 2] = -abs(xi) * collisionFriction;
    else if (yi < 0)
        output_Z[i * 2 + 1] = abs(yi) * collisionFriction;
    else if (yi > 1.0)
        output_Z[i * 2 + 1] = -abs(yi) * collisionFriction;

    output_Z[i * 2] *= environmentFriction;
    output_Z[i * 2 + 1] *= environmentFriction;
    

    //printf(""%lf\n"", (double)input_X[index]);
    //printf(""%d\n"", i);
    //printf(""%d\n"", size_X);
    /*
    if ((float)i == (float)(size_X - 1))
    {
        #define fmt ""%s\n""
        printf(""%d\n"", i);
        printf(""G: %0.10f\n"", G);
        //output_Z[i] = 6.4;
        for (int i = 0; i < size_X * 2; i++)
        {
            printf(""%0.38f\n"", output_Z[i]);
        }
    }*/
}
//http://www.iterationzero.co.uk/?p=44
//https://registry.khronos.org/OpenCL/sdk/1.2/docs/man/xhtml/clEnqueueNDRangeKernel.html
//https://cctags.github.io/posts/2015/11/17/global_and_local_work_size.html