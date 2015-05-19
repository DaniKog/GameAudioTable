using UnityEngine;
using System.Collections;

public class Plane_LookAtCamera : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//transform.LookAt(Camera.main.transform.position, -transform.parent.up);
		transform.rotation = Camera.main.transform.rotation;
	}
}
