using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes
public class Music_DynamicIntensity_Wwise : MonoBehaviour
{
	uint bankID; // an Uint (double positive integer) to save to Sound bank ID 
	
	public Text displayIntensity; // displays the intensity on screen
	public Text displayParameter; // displays the parameter's value on screen
	// Use this for initialization
	void Start ()
	{
		AkSoundEngine.LoadBank ("ExampleBank", AkSoundEngine.AK_DEFAULT_POOL_ID, out bankID); //Load the sound bank, need to happened only once in a scene no reason to add this on every single script
		//Side note - my recommendation is to create your own handler Class and that will handle all Play and Stop SFX functions.
		//For more information watch https://www.youtube.com/watch?v=j5Aq5hg1dcA
		AkSoundEngine.PostEvent ("Play_MusicDynamicIntensity",gameObject); //Play an event from the Wwise engine. The game object doesn't really matter because Music is useally a 2D sound.
		displayParameter.text = ("Parameter Value: Fades are in Engine "); // initially show 0 in the parameter text on screen
		AkSoundEngine.SetState("DynamicMusicIntensity","Int1"); //Sets initial value to the State container. 
		// t sometimes you may forget to set the initial Value fo the State, so consider this as a reminder
	}
	
	void OnTriggerEnter(Collider col) // when entering a new music intensity  room
	{
		switch (col.gameObject.tag) // Switch statement to check which room we entered
		{
		case "MusicRoom1":
			displayIntensity.text = "Music Intensity: 1"; // Display the current intensity 
			AkSoundEngine.SetState("DynamicMusicIntensity","Int1"); // Setting the Intensity State.
			break;		
		case "MusicRoom2":
			displayIntensity.text = "Music Intensity: 2";
			AkSoundEngine.SetState("DynamicMusicIntensity","Int2");
			break;
		case "MusicRoom3":
			displayIntensity.text = "Music Intensity: 3";
			AkSoundEngine.SetState("DynamicMusicIntensity","Int3");
			break;
		case "MusicRoom4":
			displayIntensity.text = "Music Intensity: 4";
			AkSoundEngine.SetState("DynamicMusicIntensity","Int4");
			AkSoundEngine.PostTrigger("Music_Intensity_Fill",gameObject); // Calling a Trigger to activate a Stringer on the event, (We are doing this same thing in FMOD just in Engine)
			break;
		default:
			break;
		}
	}
	void OnDestroy()
	{
		AkSoundEngine.StopAll(); // Stops all events that are playing.
	}
}