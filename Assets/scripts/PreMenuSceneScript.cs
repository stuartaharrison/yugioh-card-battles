/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controlling the UI within the PreMenu Scene
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PreMenuSceneScript : MonoBehaviour {

	public GameObject ui_terms;
	public GameObject ui_loading;
	public GameObject ui_loadingbar;
	public GameObject ui_login;
	public GameObject ui_overlay_login;
	public GameObject ui_overlay_loginerror;
	public GameObject ui_lbl_timeoutCounter;
	public GameObject ui_lbl_errormessage;
	public GameObject pref_gamemanager;
	public InputField ui_txtb_username;
	public InputField ui_txtb_password;

	private AccountController accountControl;
	private bool switch_LoadGamedata = false;
	private bool switch_LoadedGamedata = false;
	private bool switch_Login = false;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;
	private string username;
	private string password;

	void Start () {
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");
	}

	void Update () {
		//Check for the input into the textboxes for the users login information
		if (ui_login.activeInHierarchy) {
			username = ui_txtb_username.text;
			password = ui_txtb_password.text;
		}

		if (accountControl != null) {
			if (switch_LoadedGamedata && 
				accountControl.LoggedInAccount != null) {
				//Ready to move to the MainMenu
				SceneManager.LoadScene ("mainmenu");
			}
		}

		if (switch_LoadGamedata) { 
			//Starts once the terms button has been pressed
			//Reset back to false to prevent new Coroutines constantly starting
			switch_LoadGamedata = false;
			StartCoroutine (ISetupGameData ());
		}

		if (switch_LoadedGamedata && switch_Login) {
			//Starts when the user clicks the Login button
			switch_Login = false;
			StartCoroutine (ILoginAttempt (username + "|" + password));
		}
	}

	/// <summary>
	/// Command that starts the loading of the game data that is needed in
	/// order for the application to run correctly
	/// </summary>
	public void btn_OkayToTerms() {
		glob_audiomanager.SendMessage ("PlayClickB");
		Debug.Log ("PreMenuSceneScript: User has accepted terms..");
		switch_LoadGamedata = true; //Start loading game data in the Update
	}

	/// <summary>
	/// Command to start a login attempt for the game
	/// </summary>
	public void btn_Login() {
		glob_audiomanager.SendMessage ("PlayClickA");
		Debug.Log ("PreMenuSceneScript: Login button clicked. Begin Login..");
		switch_Login = true;
	}

	/// <summary>
	/// Coroutine for the login attempt/wait
	/// </summary>
	/// <returns>void</returns>
	/// <param name="loginData">Data to login (Format: USERNAME|PASSWORD)</param>
	IEnumerator ILoginAttempt(string loginData) {
		Debug.Log("PreMenuSceneScript: Auto-login beginning");
		yield return StartCoroutine (accountControl.ILogin (loginData));
		//Wait for login to complete
		Debug.Log ("PreMenuSceneScript: Auto-login completed");
		if (accountControl.LoggedInAccount != null) {
			//We have successfully logged in
			Debug.Log ("PreMenuSceneScript: Login was a success");
		}
	}

	/// <summary>
	/// Sets up the game data
	/// </summary>
	/// <returns>void</returns>
	IEnumerator ISetupGameData() {
		Debug.Log ("PreMenuSceneScript: Starting to load Gamedata");
		yield return new WaitForSeconds(1);
		//Play the MainMenu Audio Music now
		glob_audiomanager.SendMessage ("PlayMainMenuAudio");
		//Change the UI scenes to the loading
		ui_terms.SetActive (false);
		ui_loading.SetActive (true);
		//Begin loading of the game data
		bool autoLogin = false;
		RectTransform loadbar = ui_loadingbar.GetComponent<RectTransform> ();
		string remDetails = "";
		if (ui_loadingbar != null) {
			#region Loading Sequence 1
			//First pass we want to load the Gamemanager object and necceseties
			Instantiate(pref_gamemanager, 
				new Vector3(0, 0, 0), Quaternion.identity);
			glob_gamemanager = GameObject.FindGameObjectWithTag("GameManager");
			accountControl = glob_gamemanager.GetComponent<AccountController> ();
			yield return new WaitForSeconds (1);
			float x = 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 2
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 3
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 4
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 5
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 6
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 7
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 8
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 9
			yield return new WaitForSeconds (0.5f);
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
			#region Loading Sequence 10
			//Final sequence is to login to the game/check we can login
			//Checks and assigns the auto Login variable
			remDetails = accountControl.loginToRemember;
			autoLogin = (remDetails == null || remDetails == "") ? false : true;
			x += 0.1f;
			loadbar.localScale = new Vector3 (x, 1.0f, 1.0f);
			#endregion
		}
		//Actually do the logging in or display of the login panel
		if (autoLogin) {
			//Send off request to our newly created gamemanager
			//Connects to PHP scripts in the 'cloud'
			//Returns the status of the login attempt
			Debug.Log("PreMenuSceneScript: Auto-login beginning");
			yield return StartCoroutine (accountControl.ILogin (remDetails));
			//Wait for login to complete
			Debug.Log ("PreMenuSceneScript: Auto-login completed");
			if (accountControl.LoggedInAccount != null) {
				//We have successfully logged in
				Debug.Log ("PreMenuSceneScript: Login was a success");
			}
		}
		else {
			//The Auto-Login functionality is not on so we need to show
			//a login panel box to allow user to type in user/pass combo
			Debug.Log("PreMenuSceneScript: Auto-login not on.. " +
				"displaying menu");
			ui_loading.SetActive (false);
			ui_login.SetActive (true);
		}
		//Output to the console what is happening
		Debug.Log("PreMenuSceneScript: Finished loading Gamedata");
		switch_LoadedGamedata = true; //We have completed the data load
	}
}