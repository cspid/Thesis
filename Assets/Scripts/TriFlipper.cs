    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriFlipper : MonoBehaviour
{
    float rotationAmount = 90.0f;
    public bool turn;
    MeshRenderer renderer;
    private Vector3 oldUp;

    GameObject obj;
    Collider objCollider;

    public Camera cam;
    Plane[] planes;

    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        oldUp = transform.up;
        //cam = Camera.main;
       
    }


    void Update()
    {

      
          //  Debug.Log(obj.name + " has been detected!");
            if (turn == true)
            {
                if (gameObject.name == "Down")
                {
                    transform.Rotate(rotationAmount * Time.deltaTime, 0, 0, Space.Self);
                }
                else
                {
                    transform.Rotate(-rotationAmount * Time.deltaTime, 0, 0, Space.Self);

                }
               // print(Vector3.Angle(transform.up, -oldUp));
                if (Vector3.Angle(transform.up, -oldUp) <= 5.0f)
                {
                    turn = false;
                    transform.up = -oldUp;
                }
                //if (transform.localEulerAngles.x >= 100)
                //{
                //    renderer.enabled = false;
                //    turn = false;
                //    transform.localEulerAngles = new Vector3(0, 0, 0);
                //}
            }
       
       
    } 
}
