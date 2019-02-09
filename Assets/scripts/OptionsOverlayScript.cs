/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controlling the Options overlay and components
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OptionsOverlayScript : MonoBehaviour {

	public GameObject ui_accountOverlay;
	public Slider ui_sliderBackground;
	public Slider ui_sliderSoundEffect;

	private float backgroundVolume;
	private float soundEffectVolume;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;

	void Start () {
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");

		AudioManagerScript audioMan = 
			glob_audiomanager.GetComponent<AudioManagerScript> ();

		ui_sliderBackground.value = audioMan.audio_menubackground.volume;
		ui_sliderSoundEffect.value = audioMan.audio_soundeffect.volume;

		backgroundVolume = ui_sliderBackground.value;
		soundEffectVolume = ui_sliderSoundEffect.value;
	}

	//Called once per frame and checks if the volume of the slider changes
	//against the current volume
	void Update () {
		if (ui_sliderBackground.value != backgroundVolume) {
			//Change the background volume
			backgroundVolume = ui_sliderBackground.value;
			glob_audiomanager.SendMessage ("AdjustMenuBackgroundVolume",
				ui_sliderBackground.value);
			
		}
		if (ui_sliderSoundEffect.value != soundEffectVolume) {
			//Change the sound effect volume
			soundEffectVolume = ui_sliderSoundEffect.value;
			glob_audiomanager.SendMessage ("AdjustSoundEffectVolume",
				ui_sliderSoundEffect.value);
		}
	}

	/// <summary>
	/// Command to close the overlay
	/// </summary>
	public void btn_CloseOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_accountOverlay.SetActive (true);
		this.gameObject.SetActive (false);
	}
}