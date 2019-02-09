/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for handling connection to the web server/cloud storage space
 * Gets & Saves account and card data used within the game
*/

using UnityEngine;
using System.Collections;
using System.Threading;

public class DownloadManager {

	//Switch for determining if an error has occured
	private bool hasError = false;
	private bool downloadLoop = true;
	private Coroutine coroutine;
	private float downloadTimer = 0.0f;
	private MonoBehaviour classOwner;
	private object result; //The yielded return result from the coroutine
	//Webhost Locations
	private string web_host = "http://stuart-harrison.com/CardBattles/game.php";
	private string web_hostroot = "http://stuart-harrison.com";
	private Thread downloadTimerThread;

    public Coroutine Coroutine { get { return coroutine; } }
    public object Result { get { return result; } }

	/// <summary>
	/// Initializes a new instance of the <see cref="DownloadManager"/> class.
	/// used for updating the players Account
	/// </summary>
	/// <param name="owner">The MonoBehaviour class that created the
	/// instance of this class</param>
	/// <param name="accountToUpdate">Account to update</param>
	public DownloadManager(MonoBehaviour owner, Account accountToUpdate) {
		classOwner = owner;
		coroutine = classOwner.StartCoroutine (IUpdateAccount (accountToUpdate));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DownloadManager"/> class. 
	/// used for getting either the players Account information (logging in) or
	/// downloading a scanned in card code
	/// </summary>
	/// <param name="owner">The MonoBehaviour class that created the
	/// instance of this class</param>
	/// <param name="data">The card code or account login information</param>
	/// <param name="login">If set to <c>true</c> login.</param>
    public DownloadManager(MonoBehaviour owner, string data, bool login) {
		classOwner = owner;
		if (login) {
			coroutine = classOwner.StartCoroutine(IGetAccountData(data));
		}
		else {
			coroutine = classOwner.StartCoroutine(IGetCardData(data));
		}
    }

	/// <summary>
	/// Starts and keeps track of the time it takes for a coroutine to finish
	/// and the data to be 'downloaded'
	/// </summary>
	void DownloadTimerThread() {
		downloadLoop = true;
		while (downloadLoop) {
			Thread.Sleep (100); //Wait a milisecond
			downloadTimer += 0.1f; //Increment by 1 milisecond
		}
		Debug.Log ("DownloadManager: Action completed in " + 
			downloadTimer.ToString() + " seconds"); //Output to the console
		//how long it took
	}

	/// <summary>
	/// Is the get account data, providing the Login information is correct
	/// otherwise you get an error message back from the web server
	/// </summary>
	/// <returns>void</returns>
	/// <param name="dataToSend>Login data to send</param>
	IEnumerator IGetAccountData(string dataToSend) {
		downloadTimerThread = new Thread (DownloadTimerThread);
		downloadTimerThread.Start ();
		//Setup our request objects
		WWW getAccount = null;
		WWW getImage = null;
		//First thing is we need to split the data that is being passed in
		//because we cannot have anymore than 1 parameter in our IEnumerator
		string[] dataSplit = dataToSend.Split('|');
		//I can safely assume the data being split here
		//Create my POST data to send to the web-server
		WWWForm accountForm = new WWWForm();
		accountForm.AddField("GAME", "LOGIN");
		accountForm.AddField("CARDUSER", dataSplit[0]);
		accountForm.AddField("CARDPASS", dataSplit[1]);

		//I need to create my URL and assign my form to a new WWW object
		getAccount = new WWW(web_host, accountForm);
		yield return getAccount; //Wait for a response from the server

		if (getAccount.error != null) {
			//We have hit an error with our request, output it to the debuglog
			Debug.LogError(getAccount.error);
			hasError = true;
		}
		else {
			//Assume we have got a readied response from the web-server
			//Example: ERROR|Error: Could not retrieve the card information
			//Where ERROR; we are looking for ACCOUNT. 
			//But we still want to log the instance of an error.
			Debug.Log("DownloadManager: Result: " + getAccount.text);
			string[] resultSplit = getAccount.text.Split('|');
			//Once again, we know where this is coming from we can assume 
			//that position 0 has the data we need.
			if (resultSplit[0] == "ERROR") {
				Debug.LogError(resultSplit[1]);
				hasError = true;
			}
			else if (resultSplit[0] == "ACCOUNT") { //The one we want!
				//Account being returned also yields another argument
				//Accounts can be banned, or 
				//the user has input the wrong username/password combination
				switch(resultSplit[1]) {
				case "BANNED": //The Account being logged into is banned
					Debug.LogWarning("Attempt to login to a banned account");
					result = resultSplit[2]; //Message to explain ban
					break;
				case "WRONGCOMBO": //The Account login combination is wrong
					Debug.LogWarning(
						"Attempt to login with the wrong " +
						"Username/Password Combination");
					result = "Wrong Username/Password combination";
					break;
				default: //We can assume that we are ok to login
					Debug.Log("Login Attempt Successfull, " +
						"processing new Account object");
					//We also need to download the profile picture
					//I know that part of the URL is in position (8)
					string imageURL = web_hostroot + resultSplit[8];
					getImage = new WWW(imageURL);
					//Wait for the image request to complete
					yield return getImage;

					if (getImage.error != null) {
						Debug.LogError("Failed to collect profile picture.");
						hasError = true;
					}
					else {
						//Only begin creating the account object if no error 
						//has been caught previously
						if (!hasError) {
							Account newAccount;
							Texture2D profilePicture = getImage.texture;

							//Parse all int calues, ect before creating object
							//Mostly to fit the code onto an A4 page in terms
							//of the width!
							int id = int.Parse (resultSplit [1]);
							int level = int.Parse (resultSplit [3]);
							int exp = int.Parse (resultSplit [4]);
							int wins = int.Parse (resultSplit [5]);
							int loss = int.Parse (resultSplit [6]);
							int draw = int.Parse (resultSplit [7]);
							int privilage = int.Parse (resultSplit [10]);
							//Finally we can create the account object
							newAccount = new Account (id, resultSplit [2], level,
								exp, wins, loss, draw, profilePicture, privilage);
							//Assign the result of the function to the new
							//account object
							result = newAccount;
						}
					}
					break;
				}
				//Time to do some cleanup
				if (getAccount != null) { getAccount.Dispose(); }
				if (getImage != null) { getImage.Dispose(); }
				//Finally return the result we got from the above switch statement
				yield return result;
			}
			else { //No idea what this is!?
				Debug.LogError("Unknown Response: " + getAccount.text);
				//Set hasError to true because we did not get the data we wanted
				hasError = true;
			}
		}
		downloadLoop = false;
		downloadTimerThread.Join ();
    }

	/// <summary>
	/// Method for downloading the card data from the cloud including image
	/// </summary>
	/// <returns>void</returns>
	/// <param name="cardCode">Card code</param>
    IEnumerator IGetCardData(string cardCode) {
		//Start the timer
		downloadTimerThread = new Thread (DownloadTimerThread);
		downloadTimerThread.Start ();
		//Setup my form and assign the data we need
		WWW getCard = null;
		WWW getImage = null;
		WWWForm cardForm = new WWWForm();
		cardForm.AddField("GAME", "GETCARD");
		cardForm.AddField("CARDCODE", cardCode);
		//Send off the form request as a POST request to the webserver
		getCard = new WWW(web_host, cardForm);
		yield return getCard; //Wait for a response from the server

		if (getCard.error != null) {
			//We have hit an error with our request, output it to the debuglog
			Debug.LogError(getCard.error);
			hasError = true;
		}
		else {
			//Did not get an error, which means we got something
			Debug.Log("Here is the Result: " + getCard.text);
			string[] cardResultSplit = getCard.text.Split('|');
			//Split up the result text and find out what we got back
			if (cardResultSplit [0] == "ERROR") {
				//Error on the server side PHP script or database query,
				//quite possible that the card code was invalid and no card
				//exists within the database
				Debug.LogError (cardResultSplit [1]);
				result = cardResultSplit [1];
				hasError = true;
			}
			else if (cardResultSplit [0] == "BANNED") {
				//The card is marked as banned in the database and is therefore
				//unplayable within the game
				Debug.LogWarning ("The card scanned is a banned card!");
				//Get the image
				string imageURL = web_hostroot + cardResultSplit [1];
				getImage = new WWW (imageURL);
				yield return getImage; //Wait for a response
				//Was there an error getting the card?!
				if (getImage.error != null) {
					Debug.LogError ("Failed to get Image for Banned card");
				}
				else {
					//No, so therefore just put the banned card in the result
					//as a texture for the scanner script to handle
					result = getImage.texture;
				}
			}
			else if (cardResultSplit [0] == "SPELL") {
				Debug.Log ("DownloadManager: Found a Spell Card");
				//I know the location of the values because of when coding
				//the server side. These do not change!
				string imageURL = web_hostroot + cardResultSplit [6];
				getImage = new WWW (imageURL); //Get spell card image
				yield return getImage; //Wait
				if (getImage.error != null) {
					//Did it fail to get the image?
					Debug.LogError ("DownloadManager: Error getting the Spell " +
						"Card Image");
					//Has error is for determining if there was an error with
					//getting a necessary component of the card
					hasError = true;
				}
				else {
					//No? We can now build our spell card
					int id = int.Parse (cardResultSplit [1]);
					string name = cardResultSplit [2];
					string type = cardResultSplit [3];
					string text = cardResultSplit [4];
					string formula = cardResultSplit [5];
					//Assign new object to result for the scanner script to handle
					result = new Card (id, name, type, text, formula, 
						getImage.texture);
				}
			}
			else if (cardResultSplit [0] == "MONSTER") {
				Debug.Log ("DownloadManager: Found a Monster Card");
				string imageURL = web_hostroot + cardResultSplit [11];
				//Once again I know the position of the card image URL because
				//of setting it up on the server side
				getImage = new WWW (imageURL);
				yield return getImage; //Get the image
				if (getImage.error != null) {
					Debug.LogError ("DownloadManager: Error getting the " +
						"Monster Card Image");
					hasError = true;
				}
				else {
					//Get the variables for our Monster object
					int id = int.Parse (cardResultSplit [1]);
					string name = cardResultSplit [2];
					string mona = cardResultSplit [3];
					string monb = cardResultSplit [4];
					string attr = cardResultSplit [5];
					int level = int.Parse (cardResultSplit [6]);
					int attack = int.Parse (cardResultSplit [7]);
					int defence = int.Parse (cardResultSplit [8]);
					string text = cardResultSplit [9];
					string formula = cardResultSplit [10];
					//Assign it to the result var for scanner script to handle
					result = new Monster (id, name, mona, monb, attr, level, 
						attack, defence, text, formula, getImage.texture);
				}
			}
		}
		//Stop the timer
		downloadLoop = false;
		downloadTimerThread.Join ();
	}

	/// <summary>
	/// Sends an update request to the webserver so that it can be updated
	/// within the 'cloud'
	/// </summary>
	/// <returns>void</returns>
	/// <param name="accToUpdate">The account which we are updating</param>
	IEnumerator IUpdateAccount(Account accToUpdate) {
		//Start the timer
		downloadTimerThread = new Thread (DownloadTimerThread);
		downloadTimerThread.Start ();
		//
		Debug.Log ("DownloadManager: Updating account " + accToUpdate.Username);
		//Set up form data and assign data to the POST request
		WWW updateAccount = null;
		WWWForm accountForm = new WWWForm();
		accountForm.AddField("GAME", "UPDATEACCOUNT");
		accountForm.AddField("ACCOUNTDETAILS", accToUpdate.ToUpdateString());
		//Create the form and send it to the web host location
		updateAccount = new WWW(web_host, accountForm);
		yield return updateAccount; //Wait for a return
		//Did we get nothing, an error?
		if (updateAccount.error != null) {
			Debug.LogError("DownloadManager: Update Error: " + 
				updateAccount.error);
			hasError = true;
		}
		else { //No? Then that probably means that the account updated in the
			//cloud successfully
			Debug.Log("DownloadManager: Update Result: " + updateAccount.text);
		}
		//Stop the thread for timing the request timing
		downloadLoop = false;
		downloadTimerThread.Join ();
	}
}