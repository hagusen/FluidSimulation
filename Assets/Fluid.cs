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

        s = new float[size,size];
        density = new float[size, size];

        velocity = new Vector2[size, size]; 
        velocity0 = new Vector2[size, size];
    }

    // Add density to XY (Dye)
    public void AddDensity(int x, int y, float amount) {// Change to vector2Int ?`?
        density[x,y] += amount;
    }
    // Add Velocity on XY (vector2 velocity)
    public void AddVelocity(int x, int y, Vector2 amount) {
        velocity[x, y] += amount;
    }


    void diffuse(int b, Vector2[,] v, Vector2[,] v0, float dt, int iter) {
        float a = dt * diff * (size - 2) * (size - 2);

        lin_solve(b, v, v0, a, 1 + 6 * a, iter);
    }

    // Change to gauss seidel 
    //Double check later
    void lin_solve(int b, Vector2[,] v, Vector2[,] v0, float a, float c, int iter) {
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
            //set_bnd(b, v, size);
        }
    }
    void lin_solve(int b, float[] v, float[,] v0, float a, float c, int iter) {
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
            //set_bnd(b, v, size);
        }
    }


    void project(float[] velocX0, float[] velocY0, float[] p, float[] div, int iter, int N) {
        for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                div[IX(i, j)] = -0.5f * (
                         velocX0[IX(i + 1, j)]
                        - velocX0[IX(i - 1, j)]
                        + velocY0[IX(i, j + 1)]
                        - velocY0[IX(i, j - 1)]
                    ) / N;
                p[IX(i, j)] = 0;
            }
        }
        set_bnd(0, div, N);
        set_bnd(0, p, N);
        lin_solve(0, p, div, 1, 6, iter, N); // Wat..??

        for (int j = 1; j < N - 1; j++) {
            for (int i = 1; i < N - 1; i++) {
                velocX0[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)]
                                                - p[IX(i - 1, j)]) * N;
                velocY0[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)]
                                                - p[IX(i, j - 1)]) * N;
            }
        }
        set_bnd(1, velocX0, N);
        set_bnd(2, velocY0, N);
    }


    project(Vx0, Vy0, Vx, Vy, 4, N);



    //  Possible rewrite big
    void set_bnd(int b, float[] x, int N) {

        for (int i = 1; i < N - 1; i++) {
            x[IX(i, 0)] = b == 2 ? -x[IX(i, 1)] : x[IX(i, 1)];
            x[IX(i, N - 1)] = b == 2 ? -x[IX(i, N - 2)] : x[IX(i, N - 2)];
        }

        for (int j = 1; j < N - 1; j++) {
            x[IX(0, j)] = b == 1 ? -x[IX(1, j)] : x[IX(1, j)];
            x[IX(N - 1, j)] = b == 1 ? -x[IX(N - 2, j)] : x[IX(N - 2, j)];
        }


        x[IX(0, 0)] = 0.5f * (x[IX(1, 0)] + x[IX(0, 1)]);
        x[IX(0, N - 1)] = 0.5f * (x[IX(1, N - 1)] + x[IX(0, N - 2)]);
        x[IX(N - 1, 0)] = 0.5f * (x[IX(N - 2, 0)] + x[IX(N - 1, 1)]);
        x[IX(N - 1, N - 1)] = 0.5f * (x[IX(N - 2, N - 1)] + x[IX(N - 1, N - 2)]);



    }



}
