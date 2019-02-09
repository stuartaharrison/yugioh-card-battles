/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controlling audio across the entire app/game
*/

using UnityEngine;
using System.Collections;

public class AudioManagerScript : MonoBehaviour {

	//The audio sources/game objects attached to this
	public AudioSource audio_menubackground;
	public AudioSource audio_duelscene;
	public AudioSource audio_soundeffect;
	public AudioSource audio_soundeffectloop;
	//The actually audio files to be played
	public AudioClip background_mainmenu;
	public AudioClip background_duelscene;
	public AudioClip click_a;
	public AudioClip click_b;
	//Duel Audio Clips
	public AudioClip card_draw;
	public AudioClip lp_counterDownLoop;
	public AudioClip lp_counterAppear;
	public AudioClip lp_counterStops;
	public AudioClip lp_counterZero;
	public AudioClip duel_cardSummon;
	public AudioClip duel_monsterdestroyed;
	public AudioClip duel_cardSet;
	public AudioClip duel_cardActivate;
	public AudioClip duel_coinToss;
	public AudioClip duel_endTurn;
	public AudioClip duel_startGame;
	public AudioClip duel_endGame;
	public AudioClip duel_directHit;

	void Start () {
		LoadAudioSettings ();
	}

	/// <summary>
	/// Saves the audio settings.
	/// </summary>
	private void SaveAudioSettings() {
		PlayerPrefs.SetFloat ("OPT_AUDIOBG", audio_menubackground.volume);
		PlayerPrefs.SetFloat ("OPT_AUDIOEF", audio_soundeffect.volume);
		PlayerPrefs.Save ();
	}

	/// <summary>
	/// Loads the audio settings.
	/// </summary>
	private void LoadAudioSettings() {
		audio_menubackground.volume = PlayerPrefs.GetFloat ("OPT_AUDIOBG", 1.0f);
		audio_duelscene.volume = PlayerPrefs.GetFloat ("OPT_AUDIOBG", 1.0f);
		audio_soundeffect.volume = PlayerPrefs.GetFloat ("OPT_AUDIOEF", 1.0f);	
		audio_soundeffectloop.volume = PlayerPrefs.GetFloat ("OPT_AUDIOEF", 1.0f);
	}

	/// <summary>
	/// Adjusts the menu background volume.
	/// </summary>
	/// <param name="value">void</param>
	void AdjustMenuBackgroundVolume(float value) {
		audio_menubackground.volume = value;
		audio_duelscene.volume = value;
		SaveAudioSettings ();
	}

	/// <summary>
	/// Adjusts the sound effect volume.
	/// </summary>
	/// <param name="value">void</param>
	void AdjustSoundEffectVolume(float value) {
		audio_soundeffect.volume = value;
		audio_soundeffectloop.volume = value;
		SaveAudioSettings ();
	}

	#region Commands to Play Audio
	void PlayMainMenuAudio() {
		audio_menubackground.Play ();
	}

	void StopMainMenuAudio() {
		audio_menubackground.Stop ();
	}

	void PlayDuelAudio() {
		audio_duelscene.Play ();
	}

	void StopDuelAudio() {
		audio_duelscene.Stop ();
	}

	void PlayClickA() {
		audio_soundeffect.PlayOneShot (click_a);
	}

	void PlayClickB() {
		audio_soundeffect.PlayOneShot (click_b);
	}

	void PlayCardDraw() {
		audio_soundeffect.PlayOneShot (card_draw);
	}

	void PlayLPCounterAppears() {
		audio_soundeffect.PlayOneShot (lp_counterAppear);
	}

	void PlayLPCounterDownLoop() {
		audio_soundeffectloop.clip = lp_counterDownLoop;
		audio_soundeffectloop.loop = true;
		audio_soundeffectloop.Play ();
	}

	void StopLPCounterDownLoop() {
		audio_soundeffectloop.Stop ();
	}

	void PlayLPCounterStops() {
		audio_soundeffect.PlayOneShot (lp_counterStops);
	}

	void PlayLPCounterZero() {
		audio_soundeffect.PlayOneShot (lp_counterZero);
	}
		
	void PlayDuelCardSummon() {
		audio_soundeffect.PlayOneShot (duel_cardSummon);
	}

	void PlayDuelMonsterDestroyed() {
		audio_soundeffect.PlayOneShot (duel_monsterdestroyed);
	}

	void PlayDuelCardSet() {
		audio_soundeffect.PlayOneShot (duel_cardSet);
	}

	void PlayDuelCardActivate() {
		audio_soundeffect.PlayOneShot (duel_cardActivate);
	}

	void PlayDuelCoinToss() {
		audio_soundeffect.PlayOneShot (duel_coinToss);
	}

	void PlayDuelEndTurn() {
		audio_soundeffect.PlayOneShot (duel_endTurn);
	}

	void PlayDuelStartGame() {
		audio_soundeffect.PlayOneShot (duel_startGame);
	}

	void PlayDuelEndGame() {
		audio_soundeffect.PlayOneShot (duel_endGame);
	}

	void PlayDuelDirectHit() {
		audio_soundeffect.PlayOneShot (duel_directHit);
	}
	#endregion
}