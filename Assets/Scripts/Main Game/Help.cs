﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

//This class is for the hovering over of elements of the screen and to get the
//information for that element to pop up. These elements incluse the headings for the
//scoreboard table and the development cards. When the mouse is hovered over it
//text will appear and when it is put away from the element then the text will disappear.
public class Help : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool isSummary;
    private bool mouseOver = false;
    private string selectedText = "";

    //This method is called when the mouse pointer is taken off the element
    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject textBox = GameObject.Find("Summary Header Help Text");
        textBox.GetComponent<Text>().enabled = false;
        selectedText = "";
        textBox.GetComponent<Text>().text = selectedText;
        GameObject.Find("Summary Header Help Text Background").GetComponent<Image>().color = new Color(255, 255, 255, 0);

        textBox = GameObject.Find("Dev Cards Image Help Text");
        textBox.GetComponent<Text>().enabled = false;
        textBox.GetComponent<Text>().text = selectedText;
        GameObject.Find("Dev Card Image Help Text Background").GetComponent<Image>().color = new Color(255, 255, 255, 0);
    }

    //This method is called when the mouse pointer is put onto the element.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSummary)
        {
            GameObject.Find("Summary Header Help Text Background").GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
            if      (name == "Settlement table label")         { selectedText = "Settlements built (worth 1 victory point each)"; }
            else if (name == "City table label")               { selectedText = "Cities built (worth 2 victory point each)"; }
            else if (name == "Knight table label")             { selectedText = "Knight cards used. Use 3 or more to get the largest army card. Only one player can hold it (italic shows who holds the largest army card, worth 2 victory points)"; }
            else if (name == "Road table label")               { selectedText = "Longest road on board. Have a continuous road of 5 or more pieces to get the longest road card. Only one player can hold it (italic shows who hold the longest road card, worth 2 victory points) "; }
            else if (name == "Victory point card table label") { selectedText = "Victory point cards used (worth 1 victory point each)"; }
            else if (name == "Total table label")              { selectedText = "Total victory points earnt. Need 10 to win the game."; }
            GameObject textBox = GameObject.Find("Summary Header Help Text");
            textBox.GetComponent<Text>().enabled = true;
            textBox.GetComponent<Text>().text = selectedText;
        }
        else
        {
            GameObject.Find("Dev Card Image Help Text Background").GetComponent<Image>().color = new Color(255, 255, 255, 0.5f);
            if (name == "Road Building Card Image")
            {
                selectedText = "Road Building Card: If you play this card then you may place 2 road pieces for free (as long as it abides by the road building rules)";
            }
            else if (name == "Monopoly Card Image")
            {
                selectedText = "Monopoly Card: If you play this card you may select a resource, then steal all of that resource from every player that currently holds it";
            }
            else if (name == "Year of Plenty Card Image")
            {
                selectedText = "Year of Plenty Card: If you play this card you may take any two resource cards to add to your current resource cards in hand.";
            }
            else if (name == "Knight Card Image")
            {
                selectedText = "Knight Card: If you play this card you may move the robber. Then it will add 1 to your knight card used value. If you have used 3 or more then you will hold the largest army card. (assuming that you don't end up having used as many as the person currently holding the largest army card)";
            }
            else if (name == "Victory Point Card Image")
            {
                selectedText = "Victory point Card: If you play this card then you gain a victory point.";
            }
            GameObject textBox = GameObject.Find("Dev Cards Image Help Text");
            textBox.GetComponent<Text>().enabled = true;
            textBox.GetComponent<Text>().text = selectedText;
        }
    }
}
