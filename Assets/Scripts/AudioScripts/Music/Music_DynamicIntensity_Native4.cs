using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Allows you to use UI Classes
public class Music_DynamicIntensity_Native4 : MonoBehaviour
{
	public AudioSource[] musicLayers; // each layer of Intensity is in his own Audio Source, This an array of all those Audio Sources
	public float fadeSpeed = 1; // the fade speed between the music Intensity levels 
	int musicIntensity;	// the music Intensity 
	bool changeIntensity = false; // a bool that will determine if the states are changing or not
	AudioSource stinger; // Audio source with a stinger audio clip that will play when the Intensity reaches to 4
	public Text displayIntensity; // displays the intensity on screen
	public Text displayParameter; // displays the parameter's value on screen
	// Use this for initialization
	void Start ()
	{
		if(musicLayers.Length > 0) // if the array music Layer is not empty 
		{
			foreach (AudioSource audiosource in musicLayers) // Go through music layers array 
			{
				audiosource.Play(); // Play all the Intensity levels * it is very important that the Play() function will be called for all the audio sources on the same frame, makes sure that they will stay in Sync. 
				audiosource.volume = 0; // Mute all the Intensity levels 
			}
			stinger = gameObject.GetComponent<AudioSource>(); // Get the audio source component from this game object.
		}
		else
		{
			Debug.LogError("musicLayers array is empty"); // Present an error message when the array is empty 
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(changeIntensity == true) //if changeIntensity is set to true start changing Intensity
		{
			if(musicLayers[musicIntensity].volume < 1) // as long as the desired intensity audio source lower than 100% volume call Change layer function
			{
				ChangeLayer(musicIntensity);
			}
			else
			{
				changeIntensity = false; // when the volume have reached 100% stop changeIntensity is false and now it will not call the Change layer function
			}
			displayParameter.text = ("Current Intensity Volume: " + musicLayers[musicIntensity].volume.ToString("#.##"));
		}

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
		musicIntensity = intensityLevel;// Set the Value to the current music intensity state
		changeIntensity = true; // sets this bool to true so the layers can be changed through the update function
		displayIntensity.text = ("Music Intensity: " + (intensityLevel+1).ToString()); // Display the current intensity *adding one for better display (1-4) instead of (0-3)
	}

	void ChangeLayer(int changeTo)
	{
			for (int i = 0; i < musicLayers.Length; i++) 
			{
				if(i == changeTo)
				{
					musicLayers[i].volume += Time.deltaTime*fadeSpeed; // add volume to the desired Intensity layer
				}
				else
				{
					musicLayers[i].volume -= Time.deltaTime*fadeSpeed; // lower volume from all other Intensity layers
				}
			// this allows us to have smooth transitions because the same amount that is add to the desired Intensity layer is lowered all the other ones.
			}
	}
}
