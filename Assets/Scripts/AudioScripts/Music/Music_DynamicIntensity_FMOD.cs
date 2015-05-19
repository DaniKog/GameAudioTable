using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes

public class Music_DynamicIntensity_FMOD : MonoBehaviour
{
	FMOD.Studio.EventInstance musicEvent; 			    // we need to use an have a reference to an instance of an Object if we want to access it Parameters
	FMOD.Studio.ParameterInstance musicIntensity;		// for more information Watch https://www.youtube.com/watch?v=p14Hx_jLGEA
	float valueToSet = 0; // this float will determine the value that we need to set our parameter to
	float IntensityValue = 0; // float to save the parameter Value
	float tState;	//this a part of the Mathf.Lerp function, by incrementing this value we lerp between the values of the music parameter.
	public float fadespeed = 0.01f; // the speed that the parameter is changing
	public Text displayIntensity; // displays the intensity on screen
	public Text displayParameter; // displays the parameter's value on screen

	// Use this for initialization
	void Start ()
	{
		musicEvent = FMOD_StudioSystem.instance.GetEvent ("event:/MusicIntensity"); // get an instance of the event
		musicEvent.getParameter ("Intensity", out musicIntensity); // Use the instance of the event to access the Intensity parameter on that event
		musicIntensity.setValue (0); // set the initial music Intensity to 0
		displayParameter.text = ("Parameter Value: 0 "); // initially show 0 in the parameter text on screen
		musicEvent.start (); // Start the music event
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(IntensityValue > valueToSet+0.01 || IntensityValue < valueToSet-0.01 ) // Check if current parameter value matches the music state
		{
			tState += Time.deltaTime*fadespeed; // Increment the tStante to advance the Mathf.Lerp fucntion State's value is beteen 0-1
			IntensityValue = Mathf.Lerp(IntensityValue,valueToSet,tState); // MathF.Lerp interpellates betweeen 2 values. To learn more about Mathf.Lerp go here - http://answers.unity3d.com/questions/237294/how-the-heck-does-mathflerp-work.html 
			musicIntensity.setValue (IntensityValue); // set the intensity of the music
			displayParameter.text = ("Parameter Value: " + IntensityValue.ToString("#.##")); // Display the current intensity
		}
	}
	void OnTriggerEnter(Collider col) // when entering a new music intensity  room
	{
		switch (col.gameObject.tag) // Switch statement to check which room we entered
		{
		case "MusicRoom1":
			SetIntensity(0); // Calling the Set Intensity function
				break;		
		case "MusicRoom2":
			SetIntensity(1);
			break;
		case "MusicRoom3":
			SetIntensity(2);
			break;
		case "MusicRoom4":
			SetIntensity(3);
			break;
		default:
			break;
		}
	}
	void SetIntensity(int intensityLevel)
	{
		valueToSet = intensityLevel; // Set the Value to the current music intensity state
		tState = 0; 	//Reset tState for Mathf.Lerp 
		displayIntensity.text = ("Music Intensity: " + (intensityLevel+1).ToString()); // Display the current intensity *adding one for better display (1-4) instead of (0-3)
	}

	void OnDestroy()
	{
		musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);// Stops the music event
	}
}
