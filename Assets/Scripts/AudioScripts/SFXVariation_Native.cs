using UnityEngine;
using System.Collections;

public class SFXVariation_Native : MonoBehaviour
{
	public AudioClip[] moveSounds; //an public array of sounds (we will need to assign the sounds manually)
	AudioSource myAudioSource; //  A reference to the game object's audio source
	PlayerMovement playerScript; // A reference to the movement script

	// Use this for initialization
	void Start ()
	{
		myAudioSource = gameObject.GetComponent<AudioSource> (); //getting the script reference
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too
	}

	void PlaySFX()
	{
		if ( moveSounds[0] != null)
		{
			int randomClip = Random.Range(0,moveSounds.Length); //choose a random number between 0 and how many variations we have in the array
			myAudioSource.PlayOneShot(moveSounds[randomClip]); //Play that sounds as a oneshot from the audio sources
		}
	}

	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}
