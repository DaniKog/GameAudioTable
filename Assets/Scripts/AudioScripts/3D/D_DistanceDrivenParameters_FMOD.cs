using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes

public class D_DistanceDrivenParameters_FMOD : MonoBehaviour
{
	// This Script is on the MusicSource game object.
	public GameObject player; // A reference to the soundsource's GameObject to get the distance
	float distanceToSource; // flaot to save the distance between the audio source and the player
	public Text displayDistance; // Referance to a Text UI on screen to display the distance
	public Text displayFlanger; // Referance to a Text UI on screen to display the Flanger Rate effect
	public Text displayDelay; // Referance to a Text UI on screen to display the Delay effect

	FMOD.Studio.EventInstance musicEvent; 			    // We need to use an have a reference to an instance of an Object if we want to access it Parameters
	FMOD.Studio.ParameterInstance flangerRateParameter;	// for more information Watch https://www.youtube.com/watch?v=p14Hx_jLGEA
	FMOD.Studio.ParameterInstance delayParameter;	//Get a delay Parameter that we set in FMOD

	bool engineInit = false; // A bool that tell us when the parameters are loaded. Because before OnTiriggerEnter would happend before Start could finish assigning the Parameters.
	void Start ()
	{
		print(musicEvent = FMOD_StudioSystem.instance.GetEvent ("event:/Tetris_Remix")); // Get an instance of the event
		musicEvent.getParameter ("Tetris_Flanger", out flangerRateParameter); // Use the instance of the event to access the parameter on that even
		musicEvent.getParameter ("Bool_Delay", out delayParameter); // Use the instance of the event to access the parameter on that even. This will act like a bool, it will enable and disable the delay
		// when the player is going behind the object.
		musicEvent.start ();
		engineInit = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		distanceToSource = Vector3.Distance (player.transform.position, gameObject.transform.position); // Calculating the distance to between the player and the soundsource and assigning it to a variable
		flangerRateParameter.setValue(distanceToSource); // Assign distance to the flanger rate parameter. The parameter set in FMOD to be from 0 - 40 which fits the same size of the platform.
		displayFlanger.text = ("Flanger Rate : " + distanceToSource); // Display pitch Value the distacne and the value of the Flanger is 1 to 1
		displayDistance.text = ("Distance To Source: " + distanceToSource.ToString("##.##")); // Display Distance


		// Because this script is on the sound source we need to use Vector3.Dot
		// Vector3.Dor allows you to determine where a object is in relation to another object.
		Vector3 vecDistace = (player.transform.position - transform.position); 
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		if (Vector3.Dot(forward,vecDistace) < 0 )
		{
			delayParameter.setValue(1f); // Turns on the delay
			displayDelay.text = ("Delay : On");
		}
		else
		{
			delayParameter.setValue(0f); // Turns off the delay
			displayDelay.text = ("Delay : Off");
		}
	}
	void OnDestroy()
	{
		musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);// Stops the music event
	}
	
	//When the player crosses to the other side of the Sound source
	//Could do this with Vector3.Dot but this code is about Audio Vector.Dot might just confuse some people.
//	void OnTriggerEnter(Collider col)
//	{
//		print("Entered Trigger");
//		if(col.gameObject.name == "BackSide")
//		{
//			print("Trigger is true");
//			if(engineInit == true)// check if the parameters have been initilized
//			{
//				//delayParameter.setValue(1); // Turns on the delay
//				//displayDelay.text = c("Delay : On");
//				print("Called the Change the fucntion");
//			}
//		}
//	}
//	void OnTriggerExit(Collider col)
//	{
//		if(col.gameObject.name == "BackSide")
//		{
//			if(engineInit == true) // check if the parameters have been initilized
//			{
//				delayParameter.setValue(0); // Turns off the delay
//				displayDelay.text = ("Delay : Off");
//			}
//		}
//	}
}
