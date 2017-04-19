using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

//This class was largely inspired by the youtube tutorial video:
//Unity 5 UI Tutorial - Dropdowns [#2 - Custom Editor] .
//By user Lena Florian
//On Youtube.com
//The part about the descriptions of the AI settings was of my own creation.

//This class handles the dropdown functionality of the AI behaviour selectors on the 
//AI settings screen. It also helps display the description of the AI setting that has
//been selected to help give the user a better understanding of what they are selecting.
public class Dropdown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public RectTransform container;
    public bool isOpen;

    // Use this for initialization
    private void Start()
    {
        container = transform.FindChild("Container").GetComponent<RectTransform>();
        isOpen = false;
    }

    // Update is called once per frame
    private void Update()
    {
        Vector3 scale = container.localScale;
        scale.y = Mathf.Lerp(scale.y, isOpen ? 1 : 0, Time.deltaTime * 12);
        container.localScale = scale;
        transform.FindChild("Setting Description").GetComponent<Text>().text = GetText(transform.FindChild("ContainerText").GetComponent<Text>().text);
    }

    public void OnPointerEnter(PointerEventData eventData) { isOpen = true;  }

    public void OnPointerExit(PointerEventData eventData)  { isOpen = false; }

    public void SelectOption(String text) { transform.FindChild("ContainerText").GetComponent<Text>().text = text; }

    public string GetText(string setting)
    {
        if (setting == "Random")
        {
            return "Everything the computer player does is random. Random actions, random placement of pieces and random trading.";
        }
        else if (setting == "Quick Thinker")
        {
            return "This player is more about quick building of settlements, roads and cities. They prioritise placing initial settlements on spots that will get them more resources. This computer player rarely trades, and will only trade if they are very close to building what they want to build. When they do trade, they make the trade fair or make it more generous.";
        }
        else if (setting == "Smart Thinker")
        {
            return "This player is more about building of settlements, roads and cities and is more likely to trade more in order to try and build those pieces, even making some unfair trades. They not only look into placing initial settlements on spots that gives them the most resources, but will also priortise spots that have hexes with numbered tokens that are most likely to be rolled by the dice as well as placing settlements near harbours to allow them to get better trades.";
        }
        else if (setting == "All Around Thinker")
        {
            return "This player tries to prioritise building all pieces equally and will only priortise one over the other if they have an opportunity to build multiple pieces. They will make trades on which piece they are the closest to building, even making unfair trades to do so. When it comes to placing initial settlements, they try to get a wider variety of numbered tokens covered as well as try and avoid placing two settlements that border the same hex, and to make sure that the spot allows them to gain resources that will build the various pieces.";
        }
        else
        {
            return "";
        }
    }
}
