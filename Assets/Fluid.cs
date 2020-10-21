using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid
{

    public int size; //size
    public float dt;

    public float diff; // diffusion
    public float visc; // Viscosity


    public float[,] density;
    public float[,] s; // Density0

    public Vector2[,] velocity;
    public Vector2[,] velocity0;


    public Fluid(float diffusion, float viscosity, int size) {

        this.size = size;

        diff = diffusion;
        visc = viscosity;

        s = new float[size, size];
        density = new float[size, size];

        velocity = new Vector2[size, size];
        velocity0 = new Vector2[size, size];
    }

    // Add density to XY (Dye)
    public void AddDensity(int x, int y, float amount) {// Change to vector2Int ?`?
        density[x, y] += amount;
    }
    // Add Velocity on XY (vector2 velocity)
    public void AddVelocity(int x, int y, Vector2 amount) {
        velocity[x, y] += amount;
    }


    void Diffuse(Vector2[,] v, Vector2[,] v0, float dt, int iter) {
        float a = dt * diff * (size - 2) * (size - 2);

        lin_solve(v, v0, a, 1 + 6 * a, iter);
    }

    // Change to gauss seidel 
    //Double check later
    // vec to vec
    void lin_solve(Vector2[,] v, Vector2[,] v0, float a, float c, int iter) {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++) {
            for (int j = 1; j < size - 1; j++) {
                for (int i = 1; i < size - 1; i++) {
                    v[i, j] =
                        (v0[i, j]
                            + a * (v[i + 1, j]
                                    + v[i - 1, j]
                                    + v[i, j + 1]
                                    + v[i, j - 1]
                                    + v[i, j]
                                    + v[i, j]
                            )) * cRecip;
                }
            }
            set_bnd(true, v);
        }
    }
    // X -> y only linsolve for x -> y?
    // wrong
    void lin_solve(Vector2[,] v, float a, float c, int iter) {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++) {
            for (int j = 1; j < size - 1; j++) {
                for (int i = 1; i < size - 1; i++) {
                    v[i, j].x =
                        (v[i, j].y
                            + a * (v[i + 1, j].x
                                    + v[i - 1, j].x
                                    + v[i, j + 1].x
                                    + v[i, j - 1].x
                                    + v[i, j].x
                                    + v[i, j].x
                            )) * cRecip;
                }
            }
            set_bnd(false, v);
        }
    }

    // p = x /// div = y//
    void Project(Vector2[,] velocity0, Vector2[,] velocity, int iter) {
        for (int j = 1; j < size - 1; j++) {
            for (int i = 1; i < size - 1; i++) {
                velocity[i, j].y = -0.5f * (
                         velocity0[i + 1, j].x
                        - velocity0[i - 1, j].x
                        + velocity0[i, j + 1].y
                        - velocity0[i, j - 1].y
                    ) / size;
                velocity[i, j].x = 0;
            }
        }
        set_bnd(false, velocity);
        // Magic !!
        lin_solve(velocity, 1, 6, iter, size);

        for (int j = 1; j < size - 1; j++) {
            for (int i = 1; i < size - 1; i++) {
                velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)]
                                                - p[IX(i - 1, j)]) * size;
                velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)]
                                                - p[IX(i, j - 1)]) * size;
            }
        }
        set_bnd(1, velocX, size);
        set_bnd(2, velocY, size);
    }


    //project(Vx0, Vy0, Vx, Vy, 4, N);



    //  Possible rewrite big
    void set_bnd(bool b, Vector2[,] x) {

        if (b) {

            //set to equal or -equal of the neighbor
            for (int i = 1; i < size - 1; i++) {
                x[i, 0] = new Vector2(x[i, 1].x, -x[i, 1].y); // optional    x[i, 0] =  x[i, 1] * -Vector2.Up; To negate Y?
                x[i, size - 1] = new Vector2(x[i, size - 2].x, -x[i, size - 2].y);
            }

            for (int j = 1; j < size - 1; j++) {
                x[0, j] = new Vector2(-x[1, j].x, x[1, j].y);
                x[size - 1, j] = new Vector2(-x[size - 2, j].x, x[size - 2, j].y);
            }
        }
        else {

            // If b== 0 then set borders equal to their neighbors
            for (int i = 1; i < size - 1; i++) {
                x[i, 0] = x[i, 1];
                x[i, size - 1] = x[i, size - 2];
            }
            for (int j = 1; j < size - 1; j++) {
                x[0, j] = x[1, j];
                x[size - 1, j] = x[size - 2, j];
            }
        }

        // Corners
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, size - 1] = 0.5f * (x[1, size - 1] + x[0, size - 2]);

        x[size - 1, 0] = 0.5f * (x[size - 2, 0] + x[size - 1, 1]);
        x[size - 1, size - 1] = 0.5f * (x[size - 2, size - 1] + x[size - 1, size - 2]);
    }

    void set_bndV2(int b, Vector2[,] x) {

        // If b== 0 then set borders equal to their neighbors
        for (int i = 1; i < size - 1; i++) {
            x[i, 0] = x[i, 1];
            x[i, size - 1] = x[i, size - 2];
        }
        for (int j = 1; j < size - 1; j++) {
            x[0, j] = x[1, j];
            x[size - 1, j] = x[size - 2, j];
        }

        if (b == 1) {

            for (int i = 1; i < size - 1; i++) { // Top and bottom wall
                x[i, 0].y = -x[i, 0].y;
                x[i, size - 1].y = -x[i, size - 1].y;
            }
            for (int j = 1; j < size - 1; j++) {// Right and Left wall
                x[0, j].x = -x[0, j].x;
                x[size - 1, j].x = -x[size - 1, j].x;
            }
        }

        // Corners
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, size - 1] = 0.5f * (x[1, size - 1] + x[0, size - 2]);

        x[size - 1, 0] = 0.5f * (x[size - 2, 0] + x[size - 1, 1]);
        x[size - 1, size - 1] = 0.5f * (x[size - 2, size - 1] + x[size - 1, size - 2]);
    }


    void FluidStep(MikeAsh_Fluid fluid) {


        // Diffuse velocites 
        Diffuse(velocity0, velocity, dt, 4);
        //diffuse(velocity0, velocity, dt, iterations)

        //Clean up to make sure density is the same?
        Project(velocity0, velocity, 4);

        // Advect on the velocityes
        advect(1, Vx, Vx0, Vx0, Vy0, dt, N);
        advect(2, Vy, Vy0, Vx0, Vy0, dt, N);

        //Clean upp again
        project(Vx, Vy, Vx0, Vy0, 4, N);

        // Diffuse density(dye) and advect it
        diffuse(0, s, density, diff, dt, 4, N);
        advect(0, density, s, Vx, Vy, dt, N);
    }

}
