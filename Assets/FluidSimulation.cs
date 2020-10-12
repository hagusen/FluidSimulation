using Sirenix.OdinInspector.Editor.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    public int N = 16;
    public int iter = 3;
    Fluid fluid;
    public FilterMode filtermode = FilterMode.Point;
    public Texture2D tex;

    //2D to 1D array
    int IX(int x, int y) {
        return Mathf.Clamp(x, 0, N-1) + Mathf.Clamp(y, 0, N-1) * N;
    }

    void Start()
    {
        tex = new Texture2D(N, N, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = filtermode;

        fluid = new Fluid(0.1f, 0.01f, 0.0002f, N);

        RenderFluid();

        fluid.AddDensity(2, 3, 100);
        fluid.AddVelocity(2, 3, 2, 4);



    }
    void RenderFluid() {



        var stepsize = 1f / N;//
        for (int y = 0; y < N; y++) {
            for (int x = 0; x < N; x++) {
                //texture.SetPixel(x, y, Color.red);
                //tex.SetPixel(x, y, new Color(fluid.density[IX(x,y)],0f , 0f));
                //tex.SetPixel(x, y, new Color(fluid.Vx[IX(x, y)], fluid.Vy[IX(x, y)], fluid.density[IX(x, y)]));
                tex.SetPixel(x, y, new Color(/*(Vector2. right * fluid.Vx[IX(x, y)] +  Vector2.up * fluid.Vy[IX(x, y)]).magnitude*/0, Mathf.Abs(fluid.Vx[IX(x, y)]), Mathf.Abs(fluid.Vy[IX(x, y)])));
            }

            tex.SetPixel(13, y, Color.blue);
        }

        // Apply all SetPixel calls
        tex.Apply();


        GetComponent<Renderer>().material.mainTexture = tex;
    }

    Vector3 mouse0 = Vector2.zero;
    // Update is called once per frame
    void Update()
    {

        var s = Input.mousePosition - mouse0;
        s.Normalize();
        s *= 10;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            Vector2Int v  = Vector2Int.RoundToInt(hit.textureCoord * N);
            if (Input.GetKey(KeyCode.Mouse0)) {
                fluid.AddDensity(v.x, v.y, 100);
                fluid.AddVelocity(v.x, v.y,s.x , s.y);

            }
        }
        var x = Mathf.Sin(Time.time) *40 ;
        var y = Mathf.Cos(Time.time) *40;
        //fluid.AddDensity(N / 2, N / 2, 10);
        //fluid.AddVelocity(N / 2, N / 2, x, y );

        fluid.AddVelocity((int)(N / 1.5), N / 2, -40 *4, 0);
        fluid.AddVelocity((int)(N / 3), N / 2, 40*4, 0);






        //fluid.AddVelocity(N / 4, N / 4, x *-1, y * -1);


        if (Input.GetKeyDown(KeyCode.Space)) {
            fluid.AddDensity(2, 3, 100);
            fluid.AddVelocity(2, 3, 2, 4);
        }

        tex.filterMode = filtermode;




        FluidStep(fluid);
        RenderFluid();



        mouse0 = Input.mousePosition;
    }


    void FluidStep(Fluid fluid) {
        int N = fluid.N;
        float visc = fluid.visc;
        float diff = fluid.diff;
        //float dt = fluid.dt;
        float dt = Time.deltaTime;
        float[] Vx = fluid.Vx;
        float[] Vy = fluid.Vy;
        float[] Vx0 = fluid.Vx0;
        float[] Vy0 = fluid.Vy0;
        float[] s = fluid.s;
        float[] density = fluid.density;

        // Diffuse velocites 
        diffuse(1, Vx0, Vx, visc, dt, 4, N);
        diffuse(2, Vy0, Vy, visc, dt, 4, N);

        //Clean up to make sure density is the same?
        project(Vx0, Vy0, Vx, Vy, 4, N);

        // Advect on the velocityes
        advect(1, Vx, Vx0, Vx0, Vy0, dt, N);
        advect(2, Vy, Vy0, Vx0, Vy0, dt, N);

        //Clean upp again
        project(Vx, Vy, Vx0, Vy0, 4, N);

        // Diffuse density(dye) and advect it
        diffuse(0, s, density, diff, dt, 4, N);
        advect(0, density, s, Vx, Vy, dt, N);
    }

    void diffuse(int b, float[] x, float[] x0, float diff, float dt, int iter, int N) {
        float a = dt * diff * (N - 2) * (N - 2);
        lin_solve(b, x, x0, a, 1 + 6 * a, iter, N);
    }

    void lin_solve(int b, float[] x, float[] x0, float a, float c, int iter, int N) {
        float cRecip = 1.0f / c;
        for (int k = 0; k < iter; k++) {
            for (int j = 1; j < N - 1; j++) {
                for (int i = 1; i < N - 1; i++) {
                    x[IX(i, j)] =
                        (x0[IX(i, j)]
                            + a * (x[IX(i + 1, j)]
                                    + x[IX(i - 1, j)]
                                    + x[IX(i, j + 1)]
                                    + x[IX(i, j - 1)]
                                    + x[IX(i, j)]
                                    + x[IX(i, j)]
                            )) * cRecip;
                }
            }
            set_bnd(b, x, N);
        }
    }

    // Clean up kind of
    void project(float[] velocX, float[] velocY, float[] p, float[] div, int iter, int N) {
            for (int j = 1; j < N - 1; j++) {
                for (int i = 1; i < N - 1; i++) {
                    div[IX(i, j)] = -0.5f * (
                             velocX[IX(i + 1, j)]
                            - velocX[IX(i - 1, j)]
                            + velocY[IX(i, j + 1)]
                            - velocY[IX(i, j - 1)]
                        ) / N;
                    p[IX(i, j)] = 0;
                }
            }
        set_bnd(0, div, N);
        set_bnd(0, p, N);
        lin_solve(0, p, div, 1, 6, iter, N);

            for (int j = 1; j < N - 1; j++) {
                for (int i = 1; i < N - 1; i++) {
                    velocX[IX(i, j)] -= 0.5f * (p[IX(i + 1, j)]
                                                    - p[IX(i - 1, j)]) * N;
                    velocY[IX(i, j)] -= 0.5f * (p[IX(i, j + 1)]
                                                    - p[IX(i, j - 1)]) * N;
                }
            }
        set_bnd(1, velocX, N);
        set_bnd(2, velocY, N);
    }

    void advect(int b, float[] d, float[] d0, float[] velocX, float[] velocY, float dt, int N) {
        float i0, i1, j0, j1;

        float dtx = dt * (N - 2);
        float dty = dt * (N - 2);

        float s0, s1, t0, t1;
        float tmp1, tmp2, x, y;

        float Nfloat = N;
        float ifloat, jfloat;
        int i, j;


            for (j = 1, jfloat = 1; j < N - 1; j++, jfloat++) {
                for (i = 1, ifloat = 1; i < N - 1; i++, ifloat++) {
                    tmp1 = dtx * velocX[IX(i, j)];
                    tmp2 = dty * velocY[IX(i, j)];

                    x = ifloat - tmp1;
                    y = jfloat - tmp2;
  

                    if (x < 0.5f) x = 0.5f;
                    if (x > Nfloat + 0.5f) x = Nfloat + 0.5f;
                    i0 = Mathf.Floor(x);
                    i1 = i0 + 1.0f;
                    if (y < 0.5f) y = 0.5f;
                    if (y > Nfloat + 0.5f) y = Nfloat + 0.5f;
                    j0 = Mathf.Floor(y);
                    j1 = j0 + 1.0f;

                    s1 = x - i0;
                    s0 = 1.0f - s1;
                    t1 = y - j0;
                    t0 = 1.0f - t1;


                    int i0i = (int)i0;
                    int i1i = (int)i1;
                    int j0i = (int)j0;
                    int j1i = (int)j1;

                        // fixxxxxxxxxxxxx
                    d[IX(i, j)] =

                         s0 * (t0 * d0[IX(i0i, j0i)] + t1 *  d0[IX(i0i, j1i)]) +
                         s1 * (t0 * d0[IX(i1i, j0i)] + t1 * d0[IX(i1i, j1i)]);
                }
            }
        set_bnd(b, d, N);
    }
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
