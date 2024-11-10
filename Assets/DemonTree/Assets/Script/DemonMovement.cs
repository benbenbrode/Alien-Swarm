using UnityEngine;
using System.Collections;

public class DemonMovement : MonoBehaviour {
	private Animator anim;
	int hIdles;
	int hAngry;
	int hAttack;
	int hGrabs;
	int hThumbsUp;
		
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		hIdles = Animator.StringToHash("Idles");
		hAngry = Animator.StringToHash("Angry");
		hAttack = Animator.StringToHash("Attack");
		hGrabs = Animator.StringToHash("Grabs");
		hThumbsUp = Animator.StringToHash("ThumbsUp");
	}
	
	// Update is called once per frame
	void Update () {
	        if (Input.GetKeyDown(KeyCode.W)) {
			if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idles")) {
				anim.SetBool(hIdles, false);
				anim.SetBool(hAngry, true);
	             }
	        } else if (Input.GetKeyDown(KeyCode.S)) {
			if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idles")) {
				anim.SetBool(hIdles, false);
				anim.SetBool(hAttack, true);
			    }
	        } else if (Input.GetKeyDown(KeyCode.A)) {
			if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idles")) {
				anim.SetBool(hIdles, false);
				anim.SetBool(hGrabs, true);
			    }
			} else if (Input.GetKeyDown(KeyCode.D)) {
			if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idles")) {
				anim.SetBool(hIdles, false);
				anim.SetBool(hThumbsUp, true);
			    }
		    } else {
			if(!anim.GetCurrentAnimatorStateInfo(0).IsName("Idles")) {
				anim.SetBool(hIdles, true);
				anim.SetBool(hAngry, false);
				anim.SetBool(hAttack, false);
				anim.SetBool(hGrabs, false);
				anim.SetBool(hThumbsUp, false);
			}
		}
	}
}