using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//This class was largely inspired by the unity tutorial:
//Game development & design made fun. Learn C# using Unity 4.6 & Unity 5. Your first 7 2D & 3D games for web & mobile.
//By Ben Tristem
//On Udemy.com
//Section 7 - Glitch Garden: A Plants vs Zombies Clone
//Chapter 163 - "Tower" Selector Buttons

//This class handles with the selection of the terrain buttons on the board setup screen.
//What this does is that when a terrain button is selected, that particular terrain button is
//lighted up while the others are transparent to indicate that it has been selected.
//When another one is selected, then the same will happen to that one.
public class TerrainButton : MonoBehaviour {
    public Sprite terrainPrefab;          //In the unity editor I put in the terrain Hex prefab for that button
    public static Sprite selectedTerrain; //The BoardSetup class uses this to get whatever is selected

    private TerrainButton[] buttonArray;  //This stores all the terrain buttons

    // Use this for initialization
    private void Start()
    {
        buttonArray = GameObject.FindObjectsOfType<TerrainButton>(); //grabs all the objects that has this class as a component
    }

    //When a button is clicked, that button will become opaque while the rest are kept transparent
    private void OnMouseDown()
    {
        foreach (TerrainButton thisButton in buttonArray)
        {
            if (thisButton.GetComponent<SpriteRenderer>().color.a > 0.5f)
            {
                thisButton.GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, 0.5f);
            }
        }
        GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, 0.5f);
        selectedTerrain = terrainPrefab;
    }
}
