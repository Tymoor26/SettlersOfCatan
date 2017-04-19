using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

//This class takes the stats saved to the player prefs from the main game from when the game is officially over
//and outputs them onto the results screen in a table form.
public class Results : MonoBehaviour {
    //In the unity editor, I put in all the elements of the results table into these variables. I take each element from
    //each row and put them into the respective variable.
    public Text[] p1StatsText;
    public Text[] p2StatsText;
    public Text[] p3StatsText;
    public Text[] p4StatsText;

    // Use this for initialization
    private void Start () {
        //Get all the information from the player prefs manager
        int[] p1stats = PlayerPrefsManager.GetStats(1);
        int[] p2stats = PlayerPrefsManager.GetStats(2);
        int[] p3stats = PlayerPrefsManager.GetStats(3);
        int[] p4stats = PlayerPrefsManager.GetStats(4);
        int winner = PlayerPrefsManager.GetWinner();

        //Checking who the winner is and outputting the correct message of who won.
        if(winner == 1)
        {
            GameObject.Find("Winner label").GetComponent<Text>().text = "Player 1 won the game";
            GameObject.Find("Winner label").GetComponent<Text>().color = Color.red;
        }
        else if (winner == 2)
        {
            GameObject.Find("Winner label").GetComponent<Text>().text = "Player 2 won the game";
            GameObject.Find("Winner label").GetComponent<Text>().color = Color.yellow;
        }
        else if (winner == 3)
        {
            GameObject.Find("Winner label").GetComponent<Text>().text = "Player 3 won the game";
            GameObject.Find("Winner label").GetComponent<Text>().color = Color.white;
        }
        else if (winner == 4)
        {
            GameObject.Find("Winner label").GetComponent<Text>().text = "Player 4 won the game";
            GameObject.Find("Winner label").GetComponent<Text>().color = Color.blue;
        }

        //This goes through each index from the public array variables and puts the stats from the
        //player prefs into them. This results in the results table outputting all the stats from the game.
        for (int i = 0; i < 8; i++)
        {
            if (i == 5 || i == 6) //Checking for the largest army and longest road stat and outputting them for the relevant players onto the results table.
            {
                if (p1stats[i] == 1) { p1StatsText[i].text = "YES"; }
                else                 { p1StatsText[i].text = "NO";  }
                if (p2stats[i] == 1) { p2StatsText[i].text = "YES"; }
                else                 { p2StatsText[i].text = "NO";  }
                if (p3stats[i] == 1) { p3StatsText[i].text = "YES"; }
                else                 { p3StatsText[i].text = "NO";  }
                if (p4stats[i] == 1) { p4StatsText[i].text = "YES"; }
                else                 { p4StatsText[i].text = "NO";  }
            }
            else
            {
                p1StatsText[i].text = p1stats[i].ToString();
                p2StatsText[i].text = p2stats[i].ToString();
                p3StatsText[i].text = p3stats[i].ToString();
                p4StatsText[i].text = p4stats[i].ToString();
            }
        }
	}
}
