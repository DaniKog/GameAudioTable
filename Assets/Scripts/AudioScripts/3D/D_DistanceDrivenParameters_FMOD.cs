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
		displayDistance.text = ("Distance To Source: " + distanceToSource.ToString("##.##")); // Display Distance
		// The Distance is Calculated inside the FMOD engine. It is between the GameObject sound sound and the Gameobject that has the FMOD_Listener attached to 
		// In this case it is The FPS Controller and the music source. All the parameters are set in engine. Click on Add Parameter -> add build in Parameters -> Distance
		// Thanks. @redbluemonkey. 

		// Because this script is on the sound source we need to use Vector3.Dot
		// Vector3.Dor allows you to determine where a object is in relation to another object.
		// The Angle is also calcualted inside the FMOD mode engine. Add Build in Parameter -> Event Cone Angle.
		// Vector3.Dot is here only for to display text on the camera.
		Vector3 vecDistace = (player.transform.position - transform.position); 
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		if (Vector3.Dot(forward,vecDistace) < 0 )
		{
			displayDelay.text = ("Delay : On");
			//The Delay is afully exagurated so it will be obvious when it is On.
		}
		else
		{
			displayDelay.text = ("Delay : Off");
		}
	}
	void OnDestroy()
	{
		musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);// Stops the music event
	}

}
