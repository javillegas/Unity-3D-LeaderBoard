using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using com.shephertz.app42.paas.sdk.csharp;
using com.shephertz.app42.paas.sdk.csharp.game;


/*Register with App42 platform appHq.shephertz.com/register.
 *Create an app once you are on Quickstart page after registration.
 */

/*Login with AppHQ Management Console from https://apphq.shephertz.com/register/app42Login
 *Go to Business Service Manager from left tab, click on Game Service and select Game.
 *Create game with App42 by clicking on Add Game button from right tab in AppHQ.
 */



public class leaderBoard_UnitySample : MonoBehaviour{
    static ServiceAPI sp = new ServiceAPI("YOUR_API_KEY", "YOUR_SECRET_KEY");
    ScoreBoardService scoreBoardService = null; // Initialising ScoreBoard Service.
 	public string gameName = "SampleGame"; //GameName Created In AppHq Console.
	public string success, columnName, rankersBox, saveBox, txt_user, errorLable, box, txt_score, playerScore, playerName, playerRank;
	public int txt_max;
	
	public static bool Validator(object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
		{
        return true;
        }

	// Use this for initialization
	void Start () 
	{
	 
	 ServicePointManager.ServerCertificateValidationCallback = Validator;
	}
	
	void OnGUI()
    {
		var nxtLine = System.Environment.NewLine; //Use this whenever i need to print something On Next Line.
		
		// For Setting Up ResponseBox.
		GUI.Box(new Rect(450,40,250,175), box);
		GUI.Label(new Rect(470, 50, 1000, 2000), columnName);
		GUI.Label(new Rect(470, 70, 1000, 2000), success);
		GUI.Label(new Rect(470, 70, 1000, 2000), playerRank);
		GUI.Label(new Rect(540, 70, 1000, 2000), playerName);
		GUI.Label(new Rect(620, 70, 1000, 2000), playerScore);
		
		// Label For EXCEPTION Message .
		GUI.Label(new Rect(250, 250, 700, 400), errorLable);
		
	//======================================================================================
	//---------------------------- Saving User Score.---------------------------------------
	//======================================================================================
		GUI.Label(new Rect(20, 40, 200, 20),"User Name");
		txt_user = GUI.TextField(new Rect(100, 40, 200, 20), txt_user);
		GUI.Label(new Rect(20, 70, 200, 20),"Score");
		txt_score = GUI.TextField(new Rect(100, 70, 200, 20), txt_score);
		
        if (GUI.Button(new Rect(100, 100, 200, 50), "Save User Score"))
        {
			// Clearing Data From Response Box. 
			    success     = "";
			    box         = "";
			    playerRank  = "";
				playerName  = "";
				playerScore = "";
			    columnName  = "";
			    errorLable  = "";
			
			string userName = txt_user;  // Name Of The USER Who Wants To Save Score.
            if(txt_score == null || txt_score.Equals("") )
			{
				box = "Score Value Can Not Be Blank: ";
				return;
			}
			double score = double.Parse(txt_score);		// Value Of The Score.
			
			scoreBoardService = sp.BuildScoreBoardService(); // Initializing scoreBoardService.
            try
            {
				//Saving User Score , By Using App42 Scoreboard Service.
				//Method Name->SaveUserScore(gameName, userName, score);
				//Param->gameName(Name Of The Game, Which Is Created By You In AppHQ.)
				//Param->userName(Name Of The User For Which You Want To Save Score.)
				//Param->score( Data Type "double" Value Of Score.)
                Game savedScore = scoreBoardService.SaveUserScore(gameName, userName, score);
				
				success = "Score Successfully Saved : " + nxtLine +
					  "----------------------------------------" + nxtLine + 
					  "Game Name Is : " + savedScore.name + nxtLine + 
					  "User Name Is : " + savedScore.scoreList[0].userName + nxtLine +	
					  "Score Value Is : " + savedScore.scoreList[0].value ;
				
				// Clearing TextBoxes..
				txt_user = "";
				txt_score = "";
            }
            catch (App42Exception e)
            {
				int appErrorCode = e.GetAppErrorCode();
				if(appErrorCode == 3002)
				{
					box = "Exception Occurred :"+ nxtLine +
						  "Game With The Name (" + gameName + ")"+ nxtLine + 
							" Does Not Exist.";
					// handle here , If Game Name Does Not Exist.
				}
				
				else if(appErrorCode == 1401){
					box = "Exception Occurred :"+ nxtLine +
						  "Client Is Not authorized"+ nxtLine +
							"Please Verify Your" + nxtLine + 
							"API_KEY & SECRET_KEY"+ nxtLine +
							"From AppHq.";
					// handle here for Client is not authorized
				}
				else if(appErrorCode == 1500){
					box = "Exception Occurred :"+ nxtLine +
						  "WE ARE SORRY !!"+ nxtLine +
							"But Somthing Went Wrong.";
					// handle here for Internal Server Error
				}else{
						 errorLable = "Exception Occurred :" + e.Message;
            	}
				 App42Log.Debug("Message : " + e.Message);	
			}

        }
		
	//=======================================================================================
	//---------------------------Getting Top N Rankers.--------------------------------------
	//=======================================================================================
		GUI.Label(new Rect(850, 40, 200, 20), "Game Name Is :");
		GUI.Label(new Rect(950, 41, 200, 20), gameName);
		GUI.Label(new Rect(850, 70, 200, 20),"Select Max No.");
		txt_max = (int)GUI.HorizontalSlider(new Rect (945, 75, 100, 30), txt_max, 0, 9);
		GUI.Label(new Rect(1050, 70, 200, 20), txt_max.ToString());
		
		if (GUI.Button(new Rect(860, 100, 200, 50), "GetTop N Rankers"))
        {
			// Clearing Data From Response Box. 
			    success     = "";
			    playerRank  = "";
				playerName  = "";
				playerScore = "";
				box         = "";
				errorLable  = "";
			
			scoreBoardService = sp.BuildScoreBoardService(); // Initializing scoreBoardService.
            int max = txt_max;	// Maximum Number Of TOP RANKERS.
			
			try
            {	
				//Getting Top Scorers , By Using App42 Scoreboard Service.
				//Method Name->GetTopNRankers(gameName, max);
				//Param->gameName(Name Of The Game, Which Is Created By You In AppHQ.)
				//Param->max(Provide Max Number "N" Of Scorers.)
				Game topRankers = scoreBoardService.GetTopNRankers(gameName, max);
				
				// Creating ScoreBoard Manually.
				columnName  = "Rank          " + "Name            " + "Score          ";
				
				for (int i = 0; i < topRankers.GetScoreList().Count; i++)
				{
					string scorerName = topRankers.GetScoreList()[i].userName;
					double scorerValue = topRankers.GetScoreList()[i].value;
					
					playerRank  =  playerRank + (i+1).ToString() + nxtLine; //Getting Rank Of Player.
					playerName = playerName + scorerName + nxtLine; //Getting Player Name.
					playerScore = playerScore + scorerValue.ToString() + nxtLine; // Getting Score Value.
				
				}
				
	         	}
			
            catch (App42Exception e)
            {
				columnName  = "";
				int appErrorCode = e.GetAppErrorCode();
				if(appErrorCode == 3002)
				{
					
					box = "Exception Occurred :"+ nxtLine +
						  "Game With The Name (" + gameName + ")"+ nxtLine + 
							" Does Not Exist.";
					// handle here , if game name does not exist.
				}
				
				else if(appErrorCode == 1401){
					box = "Exception Occurred :"+ nxtLine +
						  "Client Is Not authorized"+ nxtLine +
							"Please Verify Your API_KEY & SECRET_KEY"+ nxtLine +
							"From AppHq.";
					// handle here for Client is not authorized
				}
				else if(appErrorCode == 3013){
					box = "Exception Occurred :"+ nxtLine +
						  "Scores For The Game,"+ nxtLine + 
							"With The Name (" + gameName + ")"+ nxtLine + 
							" Does Not Exist.";
					// handle here , if no scores found for the given gameName.
				}
				else if(appErrorCode == 1500){
					box = "Exception Occurred :"+ nxtLine +
						  "WE ARE SORRY !!"+ nxtLine +
							"But Somthing Went Wrong.";
					// handle here for Internal Server Error
				} 
				// handle here for Other Exceptions. i.e-
				// Max must be greater than ZERO.
				else
				{
                errorLable = "Exception Occurred :" + e.Message;
				}
				App42Log.Debug("Message : " + e.Message);	
				}

        }
	}
}
