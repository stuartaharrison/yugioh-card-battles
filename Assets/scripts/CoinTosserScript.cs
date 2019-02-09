/*
 * Author: Stuart Harrison 
 * Date: April 2016
 * Script for performing a coin toss scenario/enviroment. Can be used at the
 * start of a duel to determine who goes first or modified to work with a monster
 * effect that involves coin tosses at a later date
*/
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CoinTosserScript : MonoBehaviour {

	public GameObject ui_panelA; //Picking heads or tails w/Result
	public GameObject ui_panelB; //Letting user pick, which turn they want
	public GameObject ui_paenlC; //Displays the AI result
	public GameObject duelController;
	public Text ui_resultText;
	public Text ui_aiTurnChoice;

	private bool selectedHeads = false;
	private bool isTossingCoin = false;
	private DuelSceneScript duelControlScript;
	private GameObject glob_audiomanager;

	void Start () {
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		duelControlScript = duelController.GetComponent<DuelSceneScript> ();
	}

	void Update () {
		//If we want to toss the coin, normally called whenever the player
		//clicks on the Heads or Tails button
		if (isTossingCoin) {
			isTossingCoin = false;
			StartCoroutine (ICoinToss ());
		}
	}

	public void btn_SelectHeads() {
		Debug.Log ("CoinTosser: Player selected Heads");
		selectedHeads = true;
		isTossingCoin = true;
	}

	public void btn_SelectTails() {
		Debug.Log ("CoinTosser: Player selected Tails");
		selectedHeads = false;
		isTossingCoin = true;
	}

	public void btn_SelectFirst() {
		Debug.Log ("CoinTosser: Player selected to go First");
		glob_audiomanager.SendMessage ("PlayClickA");
		duelControlScript.PlayerTurnFirst ();
		gameObject.SetActive (false);
	}

	public void btn_SelectSecond() {
		Debug.Log ("CoinTosser: Player selected to go Second");
		glob_audiomanager.SendMessage ("PlayClickA");
		duelControlScript.AITurnFirst ();
		gameObject.SetActive (false);
	}

	/// <summary>
	/// Simulates a CoinToss within the Duel
	/// </summary>
	/// <returns>void</returns>
	public IEnumerator ICoinToss() {
		System.Random rand = new System.Random ();
		//Create a random object and get a value of 0 or 1
		int n = rand.Next (0, 1);
		glob_audiomanager.SendMessage ("PlayDuelCoinToss");
		//Play coin toss audio
		yield return new WaitForSeconds (0.5f);
		if (n == 1) { //1 == heads
			//Heads
			Debug.Log ("CoinTosser: Result Heads");
			ui_resultText.text = "Result: Heads";
		}
		else { //Otherwise we asume its 0 and therefore false
			//Tails
			Debug.Log ("CoinTosser: Result Tails");
			ui_resultText.text = "Result: Tails";
		}
		yield return new WaitForSeconds (1.0f);
		if (n == 1 && selectedHeads) {
			//Selected Heads
			ui_panelA.SetActive (false);
			ui_panelB.SetActive (true);
		}
		else if (n == 0 && !selectedHeads) {
			//Selected Tails
			ui_panelA.SetActive (false);
			ui_panelB.SetActive (true);
		}
		else {
			ui_panelA.SetActive (false);
			ui_paenlC.SetActive (true);
			yield return new WaitForSeconds (2.0f);
			//AI gets to pick because player made wrong choice
			int a = rand.Next (0, 1);
			if (a == 1) {
				//First to go
				Debug.Log ("CoinTosser: AI selected to go First");
				ui_aiTurnChoice.text = "I'll take the first turn";
			}
			else {
				//Otherwise second to go
				Debug.Log ("CoinTosser: AI selected to go Second");
				ui_aiTurnChoice.text = "I'll take the second turn";
			}
			yield return new WaitForSeconds (2.0f);
			ui_paenlC.SetActive (false);
			//Depending on the AI's decision, start the AI or Player on the
			//first turn
			if (a == 1) {
				duelControlScript.AITurnFirst ();
			}
			else {
				duelControlScript.PlayerTurnFirst ();
			}
			gameObject.SetActive (false);
		}
	}
}