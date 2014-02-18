using UnityEngine;
using System;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.game;

/*Register with App42 platform appHq.shephertz.com/register.
 *Create an app once you are on Quickstart page after registration.
 */

/*Login with AppHQ Management Console from https://apphq.shephertz.com/register/app42Login
 *Go to Business Service Manager from left tab, click on Game Service and select Game.
 *Create game with App42 by clicking on Add Game button from right tab in AppHQ.
 */

public class UnityLeaderboard : MonoBehaviour, App42CallBack
{
	//static ServiceAPI sp = null;
	ScoreBoardService scoreBoardService = null; // Initialising ScoreBoard Service.
	Constants cons = new Constants ();
	public string success, columnName, rankersBox, saveBox, txt_user, errorLable, box, txt_score, playerScore, playerName, playerRank;
	public int txt_max;
	public bool saveButton, leaderBoardButton;
	
	#if UNITY_EDITOR
	public static bool Validator (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
	{return true;}
	#endif
	void Start ()
	{
		#if UNITY_EDITOR
		ServicePointManager.ServerCertificateValidationCallback = Validator;
		#endif
		//sp = new ServiceAPI (cons.apiKey, cons.secretKey);
		App42API.Initialize(cons.apiKey, cons.secretKey);
	}
	
	void OnGUI ()
	{   
		// For Setting Up ResponseBox.
		GUI.Box (new Rect (450, 40, 250, 175), box);
		GUI.Label (new Rect (470, 50, 200, 200), columnName);
		GUI.Label (new Rect (470, 70, 200, 200), success);
		GUI.Label (new Rect (470, 70, 200, 200), playerRank);
		GUI.Label (new Rect (540, 70, 200, 200), playerName);
		GUI.Label (new Rect (620, 70, 200, 200), playerScore);

		if (GUI.Button (new Rect (470, 250, 200, 50), "QUIT")) {
			Application.Quit();
		}

		// Label For EXCEPTION Message .
		GUI.Label (new Rect (250, 250, 700, 400), errorLable);
		
		//======================================================================================
		//---------------------------- Saving User Score.---------------------------------------
		//======================================================================================
		GUI.Label (new Rect (20, 40, 200, 20), "User Name");
		txt_user = GUI.TextField (new Rect (100, 40, 200, 20), txt_user);
		GUI.Label (new Rect (20, 70, 200, 20), "Score");
		txt_score = GUI.TextField (new Rect (100, 70, 200, 20), txt_score, 4);
		txt_score = Regex.Replace (txt_score, @"[^0-9]", "");
		
		if (GUI.Button (new Rect (100, 100, 200, 50), "Save User Score")) {
			// Clearing Data From Response Box. 
			success = "";
			box = "";
			playerRank = "";
			playerName = "";
			playerScore = "";
			columnName = "";
			errorLable = "";
			
			if (txt_user == null || txt_user.Equals ("")) {
				box = "User Name Can Not Be Blank: ";
				return;
			}
			string userName = txt_user;  // Name Of The USER Who Wants To Save Score.
			if (txt_score == null || txt_score.Equals ("")) {
				box = "Score Value Can Not Be Blank: ";
				return;
			}
			double score = double.Parse (txt_score);		// Value Of The Score.
			
			scoreBoardService = App42API.BuildScoreBoardService (); // Initializing scoreBoardService.
			//Saving User Score , By Using App42 Scoreboard Service.
			//Method Name->SaveUserScore(gameName, userName, score);
			//Param->gameName(Name Of The Game, Which Is Created By You In AppHQ.)
			//Param->userName(Name Of The User For Which You Want To Save Score.)
			//Param->score( Data Type "double" Value Of Score.)
			//Param->Callback(callback for success/exception.);
			scoreBoardService.SaveUserScore (cons.gameName, userName, score, this);
			saveButton = true;
		}
		
		//=======================================================================================
		//---------------------------Getting Top N Rankers.--------------------------------------
		//=======================================================================================
		GUI.Label (new Rect (850, 40, 200, 20), "Game Name Is :");
		GUI.Label (new Rect (950, 41, 200, 20), cons.gameName);
		GUI.Label (new Rect (850, 70, 200, 20), "Select Max No.");
		txt_max = (int)GUI.HorizontalSlider (new Rect (945, 75, 100, 30), txt_max, 0, 9);
		GUI.Label (new Rect (1050, 70, 200, 20), txt_max.ToString ());
		
		if (GUI.Button (new Rect (860, 100, 200, 50), "GetTop N Rankers")) {
			// Clearing Data From Response Box. 
			success = "";
			playerRank = "";
			playerName = "";
			playerScore = "";
			box = "";
			errorLable = "";
			
			if (txt_max == 0) {
				box = "Max Must Be Greater Than Zero: ";
				return;
			}
			
			scoreBoardService = App42API.BuildScoreBoardService (); // Initializing scoreBoardService.
			int max = txt_max;	// Maximum Number Of TOP RANKERS.
			
			//Getting Top Scorers , By Using App42 Scoreboard Service.
			//Method Name->GetTopNRankers(gameName, max);
			//Param->gameName(Name Of The Game, Which Is Created By You In AppHQ.)
			//Param->max(Provide Max Number "N" Of Scorers.)
			//Param->Callback(callback for success/exception.);
			scoreBoardService.GetTopNRankers (cons.gameName, max, this);
			leaderBoardButton = true;
		}
		
	}
	
	public void OnSuccess (object response)
	{
		var nxtLine = System.Environment.NewLine; //Use this whenever i need to print something On Next Line.
		
		Game gameResponseObj = (Game)response;
		if (saveButton) {
			saveButton = false;
			columnName = "";
			success = "Score Successfully Saved : " + nxtLine +
				"----------------------------------------" + nxtLine + 
					"Game Name Is : " + gameResponseObj.GetName () + nxtLine + 
					"User Name Is : " + gameResponseObj.GetScoreList () [0].GetUserName () + nxtLine + 
					"Score Value Is : " + gameResponseObj.GetScoreList () [0].GetValue ();
			
			// Clearing TextBoxes..
			txt_user = "";
			txt_score = "";
		}
		
		if (leaderBoardButton) {
			leaderBoardButton = false;
			IList<Game.Score> topRankersList = gameResponseObj.GetScoreList ();
			if (topRankersList.Count > 0) {
				// Creating ScoreBoard Manually.
				columnName = "Rank          " + "Name            " + "Score          ";
				
				for (int i = 0; i < gameResponseObj.GetScoreList().Count; i++) {
					string scorerName = gameResponseObj.GetScoreList () [i].GetUserName ();
					double scorerValue = gameResponseObj.GetScoreList () [i].GetValue ();
					
					playerRank = playerRank + (i + 1).ToString () + nxtLine; //Getting Rank Of Player.
					playerName = playerName + scorerName + nxtLine; //Getting Player Name.
					playerScore = playerScore + scorerValue.ToString () + nxtLine; // Getting Score Value.
				}
			}
		}
	}
	
	public void OnException (Exception e)
	{
		var nxtLine = System.Environment.NewLine; //Use this whenever i need to print something On Next Line.
		
		App42Exception exception = (App42Exception)e;
		int appErrorCode = exception.GetAppErrorCode ();
		if (appErrorCode == 3002) {
			box = "Exception Occurred :" + nxtLine +
				"Game With The Name (" + cons.gameName + ")" + nxtLine + 
					" Does Not Exist.";
			// handle here , If Game Name Does Not Exist.
		} else if (appErrorCode == 3013) {
			box = "Exception Occurred :" + nxtLine +
				"Scores For The Game," + nxtLine + 
					"With The Name (" + cons.gameName + ")" + nxtLine + 
					" Does Not Exist.";
			// handle here , if no scores found for the given gameName.
		} else if (appErrorCode == 1401) {
			box = "Exception Occurred :" + nxtLine +
				"Client Is Not authorized" + nxtLine +
					"Please Verify Your" + nxtLine + 
					"API_KEY & SECRET_KEY" + nxtLine +
					"From AppHq.";
			// handle here for Client is not authorized
		} else if (appErrorCode == 1500) {
			box = "Exception Occurred :" + nxtLine +
				"WE ARE SORRY !!" + nxtLine +
					"But Somthing Went Wrong.";
			// handle here for Internal Server Error
		} else {
			errorLable = "Exception Occurred :" + exception;
			//errorLable = "Exception Occurred :" + exception.GetMessage ();
		}
		App42Log.Console ("Message : " + e);	
		//App42Log.Console ("Message : " + exception.GetMessage ());	
	}
}