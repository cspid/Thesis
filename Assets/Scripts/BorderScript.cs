using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderScript : MonoBehaviour
{

	[ExecuteInEditMode]

	//public Camera[] cameras;
	//public List<Camera> cameras;
	public Camera prefabCam;
	public Camera setCam;
	public float offset = 0.5f;

	public bool checkingBordersize;

	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (checkingBordersize == true)
		{
			//bool once = true;
			//if (once == true)
			//{
				GameObject[] borderCams = GameObject.FindGameObjectsWithTag("Border");

				for (int i = 0; i < borderCams.Length; i++)
				{
					//borders[i].GetComponentInPa
					Rect CamRect = borderCams[i].transform.parent.GetComponent<Camera>().rect;
					borderCams[i].GetComponent<Camera>().rect = new Rect(CamRect.x - offset, CamRect.y - offset, CamRect.width + (offset * 2), CamRect.height + (offset * 2));
				}
			}
		}
	//}

	public void CheckBorderSize()
	{
		bool once = true;
        if (once == true)
        {
            GameObject[] borderCams = GameObject.FindGameObjectsWithTag("Border");

            for (int i = 0; i < borderCams.Length; i++)
            {
                //borders[i].GetComponentInPa
                Rect CamRect = borderCams[i].transform.parent.GetComponent<Camera>().rect;
                borderCams[i].GetComponent<Camera>().rect = new Rect(CamRect.x - offset, CamRect.y - offset, CamRect.width + (offset * 2), CamRect.height + (offset * 2));
            }
        }
	}

    public void CheckForCams(){
		
		print(Camera.allCameras.Length);
		Camera[] usedCams = Camera.allCameras;


		for (int i = 0; i < usedCams.Length; i++){
			if (usedCams[i].tag == "Camera")
			{
				if (usedCams[i].transform.childCount == 0)
				{
					print("test");
					//setCam.depth = usedCams[i].depth - 1;
					setCam = Instantiate(prefabCam, usedCams[i].transform.position, usedCams[i].transform.rotation);
					setCam.clearFlags = CameraClearFlags.SolidColor;
					setCam.backgroundColor = new Color(0, 0, 0, 0);
					setCam.rect = new Rect(usedCams[i].rect.x - offset, usedCams[i].rect.y - offset, usedCams[i].rect.width + (offset * 2), usedCams[i].rect.height + (offset * 2));
					setCam.tag = "Border";
					setCam.transform.parent = usedCams[i].transform;
					//print(setCam.rect);
				}
			}
		}
	}
}
