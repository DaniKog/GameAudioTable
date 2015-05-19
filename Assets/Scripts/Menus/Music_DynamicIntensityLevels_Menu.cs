using UnityEngine;
using System.Collections;

public class Music_DynamicIntensityLevels_Menu : MonoBehaviour
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
			Application.LoadLevel("Music_DynamicIntensity_FMOD");
		}
		if(Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2))
		{
			Application.LoadLevel("Music_DynamicIntensity_Native4.6");
		}
		if(Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3))
		{
			Application.LoadLevel("Music_DynamicIntensity_Native5");
		}
		if(Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4))
		{
			Application.LoadLevel("Music_DynamicIntensity_Wwise");
		}
		if(Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9))
		{
			Application.LoadLevel("MainMenu");
		}
	}
}
