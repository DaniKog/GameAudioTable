using UnityEngine;
using System.Collections;

public class D_DistanceDPar_Menu : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1))
		{
			Application.LoadLevel("3D_DistanceDrivenParameters_FMOD");
		}
		if(Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
		{
			Application.LoadLevel("3D_DistanceDrivenParameters_Native");
		}
		if(Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
		{
			Application.LoadLevel("3D_DistanceDrivenParameters_Wwise");
		}
		if(Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
		{
			Application.LoadLevel("MainMenu");
		}
	}
}
