using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    public int N = 16;
    public int iter = 4;
    public Fluid fluid;
    public FilterMode filtermode = FilterMode.Point;
    public Texture2D tex;

    public float viscosity = 0.0002f;

    public bool render = false;
    public float power = 8;


    void Start() {
        tex = new Texture2D(N, N, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = filtermode;

        fluid = new Fluid(viscosity, N);

        RenderFluid ();



    }
    void RenderFluid() {



        var stepsize = 1f / N;//
        for (int y = 0; y < N; y++) {
            for (int x = 0; x < N; x++) {
                //texture.SetPixel(x, y, Color.red);
                //tex.SetPixel(x, y, new Color(fluid.density[IX(x,y)],0f , 0f));
                //tex.SetPixel(x, y, new Color(fluid.Vx[IX(x, y)], fluid.Vy[IX(x, y)], fluid.density[IX(x, y)]));
                if (render) {
                    //tex.SetPixel(x, y, new Color( fluid.velocity[x, y].magnitude , fluid.velocity[x, y].magnitude, fluid.velocity[x, y].magnitude));
                    tex.SetPixel(x, y, new Color( fluid.velocity[x, y].x, -fluid.velocity[x, y].x, Mathf.Abs(fluid.velocity[x, y].y)));

                }
                else {

                    tex.SetPixel(x, y, new Color(/*(Vector2. right * fluid.Vx[IX(x, y)] +  Vector2.up * fluid.Vy[IX(x, y)]).magnitude*/0, Mathf.Abs(fluid.velocity[x,y].x), Mathf.Abs(fluid.velocity[x, y].y)));
                }
            }

            //tex.SetPixel(13, y, Color.blue);
        }

        // Apply all SetPixel calls
        tex.Apply();


        GetComponent<Renderer>().material.mainTexture = tex;
    }

    Vector3 mouse0 = Vector2.zero;
    // Update is called once per frame
    void Update() {

        var s = Input.mousePosition - mouse0;
        s.Normalize();
        s *= 10;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            Vector2Int v = Vector2Int.RoundToInt(hit.textureCoord * N);
            if (Input.GetKey(KeyCode.Mouse0)) {
                fluid.AddVelocity(v.x, v.y, s * power / 8);

            }
        }
        var x = Mathf.Sin(Time.time) * 40;
        var y = Mathf.Cos(Time.time) * 40;
        //fluid.AddDensity(N / 2, N / 2, 10);
        //fluid.AddVelocity(N / 2, N / 2, x, y );

        fluid.AddVelocity((int)(N / 1.5), N / 2, -power * Vector2.right);
        fluid.AddVelocity((int)(N / 3), N / 2, power * Vector2.right);






        //fluid.AddVelocity(N / 4, N / 4, x *-1, y * -1);


        if (Input.GetKeyDown(KeyCode.Space)) {
            //fluid.AddVelocity(2, 3, 2, 4);
        }

        tex.filterMode = filtermode;




        fluid.FluidStep(Time.deltaTime, iter);
        RenderFluid();



        mouse0 = Input.mousePosition;
    }

    private void OnDestroy() {

    }
}
