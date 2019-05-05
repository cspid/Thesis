using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class triggerDialogue : MonoBehaviour {

	public string talkToNode = "";

    // Use this for initialization
    private void Start()
    {
      //  RunDialogue("Intro");
    }


    public void RunDialogue (string node) {
		FindObjectOfType<DialogueRunner>().StartDialogue(node);
        print("Check");
	}
}
