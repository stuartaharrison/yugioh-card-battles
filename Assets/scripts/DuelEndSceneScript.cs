/*
 * Author: Stuart Harrison 
 * Date: March 2016
 * Script for handling the scene after the duel has concluded. Such as updating
 * the player account and display the results back out to the Player
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DuelEndSceneScript : MonoBehaviour {

	public Image ui_img_playerDisplay;
	public Image ui_img_aiDisplay;
	public Text ui_lbl_playerName;
	public Text ui_lbl_aiName;
	public Text ui_lbl_result;
	public Text ui_lbl_rewards;

	private AccountController accountControl;
	private AIDetails ai_details;
	private bool hasDrawnGame = false;
	private bool hasPlayerLost = false;
	private bool hasLeveledUp = false;
	private DuelOptions duel_opts;
	private GameObject duel_opponent;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private string rewardsText = "Rewards:,";

	void Start () {
		//Get the necessary Managers
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
		//Get the information on the Duel Opponent/AI
		duel_opponent = GameObject.FindGameObjectWithTag ("DUEL_OPPONENT");
		//Get player Account details
		accountControl = glob_gamemanager.GetComponent<AccountController> ();
		//Get the AI details
		ai_details = duel_opponent.GetComponent<AIDetails> ();
		duel_opts = duel_opponent.GetComponent<DuelOptions> ();
		//Determine if the Player Lost,Won,or Drew the game
		hasPlayerLost = duel_opts.hasPlayerLost;
		hasDrawnGame = duel_opts.hasDrawnGame;
		//Assign the appropriate amount of EXP
		if (hasDrawnGame) {
			hasLeveledUp = accountControl.AddDraw (ai_details.ai_drawExp);
			rewardsText += ai_details.ai_drawExp.ToString() + "xp,";
		}
		else {
			if (hasPlayerLost) {
				hasLeveledUp = accountControl.AddLoss (ai_details.ai_lossExp);
				rewardsText += ai_details.ai_lossExp.ToString() + "xp,";
			}
			else {
				hasLeveledUp = accountControl.AddWin (ai_details.ai_winExp);
				rewardsText += ai_details.ai_winExp.ToString() + "xp,";
			}
		}
		//Set the rewards text and place the new lines where assigned
		rewardsText = rewardsText.Replace (",", "\n");
		//Assign player and AI profile pictures
		ui_img_playerDisplay.sprite = 
			accountControl.LoggedInAccount.DisplayPicture;
		ui_img_aiDisplay.sprite = ai_details.ai_image;
		//Assign player and AI names to the UI
		ui_lbl_playerName.text = accountControl.LoggedInAccount.Username;
		ui_lbl_aiName.text = ai_details.ai_name;
		//Assign the rewards the player recieved to the UI
		ui_lbl_rewards.text = rewardsText;
		//Output the Result to the UI
		if (hasPlayerLost) {
			ui_lbl_result.color = Color.red;
			ui_lbl_result.text = "Result: Lost";
		}
		else {
			ui_lbl_result.color = Color.green;
			ui_lbl_result.text = "Result: Win";
		}

		Destroy (duel_opponent); //Dump the old Duel Opponent object
		//A new one is made at each new Duel, so it's okay
		//Stop the Duel music and go back to the MainMenu audio
		glob_audiomanager.SendMessage ("StopDuelAudio");
		glob_audiomanager.SendMessage ("PlayMainMenuAudio");
	}
		
	/// <summary>
	/// Command to move to the MainMenuScene once the player is satisified and
	/// ready to move on
	/// </summary>
	public void btn_BackToMainMenu() {
		glob_audiomanager.SendMessage ("PlayClickA");
		SceneManager.LoadScene ("mainmenu");
	}
}