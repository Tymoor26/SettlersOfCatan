using UnityEngine;
using System.Collections;

//This class was largely inspired by the unity tutorial:
//Game development & design made fun. Learn C# using Unity 4.6 & Unity 5. Your first 7 2D & 3D games for web & mobile.
//By Ben Tristem
//On Udemy.com
//Section 7 - Glitch Garden: A Plants vs Zombies Clone
//Chapter 136 - Introducing Player Prefs
//Chapter 137 - Our PlayerPrefsManager.cs
//Chapter 138 - Our PlayerPrefsManager - part 2

//This class handles everything that deals with the player prefs class that is built into
//Unity. What player prefs allows is to save various integer and string values that can be taken 
//from one part of the game to the other. In the tutorials I learnt this from allowed the game to save
//such information as the volume setting, the difficulty setting and the latest level that has been unlocked.
//Player prefs is a tool thats useful for saving progress in a game. In my game I am saving information
//such as all the board settings, the opponent settings and the stats from the results of the game. Since all of this
//information can only be saved as a string or an int, I have to make sure that wherever I use this information, that I take these 
//values and do the correct thing with them.
public class PlayerPrefsManager : MonoBehaviour {
    //All the various information that is going to be saved.
    const string OPPONENT_1_SETTING = "opponent_1_setting";
    const string OPPONENT_2_SETTING = "opponent_2_setting";
    const string OPPONENT_3_SETTING = "opponent_3_setting";
    const string WINNER = "winner";
    public static string[] HEX_TERRAIN = { "hex_1_terrain", "hex_2_terrain", "hex_3_terrain", "hex_4_terrain", "hex_5_terrain",
        "hex_6_terrain", "hex_7_terrain", "hex_8_terrain", "hex_9_terrain", "hex_10_terrain", "hex_11_terrain",
        "hex_12_terrain", "hex_13_terrain", "hex_14_terrain", "hex_15_terrain", "hex_16_terrain", "hex_17_terrain",
        "hex_18_terrain", "hex_19_terrain" };
    public static string[] HEX_TOKEN= {"hex_1_token", "hex_2_token", "hex_3_token", "hex_4_token", "hex_5_token",
        "hex_6_token", "hex_7_token", "hex_8_token", "hex_9_token", "hex_10_token", "hex_11_token",
        "hex_12_token", "hex_13_token", "hex_14_token", "hex_15_token", "hex_16_token", "hex_17_token",
        "hex_18_token", "hex_19_token"};
    public static string[] HARBOURS = { "harbour_1", "harbour_2", "harbour_3", "harbour_4", "harbour_5",
        "harbour_6", "harbour_7", "harbour_8", "harbour_9", "harbour_10", "harbour_11",
        "harbour_12", "harbour_13", "harbour_14", "harbour_15", "harbour_16", "harbour_17", "harbour_18" };
    public static string[] STATS = { "settlements_built", "cities_built", "knight_cards_used",
        "longest_road", "victory_point_cards_used", "largest_army_card_held","longest_road_card_held", "total_points" };

    //Setters and getters methods for all the information that is being stored.
    //position - 1 is here to make sure its getting the correct index value in the array.
    public static void SetTerrainTile(int position, string terrain)
    {
        int p = position-1;
        PlayerPrefs.SetString(HEX_TERRAIN[p], terrain);
    }

    public static string GetTerrainTile(int position)
    {
        int p = position-1;
        return PlayerPrefs.GetString(HEX_TERRAIN[p]);
    }

    public static void SetToken(int position, string token)
    {
        int p = position-1 ;
        PlayerPrefs.SetString(HEX_TOKEN[p], token);
    }

    public static string GetToken(int position)
    {
        int p = position- 1;
        return PlayerPrefs.GetString(HEX_TOKEN[p]);
    }

    public static void SetHarbour(int position, string harbour)
    {
        int p = position - 1;
        PlayerPrefs.SetString(HARBOURS[p], harbour);
    }

    public static string GetHarbour(int position)
    {
        int p = position-1;
        return PlayerPrefs.GetString(HARBOURS[p]);
    }

    public static void SetOpponentSetting(int opponent, string setting)
    {
        if (opponent == 1)
        {
            PlayerPrefs.SetString(OPPONENT_1_SETTING, setting);
        }
        else if (opponent == 2)
        {
            PlayerPrefs.SetString(OPPONENT_2_SETTING, setting);
        }
        else if (opponent == 3)
        {
            PlayerPrefs.SetString(OPPONENT_3_SETTING, setting);
        }
        else
        {
            print("error, this shouldn't happen");
        }
    }

    public static string GetOpponentSetting(int opponent)
    {
        if (opponent == 1)
        {
            return PlayerPrefs.GetString(OPPONENT_1_SETTING);
        }
        else if (opponent == 2)
        {
            return PlayerPrefs.GetString(OPPONENT_2_SETTING);
        }
        else if (opponent == 3)
        {
            return PlayerPrefs.GetString(OPPONENT_3_SETTING);
        }
        else
        {
            return "error, this shouldn't happen";
        }
    }

    public static void SetResults(int[] p1stats, int[] p2stats, int[] p3stats, int[] p4stats, int winner)
    {
        for (int i = 0; i < 8; i++)
        {
            PlayerPrefs.SetInt("p1_" + STATS[i], p1stats[i]);
            PlayerPrefs.SetInt("p2_" + STATS[i], p2stats[i]);
            PlayerPrefs.SetInt("p3_" + STATS[i], p3stats[i]);
            PlayerPrefs.SetInt("p4_" + STATS[i], p4stats[i]);
        }
        PlayerPrefs.SetInt(WINNER, winner);
    }

    public static int[] GetStats(int player)
    {
        int[] stats = new int[8]; 
        for (int i = 0; i < 8; i++)
        {
            stats[i] = PlayerPrefs.GetInt("p" + player + "_" + STATS[i]);
        }
        return stats;
    }

    public static int GetWinner()
    {
        return PlayerPrefs.GetInt(WINNER);
    }
}
