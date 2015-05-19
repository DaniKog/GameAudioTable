using UnityEngine;
using System.Collections;
using UnityEngine.UI;  // Allows you to use UI Classes such as Text 
using UnityEngine.Audio; // Allows you to use unity 5 new Audio Classes :)



public class Music_DynamicIntensity_Native5 : MonoBehaviour
{
	public AudioMixerSnapshot[] Intensitylevels; // each layer of Intensity is represented by Audio Mixer Screenshots, This an array of all those Audio Mixer Screenshots
	public AudioSource[] musicLayers; // each layer of Intensity is in his own Audio Source, This an array of all those Audio Sources
	public float fadeSpeed = 0.5f; // the fade speed between the music Intensity levels 
	AudioSource stinger; // Audio source with a stinger audio clip that will play when the Intensity reaches to 4
	public Text displayIntensity; // displays the intensity on screen
	// Use this for initialization
	void Start ()
	{
		if(Intensitylevels.Length > 0) // if the array music Layer is not empty 
		{
			Intensitylevels [0].TransitionTo (0); //Instantly active Screenshot 1 which is music Intensity 1
			foreach (AudioSource audiosource in musicLayers) // Go through music layers array 
			{
				audiosource.Play(); // Play all the Intensity levels * it is very important that the Play() function will be called for all the audio sources on the same frame, makes sure that they will stay in Sync. 
				//I tried using the Play on Awake button on the audio source but sometimes Awake is being called at different times for each object that resulted in out of sync loop.
			}
		}
		else
		{
			Debug.LogError("musicLayers array is empty"); // Present an error message when the array is empty 
		}
		stinger = gameObject.GetComponent<AudioSource>(); // Get the audio source component from this game object.
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	void OnTriggerEnter(Collider col) // when entering a new music intensity  room
	{
		switch (col.gameObject.tag) // Switch statement to check which room we entered
		{
		case "MusicRoom1":
			SetIntensity(0); //Calls a function to change the Intensity levels
			break;		
		case "MusicRoom2":
			SetIntensity(1);
			break;
		case "MusicRoom3":
			SetIntensity(2);
			break;
		case "MusicRoom4":
			SetIntensity(3);
			stinger.Play();
			break;
		default:
			break;
		}
	}
	void SetIntensity(int intensityLevel)
	{
		Intensitylevels [intensityLevel].TransitionTo (fadeSpeed);
		displayIntensity.text = ("Music Intensity: " + (intensityLevel+1).ToString()); // Display the current intensity *adding one for better display (1-4) instead of (0-3)
	}
}
