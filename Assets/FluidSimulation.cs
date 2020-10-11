using Sirenix.OdinInspector.Editor.Validation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      
        var texture = new Texture2D(256, 256, TextureFormat.RGB24, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        // set the pixel values
        var stepsize = 1f / 256f;

        for (int y = 0; y < 256; y++) {
            for (int x = 0; x < 256; x++) {
                //texture.SetPixel(x, y, Color.red);
                texture.SetPixel(x, y, new Color(x * stepsize, y * stepsize, 0f));
            }
            
             texture.SetPixel(34, y, Color.blue);
        }

        // Apply all SetPixel calls
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
