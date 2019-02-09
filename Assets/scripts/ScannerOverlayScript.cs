/*
 * Author: Stuart Harrison 
 * Date: February 2016
 * Class for controlling and handling the card scanning functionality within
 * the main menu scene
*/

using com.google.zxing.qrcode;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Threading;

public class ScannerOverlayScript : MonoBehaviour {

	public GameObject ui_accountOverlay;
	public GameObject ui_panel_camera;
	public GameObject ui_panel_data;
	public Image ui_card_image;
	public RawImage cameraDisplay;
	public Text ui_card_text;

	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private object downloadResult = null;
	private ScannerScript qr_scanner = null;

	void Start () {
		//Get our Gamemanagers
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
	}

	void Update () {
		qr_scanner = gameObject.GetComponent<ScannerScript> ();
		if (qr_scanner != null) {
			cameraDisplay.texture = qr_scanner.CameraTexture;

			if (qr_scanner.Result != null) { //We got a QR code from the scanner
				Debug.Log("ScannerOverlayScript: Got result from QR Scanner " + 
					qr_scanner.Result);
				//Start the coroutine for downloading the card
				StartCoroutine(IDecodeCard((string)qr_scanner.Result));
				Destroy (qr_scanner);
			}
		}

		//Check if we have decoded/downloading some data from the cloud
		if (downloadResult != null) {
			Sprite displayPicture = null;
			string displayText = null;
			ui_card_text.color = Color.white;

			if (downloadResult is Monster) {
				Debug.Log ("ScannerOverlayScript: Downloaded a Monster Card");
				Debug.Log ("Monster Card: " + ((Monster)downloadResult).Name);
				displayPicture = ((Monster)downloadResult).Image;
				displayText = ((Monster)downloadResult).OutputCardDetails ();
			}
			else if (downloadResult is Card) {
				Debug.Log ("ScannerOverlayScript: Downloaded a Spell/Trap Card");
				Debug.Log ("Spell/Trap Card: " + ((Card)downloadResult).Name);
				displayPicture = ((Card)downloadResult).Image;
				displayText = ((Card)downloadResult).OutputCardDetails ();
			}
			else {
				Debug.LogWarning ("ScannerOverlayScript: Got something other " +
					"than what is normally gotten from the cloud");
				ui_card_text.color = Color.red;
				displayText = "Invalid Card Code...";
			}

			ui_card_image.sprite = displayPicture;
			ui_card_text.text = displayText;

			ui_panel_camera.SetActive (false);
			ui_panel_data.SetActive (true);

			downloadResult = null;
		}
	}

	/// <summary>
	/// Raises the enable event.
	/// </summary>
	void OnEnable() {
		if (qr_scanner == null) {
			gameObject.AddComponent<ScannerScript> ();
		}
	}

	/// <summary>
	/// Method that will cancel and close the entire card scanning overlay
	/// and components. Resets everything back
	/// </summary>
	public void btn_CloseOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		if (qr_scanner != null) {
			Destroy (qr_scanner);
		}
		qr_scanner = null;
		ui_panel_camera.SetActive (true);
		ui_panel_data.SetActive (false);
		ui_accountOverlay.SetActive (true);
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Takes the scanned in QRCode data and attempts to get a card value back
	/// from the webserver.
	/// </summary>
	/// <returns>void</returns>
	/// <param name="result">QRCode Scan Result Text (Card Code)</param>
	IEnumerator IDecodeCard(string result) {
		DownloadManager dlManager = new DownloadManager(this, result, false);
		yield return dlManager.Coroutine;
		downloadResult = dlManager.Result;
	}
}