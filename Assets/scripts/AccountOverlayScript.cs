/*
 * Author: Stuart Harrison 
 * Date: December 2015
 * Class for controling the account information displayed on the mainmenuscene
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AccountOverlayScript : MonoBehaviour {

	//UnityUI objects that are assigned in the editor
	public Image ui_img_displayPicture;
	public Text ui_lbl_accountName;
	public Text ui_lbl_information;

	private AccountController accountControl;
	private GameObject glob_gamemanager;
	private string accountName;
	private string accountInformation;

	//Need to gain access to the AccountController class so that we can
	//'pull' the account information we want to display
	void Start () {
		glob_gamemanager = GameObject.FindGameObjectWithTag ("GameManager");
		if (glob_gamemanager != null) {
			accountControl = glob_gamemanager.GetComponent<AccountController> ();
		}
	}

	void Update () {
		//When the accountcontrol is not null and we have access to the logged
		//in account display the details onto the assigned UnityUI objects
		if (accountControl != null) {
			//Shouldn't really need to check for if the logged in account
			//is not null because the AccountController and NetworkController
			//handle moving scenes should the logged in account be null,
			//therefore this script shouldn't even be in the same scene as a
			//null logged in a ccount
			accountName = accountControl.LoggedInAccount.Username;
			accountName += " (Level " + accountControl.
				LoggedInAccount.Level + ")";

			accountInformation = "Wins: " + 
				accountControl.LoggedInAccount.Wins + ",";
			accountInformation += "Loss: " + 
				accountControl.LoggedInAccount.Loss + ",";
			accountInformation += "Draw: " + 
				accountControl.LoggedInAccount.Draw + ",";

			accountInformation = accountInformation.Replace (",", "\n");

			ui_img_displayPicture.sprite = 
				accountControl.LoggedInAccount.DisplayPicture;
			ui_lbl_accountName.text = accountName;
			ui_lbl_information.text = accountInformation;
		}
	}
}