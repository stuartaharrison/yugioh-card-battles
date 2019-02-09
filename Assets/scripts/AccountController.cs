/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for storing and handling the players account information
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AccountController : MonoBehaviour {

	public Account LoggedInAccount;
	public Coroutine loginCoroutine;
	public float timeoutTimer = 60.0f;
	public GUISkin guiSkin;
	public string loginToRemember = null;

	private bool hasLeveledUp = false;
	private bool switch_Login = false;
	private bool switch_UpdateAccount = false;
	private DownloadManager dlManager = null;
	private float timer_timeout;
	private string loginData;

	//Called when the object first starts
	void Start () {
		loginToRemember = PlayerPrefs.GetString ("LOG_REM", null);
	}
		
	//Called once per frame
	void Update () {
		//Check to see if we need to send an update request to the server
		if (switch_UpdateAccount) {
			switch_UpdateAccount = false;
			StartCoroutine ("IUpdateAccount");
		}
		//If we are loggining into the player account
		if (switch_Login) {
			timeoutTimer -= 1 * Time.deltaTime; //Drop the timeout by 1 second
			if (timeoutTimer <= 0) { //If we reach the end of the counter
				//Log out a message and then cancel any coroutines that are
				//running and display a timeout message to the player.
				Debug.LogWarning ("AccountController: Login timeout reached");
				gameObject.SendMessage ("DisplayErrorMessage", 
					"Failed to login. Reason: Timeout reached");
				CancelLogin ();
			}
		}
	}

	/// <summary>
	/// Used to show a temporary box with message to alert the user to the idea
	/// that the game is getting their account from the server
	/// </summary>
	void OnGUI() {
		GUI.skin = guiSkin;
		if (switch_Login) { //Only display whilst we are 'logging in'
			string tempText = "Please wait while we log into your account.," +
				"(Timeout: " + timeoutTimer.ToString() + "s)";
			tempText = tempText.Replace (",", "\n"); //Put a new line so that
			//the counter is below the message
			//Create a box across the entire screen to darken the 'background'
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			//Then make the error message label show
			GUI.color = Color.white; //Change the colour back from black with
			//an opacity to white
			GUI.Label (new Rect ((Screen.width / 2) - 225, Screen.height / 2, 
				550, 100), tempText);
		}
		//Temporary code for displaying a happy You have levelled up message!
		if (hasLeveledUp) {
			GUI.Box (new Rect (0, 0, Screen.width, Screen.height), "");
			GUI.color = Color.white;
			GUI.Label (new Rect ((Screen.width / 2) - 225, Screen.height / 2, 
				550, 100), "You have levelled up! \n" +
				"Congratulation you are now level " + 
				LoggedInAccount.Level.ToString());
			if (GUI.Button (new Rect ((Screen.width / 2) - 75, 
				(Screen.height / 2 + 75), 150, 50), "Thanks!")) {
				hasLeveledUp = false; //close the overlay
			}
		}
	}

	/// <summary>
	/// Stops all coroutines that handle the logging in or the players account
	/// and 'resets' all switches to make another login attempt
	/// </summary>
	void CancelLogin() {
		Debug.Log ("AccountController: Cancelling login attempt");
		switch_Login = false;
		switch_UpdateAccount = false;
		StopAllCoroutines ();
		dlManager = null;
	}

	/// <summary>
	/// Adds a win and experience to the players account
	/// </summary>
	/// <returns><c>true</c>If the player has levelled up from the exp gain. 
	/// <c>false</c>False if the player account did not level up</returns>
	/// <param name="exp">Exp.</param>
	public bool AddWin(int exp) {
		bool hasLeveledUp = false; //acts as a flag
		if (LoggedInAccount != null) { //Can only do it if we are logged in,
			//safety check!
			LoggedInAccount.AddWin (); //Add the win
			hasLeveledUp = LoggedInAccount.AddExp (exp); //Add the exp
		}
		switch_UpdateAccount = true;
		this.hasLeveledUp = hasLeveledUp;
		return hasLeveledUp;
	}

	/// <summary>
	/// Adds a loss and experience to the players account
	/// </summary>
	/// <returns><c>true</c>If the player has levelled up from the exp gain. 
	/// <c>false</c>False if the player account did not level up</returns>
	/// <param name="exp">Exp.</param>
	public bool AddLoss(int exp) {
		bool hasLeveledUp = false;
		if (LoggedInAccount != null) {
			LoggedInAccount.AddLoss ();
			hasLeveledUp = LoggedInAccount.AddExp (exp);
		}
		switch_UpdateAccount = true;
		this.hasLeveledUp = hasLeveledUp;
		return hasLeveledUp;
	}

	/// <summary>
	/// Adds a draw and experience to the players account
	/// </summary>
	/// <returns><c>true</c>If the player has levelled up from the exp gain. 
	/// <c>false</c>False if the player account did not level up</returns>
	/// <param name="exp">Exp.</param>
	public bool AddDraw(int exp) {
		bool hasLeveledUp = false;
		if (LoggedInAccount != null) {
			LoggedInAccount.AddDraw ();
			hasLeveledUp = LoggedInAccount.AddExp (exp);
		}
		switch_UpdateAccount = true;
		this.hasLeveledUp = hasLeveledUp;
		return hasLeveledUp;
	}

	/// <summary>
	/// Logout of the currently logged in account. Will just set the main
	/// variable to null but that prevents scene changing within the 
	/// login menu scenes
	/// </summary>
	public void Logout() {
		//TODO: Handle some Logout logic, such as saving. Likely needs to be
		//within a coroutine
		LoggedInAccount = null;
	}

	/// <summary>
	/// Saves the last login information (username & password) to the player 
	/// prefs
	/// </summary>
	/// <param name="loginToSave">Login to save 
	/// (format: USERNAME|PASSWORD)</param>
	public void SaveLocalAccountInformation(string loginToSave) {
		PlayerPrefs.SetString ("LOG_REM", loginToSave);
		PlayerPrefs.Save ();
	}

	/// <summary>
	/// Deletes the last login information from the player prefs
	/// </summary>
	public void ForgetLocalAccountInformation() {
		PlayerPrefs.DeleteKey ("LOG_REM");
		PlayerPrefs.Save ();
	}

	/// <summary>
	/// Creates an instance of the download manager to send a message to the
	/// webserver with login details in an attempt create and get a new
	/// account object that represents the players account that they just tried
	/// to log into
	/// </summary>
	/// <returns>The account information.</returns>
	/// <param name="loginData">Login data (Format: USERNAME|PASSWORD)</param>
	public IEnumerator ILogin(string loginData) {
		timer_timeout = timeoutTimer;
		switch_Login = true;
		dlManager = new DownloadManager (this, loginData, true);
		yield return dlManager.Coroutine;
		//We have logged into the account
		if (dlManager.Result is Account) {
			Debug.Log ("AccountController: Successfully logged into account " + 
				"with username: " + ((Account)dlManager.Result).Username);
			LoggedInAccount = (Account)dlManager.Result;
			//Save the account login information for instant login next time the
			//game is started
			SaveLocalAccountInformation (loginData);
		}
		else { //We got an error message that needs to be displayed
			Debug.LogWarning("AccountController: Login failed with message " + 
				(string)dlManager.Result);
			gameObject.SendMessage ("DisplayErrorMessage", 
				(string)dlManager.Result);
		}
		switch_Login = false;
	}

	/// <summary>
	/// Creates an instance of the download manager and sends the basic account
	/// information back to the server for it to update the database on the
	/// server side
	/// </summary>
	private IEnumerator IUpdateAccount() {
		dlManager = new DownloadManager (this, LoggedInAccount);
		yield return dlManager.Coroutine;
	}
}