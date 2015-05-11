using UnityEngine;
using System.Collections;
using UnityEngine.UI; // This library allows us to use the UI Classes in unity, we need itdisplay the pitch on the screen.

public class SFXPitchModulation_Wwise : MonoBehaviour
{

	uint bankID;
	PlayerMovement playerScript; // A reference to the movement script
	public Text displayPitch; // Text that will display the pitch
	float pitchValue = 0; // We will assign this value to the pitch before we play the event
	// Side Note:
	// I know that to make pitch modulation in Wwise all it takes is to tick the pitch modulation option on a sound.
	// I am doing this though a RTPC so I can display the Pitch in game.
	// Use this for initialization
	void Start ()
	{
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too
		
		AkSoundEngine.LoadBank ("ExampleBank", AkSoundEngine.AK_DEFAULT_POOL_ID, out bankID); //Load the sound bank, need to happened only once in a scene no reason to add this on every single script
		//Side note - my recommendation is to create your own handler Class and that will handle all Play and Stop SFX functions.
		//For more information watch https://www.youtube.com/watch?v=j5Aq5hg1dcA
	}
	
	void PlaySFX()
	{
		//Unchanged pitch is 0
		pitchValue = Random.Range(-5000, 5000); // the pitch modulation is Wwise runs between -48000 and 48000 
		//I noticed that in those ranges on the high ends it plays the same sounds. Roughly (-48000 - -5000) and (5000 - 48000) sounds the same.
		AkSoundEngine.SetRTPCValue("SFX_PitchMod",pitchValue); // I set the value of the RTPC to match the value of our float.
		displayPitch.text = ("Pitch Value :" + pitchValue.ToString()); // Display the current pitch on the screen
		AkSoundEngine.PostEvent ("Play_Spike_Explode1",gameObject); //Play an event from the Wwise engine
	}
	
	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}
