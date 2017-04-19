using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

//This class was largely inspired by the unity tutorial:
//Game development & design made fun. Learn C# using Unity 4.6 & Unity 5. Your first 7 2D & 3D games for web & mobile.
//By Ben Tristem
//On Udemy.com
//Section 4 - Number Wizard UI
//Chapter 48 - How to Load Scenes and Quit

//This class handles going from scene to scene on the game itself.
//It also has methods for saving the information on various screens like
//the way the board is setup on the board setup screen and the settings that has
//been selected for the AI in the AI settings selector screen.
public class LevelManager : MonoBehaviour
{
    //public float autoLoadNextLevelAfter;
    //public bool onSplash;

    private void Start()
    {
        //if (onSplash && autoLoadNextLevelAfter > 0) { Invoke("LoadNextLevel", autoLoadNextLevelAfter); }
    }

    //Method for loading a specific scene
    public void LoadLevel(string name)
    {
        Debug.Log("New Level load: " + name);
        SceneManager.LoadScene(name);
    }

    //Method for quitting the game when the quit button on the start screen has been clicked
    public void QuitRequest()
    {
        Debug.Log("Quit requested");
        Application.Quit();
    }

    //Method for loading the next level in the build settings order
    public void LoadNextLevel()
    {
        SceneManager.LoadScene(Application.loadedLevel + 1);
    }

    //Method for loading the previous level in the build settings order
    public void LoadPreviousLevel()
    {
        SceneManager.LoadScene(Application.loadedLevel - 1);
    }

    //Method for taking what has been set up on the board and saving it to the player prefs so that it can be used later
    public void SaveBoardSetup()
    {
        for (int i = 1; i < 20; i++)
        {
            //save terrain tiles
            string terrain = GameObject.Find("Hex " + i).GetComponent<Image>().sprite.name;
            PlayerPrefsManager.SetTerrainTile(i, terrain);

            //save tokens
            string token;
            if (terrain != "Desert tile") { token = GameObject.Find("Token " + i).GetComponent<Image>().sprite.name; }
            else                          { token = "Robber"; }
            PlayerPrefsManager.SetToken(i, token);


            //save harbours
            if (i < 19)
            {
                string harbour;
                if(GameObject.Find("Harbour " + i).GetComponent<Image>().color != Color.grey)
                {
                    harbour = GameObject.Find("Harbour " + i).GetComponent<Image>().sprite.name;
                    PlayerPrefsManager.SetHarbour(i,harbour);
                }
                else
                {
                    harbour = "null";
                    PlayerPrefsManager.SetHarbour(i,harbour);
                }
            }
        }
        LoadNextLevel();
    }

    //Method for taking what has been selected on the AI settings selector screen and saving it to the player prefs so it can be used later
    public void SaveOpponentSetup()
    {
        for (int i = 1; i < 4; i++)
        {
            string setting = GameObject.Find("Opponent " + i + " Dropdown").transform.FindChild("ContainerText").GetComponent<Text>().text;
            PlayerPrefsManager.SetOpponentSetting(i, setting);
        }
        LoadNextLevel();
    }
}
