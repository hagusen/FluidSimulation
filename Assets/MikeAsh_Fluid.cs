using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MikeAsh_Fluid
{
    public int N;
    public float dt;
    public float diff; // diffusion
    public float visc; // Viscosity


    public float[] s;          // Density0
    public float[] density;

    public float[] Vx;         //Velocity
    public float[] Vy;

    public float[] Vx0;        //Velocity0
    public float[] Vy0;

    public MikeAsh_Fluid(float dt, float diffusion, float viscosity, int N) {

        this.N = N;

        this.dt = dt;
        diff = diffusion; 
        visc = viscosity; 

        s = new float[N*N];        
        density = new float[N * N];

        Vx = new float[N * N];        
        Vy = new float[N * N];

        Vx0 = new float[N * N];        
        Vy0 = new float[N * N];
    }
    // Add density to XY (Dye)
    public void AddDensity(int x, int y, float amount) {
        int index = IX(x, y);
        density[index] += amount;
    }
    // Add Velocity on XY (vector2 velocity)
    public void AddVelocity(int x, int y, float amountX, float amountY) {
        int index = IX(x, y);
        Vx[index] += amountX;
        Vy[index] += amountY;
    }





    //2D to 1D array
    int IX(int x, int y) {
        return Mathf.Clamp(x, 0, N - 1) + Mathf.Clamp(y, 0, N - 1) * N;
    }


}

