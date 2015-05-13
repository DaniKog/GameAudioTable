using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes 

public class D_DistanceDrivenParameters_Native : MonoBehaviour
{
	//Some audio engines allows you to define Parameters that will be driven by Distance. Here is how you make the same thing using Untiy Native.

	public GameObject soundSourceObj; // A reference to the soundsource's GameObject to get the distance
	AudioSource soundSource; // A reference to the soundsource's GameObject to get the distance
	float distanceToSource; // flaot to save the distance between the audio source and the player
	bool isBehindTheObject = false; // A variable to check if the player is behind the object
	public Text displayDistance; // Reference  to a Text UI on screen to display the distance
	public Text displayPitch; // Reference  to a Text UI on screen to display the Pitch Value
	// Use this for initialization
	void Start ()
	{
		soundSource = soundSourceObj.GetComponent<AudioSource> (); //getting the audio source component from the sound Source gameobject
	}
	
	// Update is called once per frame
	void Update ()
	{
		distanceToSource = Vector3.Distance (soundSourceObj.transform.position, gameObject.transform.position); // Calculating the distance to between the player and the soundsource and assigning it to a variable
		if(isBehindTheObject == true) 
		{
			// If the player is behind the Object it is cause the pitch value to be negative.
			// Will cause the audio to play backwards.
			soundSource.pitch = distanceToSource * -0.5f; // Covert Distance to Pitch ( you could use 1 to 1 ratio I just found this to be more plesent.
			// Tune this to fit your preferences.
		}
		else
		{
			soundSource.pitch = distanceToSource * 0.5f;
			// Covert Distance to Pitch ( you could use 1 to 1 ratio I just found this to be more plesent.
			// Tune this to fit your preferences.
		}
		displayPitch.text = ("Pitch Value: " + soundSource.pitch.ToString("##.##")); // Display pitch Value
		displayDistance.text = ("Distance To Source: " + distanceToSource.ToString("##.##")); // Display Distance
	}


	//When the player crosses to the other side of the Sound source
	//Could do this with Vector3.Dot but this code is about Audio Vector.Dot might just confuse some people.
	void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.name == "BackSide")
		{
			isBehindTheObject = true;
		}
	}
	void OnTriggerExit(Collider col)
	{
		if(col.gameObject.name == "BackSide")
		{
			isBehindTheObject = false;
		}
	}
}
