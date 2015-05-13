using UnityEngine;
using System.Collections;
using UnityEngine.UI; // This library allows us to use the UI Classes in unity, we need itdisplay the pitch on the screen.

public class SFXPitchModulation_Native : MonoBehaviour
{
	public AudioClip shardSound; //An public Audioclip (we will need to assign the sound manually)
	AudioSource myAudioSource; //  A reference to the game object's audio source
	PlayerMovement playerScript; // A reference to the movement script
	public Text displayPitch; // Text that will display the pitch
	public float minPitch = 0; // A float that will determine the minimum pitch value (the lowest value is 0)
	public float maxPitch = 3; // A float that will determine the maximum pitch value (the highest value is 3)
	// Important Note.
	// In unity 5's new audio system the pitch can go to blow 0 to the lowest of -3. This will not play audio,
	// but if audio is playing in and pitch goes blow 0 then it will cause the audio to play backwards. Super interesting, go ahead download the project and try for yourself.

	// Use this for initialization
	void Start ()
	{
		myAudioSource = gameObject.GetComponent<AudioSource> (); //getting the script reference
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too
	}
	
	void PlaySFX()
	{
		if ( shardSound != null)
		{
			//Unchanged pitch is 1
			float randomPitch = Random.Range(minPitch,maxPitch); //Choose a random number between the min pitch value and the max pitch value
			myAudioSource.pitch = randomPitch; //assign the random pitch value to the pitch variable inside our Audio Source
			displayPitch.text = ("Pitch Value :" + randomPitch.ToString()); // Display the current pitch on the screen
			myAudioSource.PlayOneShot(shardSound); //Play that sound as a oneshot from the audio sources
		}
	}
	
	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}
