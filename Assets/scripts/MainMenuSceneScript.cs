/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controlling the UI within the MainMenu Scene
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuSceneScript : MonoBehaviour {

	public GameObject ui_mainmenu;
	public GameObject ui_accountOverlay;
	public GameObject ui_singleplayer;
	public GameObject ui_scanneroverlay;
	public GameObject ui_optionsmenu;

	private GameObject glob_audiomanager;
	private GameObject glob_gamemanager;

	void Start () {
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
	}

	/// <summary>
	/// Command to bring up the UI to select your opponent
	/// </summary>
	public void btn_vsCpu() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_singleplayer.SetActive (true);
		ui_accountOverlay.SetActive (false);
	}

	/// <summary>
	/// Command to change scene to display the Networking menu
	/// </summary>
	public void btn_vsMultiplayer() {
		glob_audiomanager.SendMessage ("PlayClickA");
	}

	/// <summary>
	/// Command to bring up the card scanner overlay
	/// </summary>
	public void btn_ScanCard() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_scanneroverlay.SetActive (true);
		ui_accountOverlay.SetActive (false);
	}

	/// <summary>
	/// Command to bring up the options overlay
	/// </summary>
	public void btn_Options() {
		glob_audiomanager.SendMessage ("PlayClickA");
		ui_optionsmenu.SetActive (true);
		ui_accountOverlay.SetActive (false);
	}

	/// <summary>
	/// Command to logout of the account and switch to the loginscene
	/// </summary>
	public void btn_Logout() {
		glob_audiomanager.SendMessage ("PlayClickA");
		SceneManager.LoadScene ("loginmenu");
	}
}