/*
 * Author: Stuart Harrison 
 * Date: February 2016
 * Script for bringing up and handling the QRCode and card getting in the duel
 * scene/main part of the application
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelCardScannerScript : MonoBehaviour {

	//Open the Scanner Overlay
	//Scan the Card
	//If it's a Monster then decide it it's in Attack or Defence
	//If it's a Spell or Trap card, set or activate it

	public GameObject duelSceneController;
	public GameObject ui_scanner;
	public GameObject ui_monsterPanel;
	public GameObject ui_spellPanel;
	public Image img_MonsterAttack;
	public Image img_MonsterDefence;
	public Image img_SpellActive;
	public RawImage ui_cameraDisplay;
	public Text ui_lblMonsterName;
	public Text ui_lblSpellName;

	private DuelSceneScript duelController;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private object downloadResult = null;
	private ScannerScript qr_scanner = null;

	//Test variables (April 2016 - Do not touch right now!)
	public bool isSetting = false;
	public bool isSpellCard = false;
	public object temp;

	void Start () {
		//Get our Gamemanagers and duel controller
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		duelController = duelSceneController.GetComponent<DuelSceneScript> ();
	}

	void Update () {
		//Check to see if the QRCode Scanner is running
		qr_scanner = gameObject.GetComponent<ScannerScript> ();
		if (qr_scanner != null) { //If it is running
			//Assign the camera input to the correct UI object
			ui_cameraDisplay.texture = qr_scanner.CameraTexture;

			if (qr_scanner.Result != null) { //We got a QR code from the scanner
				Debug.Log("ScannerOverlayScript: Got result from QR Scanner " + 
					qr_scanner.Result);
				//Start the coroutine for downloading the card
				StartCoroutine(IDecodeCard((string)qr_scanner.Result));
				Destroy (qr_scanner);
			}
		}

		//Check to see what we have recieved from the download manager
		if (downloadResult != null) {
			if (downloadResult is Monster) {
				Debug.Log ("DuelCardScannerScript: Got a monster card");
				Debug.Log ("Monster Card: " + ((Monster)downloadResult).Name);
				img_MonsterAttack.sprite = ((Monster)downloadResult).Image;
				img_MonsterDefence.sprite = ((Monster)downloadResult).Image;
				ui_lblMonsterName.text = ((Monster)downloadResult).Name;
				ui_scanner.SetActive (false);
				ui_monsterPanel.SetActive (true);
				temp = (Monster)downloadResult;
			}
			else if (downloadResult is Card) {
				Debug.Log ("DuelCardScannerScript: Got a spell card");
				Debug.Log ("Spell/Trap Card: " + ((Card)downloadResult).Name);
				img_SpellActive.sprite = ((Card)downloadResult).Image;
				ui_lblSpellName.text = ((Card)downloadResult).Name;
				ui_scanner.SetActive (false);
				ui_spellPanel.SetActive (true);
				temp = (Card)downloadResult;
			}
			else {
				Debug.LogWarning ("DuelCardScannerScript: Did not find a card");
				glob_gamemanager.SendMessage ("DisplayErrorMessage", 
					"Could not get the card data");
			}
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
	/// Closes the game object safely. Shuts down the camera and decoding
	/// thread
	/// </summary>
	private void CloseSafely() {
		if (qr_scanner != null) { //If we have the QRCode scanner running
			//destroy it!
			Destroy (qr_scanner);
		}
		//Reset all variables and displays
		qr_scanner = null;
		ui_scanner.SetActive (true);
		ui_monsterPanel.SetActive (false);
		ui_spellPanel.SetActive (false);
		gameObject.SetActive (false); //Hide this object nows
	}

	/// <summary>
	/// Checks to see if there is any space available on the Players monster
	/// zones.
	/// </summary>
	/// <returns><c>true</c>, if monster zones full are full, 
	/// <c>false</c> otherwise.</returns>
	private bool isMonsterZonesFull() {
		bool flag = true;
		foreach (DuelMonsterZoneScript zone in 
			duelController.ui_playerMonsterZones) {
			if (zone.ZoneCard == null) {
				flag = false;
			}
			if (!flag) { break; }
		}
		return flag;
	}

	/// <summary>
	/// Method for getting the first avaialble Monster Zone of the players
	/// field
	/// </summary>
	/// <returns>The empty monster zone script</returns>
	private DuelMonsterZoneScript getEmptyMonsterZone() {
		DuelMonsterZoneScript returnScript = null;
		if (!isMonsterZonesFull ()) { //No point continuing if the zones are full
			foreach (DuelMonsterZoneScript zone in 
				duelController.ui_playerMonsterZones) {
				if (zone.ZoneCard == null) {
					returnScript = zone;
					break;
				}
			}
		}
		return returnScript;
	}

	/// <summary>
	/// Method that will cancel and close the entire card scanning overlay
	/// and components. Resets everything back
	/// </summary>
	public void btn_CloseOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		CloseSafely ();
	}

	/// <summary>
	/// Command executed when the UI button in the Monster position selection
	/// menu is pressed. Depending if facedown is selected or not will
	/// depend on how the game places the card onto the screen/field
	/// </summary>
	/// <param name="facedown">If set to <c>true</c> facedown.</param>
	public void btn_PlaceMonsterCard(bool facedown) {
		glob_audiomanager.SendMessage ("PlayClickB");
		if (temp != null && temp is Monster) { //Only do this if it is a monster!
			Monster monster = (Monster)temp;
			DuelMonsterZoneScript dz = getEmptyMonsterZone (); //Get empty zone
			//to assign this card to
			if (dz != null) {
				Debug.Log ("DuelCardScannerScript: Player summoning " +
					monster.Name);
				//Play the animation/sequence for displaying the card
				dz.PlaceCardOnZone (monster, facedown);
				CloseSafely ();
			}
			else {
				Debug.Log ("DuelCardScannerScript: Monster Zones full");
				//Display error message
			}
		}
	}

	/// <summary>
	/// Command executed when the UI button in the Spell/Trap selection menu
	/// is pressed. If it is not facedown then we need to execute the effect
	/// of the spell/trap card, otherwise we place it facedown in the first
	/// available spell/trap zone
	/// TODO: Finish
	/// </summary>
	/// <param name="facedown">If set to <c>true</c> facedown.</param>
	public void btn_PlaceSpellCard(bool facedown) {
		glob_audiomanager.SendMessage ("PlayClickB");
		if (temp != null && temp is Card) {

		}
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