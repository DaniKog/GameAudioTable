using UnityEngine;
using System.Collections;
using UnityEngine.UI; // This library allows us to use the UI Classes in unity, we need itdisplay the pitch on the screen.

public class SFXPitchModulation_FMOD : MonoBehaviour
{

	PlayerMovement playerScript; // A reference to the movement script
	public Text displayPitch; // Text that will display the pitch
	float pitchValue = 0; // We will assign this value to threference e pitch before we play the event
	// Side Note:
	// I know that to make pitch modulation in FMOD all it takes is to add pitch modulation on a sound event.
	// I am doing this though a Game Parameter so I can display the Pitch in game.
	FMOD.Studio.EventInstance sFXEvent; 			// We need to use an have a reference to an instance of an Object if we want to access it Parameters
	FMOD.Studio.ParameterInstance pitchParameter;	// for more information Watch https://www.youtube.com/watch?v=p14Hx_jLGEA


	// Use this for initialization
	void Start ()
	{
		playerScript = gameObject.GetComponent<PlayerMovement> (); //getting the script reference
		playerScript.PlayerMoved += PlaySFX; //Assigning the PlayerMoved event to PlaySFX() Funciton. So every time PlayerMoved event is fired PlaySFX() Function is fired too
		sFXEvent = FMOD_StudioSystem.instance.GetEvent ("event:/Spike_Explode"); // Get an instance of the event
		sFXEvent.getParameter ("PitchMod", out pitchParameter); // Use the instance of the event to access the parameter on that even
	}
	
	void PlaySFX()
	{
		//Unchanged pitch is 0
		pitchValue = Random.Range(-24f, 24f); // the pitch modulation is FMOD runs between -24 and 24 
		pitchParameter.setValue (pitchValue); // Set the parameter value
		displayPitch.text = ("Pitch Value :" + pitchValue.ToString()); // Display the current pitch on the screen
		//FMOD_StudioSystem.instance.PlayOneShot ("event:/Spike_Explode",gameObject.transform.position); // Play one shot will not work because it is not set on a instance of an event
		sFXEvent.start (); // Start the event. I am sure there is a better way to do this I am still looking into it // FIX
	}
	
	void OnDestroy()
	{
		playerScript.PlayerMoved -= PlaySFX; //Unassign the event
	}
}
