using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluid
{

    public int N; //size
    public float dt;

    public float diff; // diffusion
    public float visc; // Viscosity


    //public float[] s;          // Density0
    //public float[] density;
    public float[,] density;
    public float[,] s; // Density0
    //public Vector2[,] density;

    public float[] Vx;         //Velocity
    public float[] Vy;
    public Vector2[,] velocity;

    public float[] Vx0;        //Velocity0
    public float[] Vy0;
    public Vector2[,] velocity0;


}
