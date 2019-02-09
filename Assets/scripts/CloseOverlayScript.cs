/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Script used to close down any overlay that appears infront of the game
 * to give the user some information. Replaces the need to include this code
 * in every UI overlay created within the game/application
*/

using UnityEngine;
using System.Collections;

public class CloseOverlayScript : MonoBehaviour {

	private GameObject glob_audiomanager;

	void Start () {
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
	}

	public void CloseThisOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("CloseOveryScript: Overlay closed..");
		this.gameObject.SetActive (false);
	}
}