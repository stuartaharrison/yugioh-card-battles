/*
 * Author: Stuart Harrison 
 * Date: January 2016
 * Script for handling all the logic with displaying and selecting an AI opponent
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SinglePlayerOverlayScript : MonoBehaviour {

	public GameObject[] ai_opponents;
	public GameObject duel_opponent_prefab;
	public GameObject ui_opponentselect;
	public GameObject ui_dueloptions;
	public GameObject ui_accountOverlay;
	public Toggle ui_toggleTraditional;
	public Toggle ui_toggleMonstersOnly;
	public Toggle ui_toggleNoEffect;

	private bool duelOpponentLoaded = false;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private GameObject selectedAI;

	void Start () {
		//Get our Gamemanagers
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");

		//Foreach AI assigned in Unity Editor
		foreach (GameObject ai in ai_opponents) {
			//Compare your account level with the AI level to unlock it
			AIDetails aiDet = ai.GetComponent<AIDetails> ();
			int myLevel = glob_gamemanager.GetComponent<AccountController> ().
				LoggedInAccount.Level;
			if (aiDet != null) {
				if (myLevel >= aiDet.ai_unlocklevel) {
					//If unlocked then we can display it in our player select
					ai.SetActive (true);
				}
			}
		}
	}

	void Update () {
		//When a duelOpponent has been selected and we have sucessfully created
		//and loaded our duel manager object we can move to the duel scene
		if (duelOpponentLoaded && 
			GameObject.FindGameObjectWithTag ("DUEL_OPPONENT") != null) {
			SceneManager.LoadScene ("duelscene");
		}
	}
		
	/// <summary>
	/// Command to close the overlay and reset all the components
	/// </summary>
	public void btn_CloseOverlay() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_accountOverlay.SetActive (true);
		this.gameObject.SetActive (false);
	}

	/// <summary>
	/// Command to select the appropriate AI and move the UI onto selecting
	/// the duel options
	/// </summary>
	/// <param name="opponent">Opponent.</param>
	public void btn_OpponentSelected(GameObject opponent) {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("SinglePlayerOverlayScript: Opponent: " + opponent.name);
		selectedAI = opponent;
		ui_opponentselect.SetActive (false);
		ui_dueloptions.SetActive (true);
	}

	/// <summary>
	/// Command to move back a menu in the Single Player overlay. So after
	/// the player has chosen an AI, they can go back and change the AI if they
	/// want
	/// </summary>
	public void btn_BackStep() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_opponentselect.SetActive (true);
		ui_dueloptions.SetActive (false);
	}

	/// <summary>
	/// Command to setup the duel opponent and all necessary components required
	/// for executing the AI correctly
	/// </summary>
	public void btn_Duel() {
		glob_audiomanager.SendMessage ("PlayClickA");
		//When this button is pressed we need too
		//1.Instatiate the Duel Opponent Controller
		//2.Pass all the options and AI details to the Controller
		//3.Load the Duel Scene
		if (GameObject.FindGameObjectWithTag ("DUEL_OPPONENT") == null) {
			Instantiate (duel_opponent_prefab, new Vector3 (0.0f, 0.0f, 0.0f), 
				Quaternion.identity);

			AIDetails dOpponent = GameObject.
				FindGameObjectWithTag ("DUEL_OPPONENT").
				GetComponent<AIDetails> ();
			DuelOptions duelOpts = GameObject.
				FindGameObjectWithTag ("DUEL_OPPONENT").
				GetComponent<DuelOptions> ();

			AIDetails sOpponent = selectedAI.GetComponent<AIDetails> ();
			//Assign everything to the new Duel opponent object
			dOpponent.ai_name = sOpponent.ai_name;
			dOpponent.ai_deckname = sOpponent.ai_deckname;
			dOpponent.ai_image = sOpponent.ai_image;
			dOpponent.ai_unlocklevel = sOpponent.ai_unlocklevel;
			dOpponent.ai_decklist = sOpponent.ai_decklist;

			dOpponent.ai_winExp = sOpponent.ai_winExp;
			dOpponent.ai_lossExp = sOpponent.ai_lossExp;
			dOpponent.ai_drawExp = sOpponent.ai_drawExp;

			dOpponent.speech_DuelStart = sOpponent.speech_DuelStart;
			dOpponent.speech_EndTurn = sOpponent.speech_EndTurn;
			dOpponent.speech_DrawCard = sOpponent.speech_DrawCard;
			dOpponent.speech_SummonCard = sOpponent.speech_SummonCard;
			dOpponent.speech_SetCard = sOpponent.speech_SetCard;
			dOpponent.speech_DirectAttack = sOpponent.speech_DirectAttack;
			dOpponent.speech_NormalAttack = sOpponent.speech_NormalAttack;

			duelOpts.duelopt_traditionalrules = ui_toggleTraditional.isOn;
			duelOpts.duelopt_monstersonly = ui_toggleMonstersOnly.isOn;
			duelOpts.duelopt_noeffectmonsters = ui_toggleNoEffect.isOn;

			duelOpponentLoaded = true;
		}
	}
}