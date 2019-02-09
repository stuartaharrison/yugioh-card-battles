/*
 * Author: Stuart Harrison 
 * Date: March 2016
 * Class for holding the Duel Options when in the duel scene. Controls how the
 * AI can behave and what Life points it should assign at the start. It is also
 * used to store the outcome of the duel!
*/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DuelSceneUIMenuScript : MonoBehaviour {

	public GameObject duel_SceneController;
	public GameObject ui_menuOverlay;
	public GameObject ui_cardDetailsOverlay;
	public GameObject ui_cardOptionsOverlay;
	public GameObject ui_cardScannerOverlay;
	//UI Buttons we need to control interactivity of
	public Button uibtn_summon;
	public Button uibtn_nextphase;
	public Button uibtn_endturn;
	public Button uibtn_surrender;
	//Debug buttons we need to control interactivity of
	public GameObject uibtn_debugShowHand;
	public GameObject uibtn_debugShowDeck;
	public GameObject uibtn_debugInstantWin;

	private DuelSceneScript duel_Scene;
	private GameObject duel_opponent;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;

	void Start () {
		//Get my GameManagers and Duel Scene Script
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		duel_opponent = GameObject.FindGameObjectWithTag ("DUEL_OPPONENT");
		duel_Scene = duel_SceneController.GetComponent<DuelSceneScript> ();

		//Check the privilages of the logged in account. Admin privilages will
		//display the debug/testing functionality buttons!
		if (glob_gamemanager.GetComponent<AccountController> ().
			LoggedInAccount.Privilages >= 0) {
			uibtn_debugShowHand.SetActive (true);
			uibtn_debugShowDeck.SetActive (true);
			uibtn_debugInstantWin.SetActive (true);
		}
	}

	void Update () {
		//Turns on/off interactions with certain player functions based
		//on whether it's his/her turn or not
		uibtn_summon.interactable = duel_Scene.isPlayerTurn;
		uibtn_nextphase.interactable = duel_Scene.isPlayerTurn;
		uibtn_endturn.interactable = duel_Scene.isPlayerTurn;
	}

	/// <summary>
	/// Buttons the show hide menu overlay.
	/// </summary>
	public void btn_ShowHideMenuOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		if (ui_cardDetailsOverlay.activeInHierarchy || 
			ui_cardOptionsOverlay.activeInHierarchy) {
			ui_cardDetailsOverlay.SetActive (false);
			ui_cardOptionsOverlay.SetActive (false);
			ui_menuOverlay.SetActive (false);
		}
		else {
			ui_menuOverlay.SetActive (!ui_menuOverlay.activeInHierarchy);
		}
	}

	/// <summary>
	/// Command to bring up the card scanner overlay that will allow the player
	/// to scan/play cards in the duel
	/// </summary>
	public void btn_PlayACard() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelSceneUIMenuScript: Player playing a card");
		ui_menuOverlay.SetActive (false);
		ui_cardDetailsOverlay.SetActive (false);
		ui_cardOptionsOverlay.SetActive (false);
		ui_cardScannerOverlay.SetActive (true);
	}

	/// <summary>
	/// Command to call the function to move onto the next phase in the turn
	/// </summary>
	public void btn_NextPhase() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelSceneUIMenuScript: Player changing phase");
		ui_menuOverlay.SetActive (false);
		ui_cardDetailsOverlay.SetActive (false);
		ui_cardOptionsOverlay.SetActive (false);
		duel_Scene.NextTurnPhase ();
	}

	/// <summary>
	/// Command that will call the method to end the players turn and start
	/// the AI turn coroutine
	/// </summary>
	public void btn_EndTurn() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelSceneUIMenuScript: Player ending their turn");
		ui_menuOverlay.SetActive (false);
		ui_cardDetailsOverlay.SetActive (false);
		ui_cardOptionsOverlay.SetActive (false);
		duel_Scene.EndPlayerTurn ();
	}

	/// <summary>
	/// Command that will set the PLayer's life points to 0, effectively
	/// surrending the game to his/opponent
	/// </summary>
	public void btn_SurrenderDuel() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("DuelSceneUIMenuScript: Player has begun surrendering");
		ui_menuOverlay.SetActive (false);
		ui_cardDetailsOverlay.SetActive (false);
		ui_cardOptionsOverlay.SetActive (false);
		duel_Scene.PlayerSurrender ();
	}

	/// <summary>
	/// Debug command that sends a message command to the AI object to output
	/// the AI's hand list into the console
	/// </summary>
	public void btn_DebugCheckAIHand() {
		glob_audiomanager.SendMessage ("PlayClickA");
		duel_opponent.SendMessage ("CommandRevealHand");
	}

	/// <summary>
	/// Debug command that sends a message command to the AI object to output
	/// the AI's deck list into the console
	/// </summary>
	public void btn_DebugCheckAIDeck() {
		glob_audiomanager.SendMessage ("PlayClickA");
		duel_opponent.SendMessage ("CommandRevealDeck");
	}
		
	/// <summary>
	/// Calls a function to reduce the AI's life point score to 0 and instantly
	/// win the duel/game
	/// </summary>
	public void btn_DebugInstantWin() {
		glob_audiomanager.SendMessage ("PlayClickA");
		duel_Scene.AISurrender ();
	}
}