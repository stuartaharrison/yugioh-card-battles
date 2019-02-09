/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controlling the UI within the LoginMenu Scene
*/

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoginMenuSceneScript : MonoBehaviour {

	public InputField ui_txtb_username;
	public InputField ui_txtb_password;

	private AccountController accountControl;
	private bool switch_Login = false;
	private GameObject glob_gamemanager;
	private GameObject glob_audiomanager;

	void Start () {
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		glob_audiomanager = GameObject.FindGameObjectWithTag ("audio_manager");

		if (glob_gamemanager != null) {
			accountControl = glob_gamemanager.GetComponent<AccountController> ();
		}

		if (accountControl != null) {
			//Since the login menu scene only appears when the use logs out of 
			//their account we can delete the key that holds the user/pass combo
			//for the auto login functionality
			accountControl.Logout();
			accountControl.ForgetLocalAccountInformation();
		}
	}

	void Update () {
		//accountControl can only be not null when we have successfully logged in
		//so therefore we can change the scene
		if (accountControl != null) {
			if (accountControl.LoggedInAccount != null) {
				SceneManager.LoadScene ("mainmenu");
			}
		}
		//When the button is pressed to try logging into the game
		if (switch_Login) {
			switch_Login = false;
			StartCoroutine (ILoginAttempt (ui_txtb_username.text + "|" + 
				ui_txtb_password.text));
		}
	}

	/// <summary>
	/// Command to start a login attempt
	/// </summary>
	public void btn_Login() {
		glob_audiomanager.SendMessage ("PlayClickA");
		if (accountControl != null) {
			switch_Login = true; //Used for the Update to start the coroutine
		}
		else {
			Debug.LogError ("LoginMenuSceneScript: accountControl not assigned");
		}
	}

	/// <summary>
	/// Coroutine for the login attempt/wait
	/// </summary>
	/// <returns>void</returns>
	/// <param name="loginData">Data to login (Format: USERNAME|PASSWORD)</param>
	IEnumerator ILoginAttempt(string loginData) {
		Debug.Log("LoginMenuSceneScript: login beginning");
		yield return StartCoroutine (accountControl.ILogin (loginData));
		//Wait for login to complete
		Debug.Log ("LoginMenuSceneScript: login completed");
		if (accountControl.LoggedInAccount != null) {
			//We have successfully logged in
			Debug.Log ("LoginMenuSceneScript: Login was a success");
		}
	}
}