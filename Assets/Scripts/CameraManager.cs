using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {


    public Camera camera1;
    public Camera camera2;
    bool moving;
    bool runFunction;
    bool once = true;


    float camA_TargetX;
    float camA_TargetY;
    float camA_TargetW;
    float camA_TargetH;


    void Update () {

        // Test Primary function
        // if (Input.GetKeyDown("space")) moving = true;
        // if (Input.GetKeyDown("space")) ResizeCamera(camera1, 0.5f, 0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);

        //Test LeftEatsRight
        if (Input.GetKeyDown("space")) moving = true;
        if (moving == true)
            {
                LeftEatsRight(camera1, camera2, 0.5f, false);
            }
    }

    //This is The primary function that others will use to perform camera movement tasks
    void ResizeCamera (Camera camera, float newX, float newY, float newW, float newH, float speedX, float speedY, float speedW, float speedH) {
        //get the rect dimensions of the Camera
        float x = camera.rect.x;
        float y = camera.rect.y;
        float w = camera.rect.width;
        float h = camera.rect.height;

        //Lerp the original rect values to ones we input in the funtion's conditions
        camera.rect = new Rect(Mathf.Lerp(x, newX, speedX * Time.deltaTime), Mathf.Lerp(y, newY, speedY * Time.deltaTime), Mathf.Lerp(w, newW, speedW * Time.deltaTime), Mathf.Lerp(h, newH, speedH * Time.deltaTime));
        if (x == newX && y == newY && w == newW && h == newH)
        {
            moving = false;
            once = true;
        }
    }

    //Takes two Conditions, Camera A and Camera B, and has A Move to the the space B occupied
    //Whichever direction we eat, camera 1 is on the left and camera 2 on the right

    void LeftEatsRight(Camera cameraA, Camera cameraB, float speed, bool leftToRight){
        //Get the Starting position of Cam B
        if (once == true) {
             camA_TargetX = cameraB.rect.x;
             camA_TargetY = cameraB.rect.y;
             camA_TargetW = cameraB.rect.width;
             camA_TargetH = cameraB.rect.height;

            once = false;
        }

        if (leftToRight == true){
            ResizeCamera(cameraA, camA_TargetX, camA_TargetY, camA_TargetW, camA_TargetH, speed, speed, speed, speed);
            ResizeCamera(cameraB, camA_TargetX + camA_TargetW, cameraB.rect.y, 0, cameraB.rect.height, speed, speed, speed, speed);
        } else{
            ResizeCamera(cameraA, camA_TargetX, camA_TargetY, camA_TargetW, camA_TargetH, speed, speed, speed, speed);
            ResizeCamera(cameraB, cameraB.rect.x, cameraB.rect.y, 0, cameraB.rect.height, speed, speed, speed, speed);
        }
        // Stop the function of Camera B's width is approximately 0
        if (cameraB.rect.width < 0.001)
        {
            moving = false;
            once = true;
        }
    }
}


/* Notes On Rect Sizes
 * all Presets leave a border of 0.02
 * 
 * Split Left/Right
 *          Left: x: 0.02 y: 0.03554, w: 0.47, h: 0.93
 *          Right: x: 0.51 y: 0.03554, w: 0.47, h: 0.93




*/
