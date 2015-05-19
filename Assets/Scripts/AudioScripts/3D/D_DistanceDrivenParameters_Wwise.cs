using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes

public class D_DistanceDrivenParameters_Wwise : MonoBehaviour {

	//Some audio engines allows you to define Parameters that will be driven by Distance. 
	//In Wwise it is defined inside the sound it self in the Positioning tab -> 3D. Assgin an Attenuation and click on edit.
	uint bankID; // an Uint (double positive integer) to save to Sound bank ID to
	public GameObject soundSourceObj; // A reference to the soundsource's GameObject to get the distance
	float distanceToSource; // flaot to save the distance between the audio source and the player
	public Text displayDistance; // Referance to a Text UI on screen to display the distance
	public Text displayLowPass; // Referance to a Text UI on screen to display the LowPass Filter
	// Use this for initialization
	void Start ()
	{
		AkSoundEngine.LoadBank ("ExampleBank", AkSoundEngine.AK_DEFAULT_POOL_ID, out bankID); //Load the sound bank, need to happened only once in a scene no reason to add this on every single script
		//Side note - my recommendation is to create your own handler Class and that will handle all Play and Stop SFX functions.
		//For more information watch https://www.youtube.com/watch?v=j5Aq5hg1dcA
		AkSoundEngine.PostEvent ("Play_Tetris_Remix",soundSourceObj); //Play an event from the Wwise engine from the soundSourceObj Game Object 
		//On Tetris_Remix sound in Wwise the Max distance is set to 30. Which is the size of the platform in the scene

	}
	
	// Update is called once per frame
	void Update ()
	{
		distanceToSource = Vector3.Distance (soundSourceObj.transform.position, gameObject.transform.position); // Calculating the distance to between the player and the soundsource and assigning it to a variable
		displayDistance.text = ("Distance To Source: " + distanceToSource.ToString("##.##")); // Display Distance
		// The Distance is Calculated inside the Wwise engine. It is between the GameObject sound sound and the Gameobject that has the AkListener attached to.
		// In this case it is The main camera and soundSourceObj. Any Game Parameter can be set to work with distance. Create a new Parameter and click on Blind to Built-In Parameters. // Thanks @AKMikeD
	}

	void OnDestroy()
	{
		AkSoundEngine.StopAll(); // Stops all events that are playing.
	}

	// When the player crosses to the other side of the Sound source
	// The Angle is actually calulated in Wwise. But we are not using the Build-In Parameters for this one.
	// We are useing the one in the positioning tab -> Attenuation -> Custom -> Edit -> Cone Attentuation.
	// The Trigger are here only for display
	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.name == "BackSide")
		{
			displayLowPass.text = ("LowPass: On");
		}
	}
	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.name == "BackSide")
		{
			displayLowPass.text = ("LowPass: Off");
		}
	}
	
	

}
