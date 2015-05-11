using UnityEngine;
using System.Collections;

public class SFX_PitchMod_Menu : MonoBehaviour {

	public void SFX_PitchMod_Native()
	{
		Application.LoadLevel("SFX_PitchMod_Native");
	}
	public void SFX_PitchMod_Wwise()
	{
		Application.LoadLevel("SFX_PitchMod_Wwise");
	}
	public void SFX_PitchMod_FMOD()
	{
		Application.LoadLevel("SFX_PitchMod_FMOD");
	}
}
