﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//This class handles all the functionality to do with the board.
//This ranges from setting up the board when the game starts, checking for various requirements to check whether a
//spot on the board is a valid place to place a settlement, city or road piece, 
//deactivating the buttons for the piece placement on the board, getting the longest
//road, getting and distributing the resources for when a robber is moved, 
//help set up the trade window functionality and various other getter and setter methods.
//There are also methods for getting the weights for placement options for the AI
public class Board : MonoBehaviour {
    public Sprite[] tokenPrefabs;
    public Sprite[] terrainPrefabs;
    public Sprite[] harbourPrefabs;
    public Player player;
    public Computer compPlayer1;
    public Computer compPlayer2;
    public Computer compPlayer3;
    private bool evenHarbour = false;
    private Dictionary<GameObject, GameObject> listOfSpotsAndTheirHarbours; //Dictionary is used for object pairs

    //-------------------------------------------------------------------------------
    //Check methods to help with the generation of legal moves.

    //This checks wheter a given road spot is connected to a settlement that belongs to the same player.
    //If this is true then this road spot is a valid spot for the player to place the road on
    public bool CheckForConnectingSettlements(GameObject roadSpot, Color color)
    {
        for (int j = 1; j < 55; j++) //go through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + j);
            //check if the two board spots are next to each other and whether the settlement spot belongs to the player
            if (SpotComparison(roadSpot, settlementSpot, 1)
                && settlementSpot.GetComponent<Button>().colors.disabledColor == color) { return true; }
        }
        return false;
    }

    //This method checks whether a given board spot is connected to a road that belongs to that player. Since there are two 
    //types of peices that can be connected to a road, the variable type differentiates between the two,
    //where 0 if for comparing road to road and 1 for settlement to road.
    //If this is true then the given board spot is a valid spot for that player to place that piece on.
    public bool CheckForConnectingRoads(GameObject spot, int type, bool initialRound1, bool initialRound2, Color colour)
    {
        if (type == 1 && (initialRound1 || initialRound2)) { return true; }
        for (int i = 1; i < 73; i++) //go through all the road spots
        {
            GameObject otherRoadSpot = GameObject.Find("Road Button " + i);
            //check that if spot represents a road spot, then make sure that otherRoadSpot isn't the same as spot. Also check that
            //the two spots are next to each other and that otherRoadSpot belongs to the player. 
            if (otherRoadSpot != spot && SpotComparison(spot, otherRoadSpot, type) && otherRoadSpot.GetComponent<Button>().colors.disabledColor == colour)
            {
                if (type == 1) { return true; } //check if it was a settlement to road comparison

                //check if there is a settlement that belongs to the opponent that is between the two road spots. If there isn't one then a road piece can
                //be placed at the given spot. If there is a settlement that belons to the opponent then the given road spot can't be placed there.
                else if (!CheckIfOpponentsSettlementBlocksRoadPath(spot, otherRoadSpot, colour)) { return true; }
            }
        }
        return false;
    }

    //This method checks whether there is a settlement that belongs to the opponent in between the two given road spots.
    private bool CheckIfOpponentsSettlementBlocksRoadPath(GameObject roadSpot, GameObject otherRoadSpot, Color playerColour)
    {
        for (int j = 1; j < 55; j++) //go through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + j);
            ColorBlock settlementSpotColour = settlementSpot.GetComponent<Button>().colors;
            Sprite settlementSpotSprite = settlementSpot.GetComponent<Image>().sprite;

            //check for if both the given road spots are next to this settlement spot and if it belongs to the player or if there is no settlement there
            bool check1 = SpotComparison(roadSpot, settlementSpot, 1);
            bool check2 = SpotComparison(otherRoadSpot, settlementSpot, 1);

            bool isSettlementInBetween = SpotComparison(roadSpot, settlementSpot, 1) && SpotComparison(otherRoadSpot, settlementSpot, 1);
            bool doesSettlementBelongToPlayerOrNoOne = (settlementSpotColour.disabledColor == playerColour || settlementSpotSprite.name == "UISprite");

            if (isSettlementInBetween && doesSettlementBelongToPlayerOrNoOne) { return false; }
        }
        return true;
    }

    //This method checks whether the given settlement spot obeys the distance rule. 
    //The distance rule is a rule where you cannot place a settlement in a spot that has a settlement placed
    //next in a spot that is next to it. This method checks to see if there is a settlement that has been placed
    //in a spot next to the given settlement spot.
    public bool ObeysDistanceRule(GameObject settlementSpot)
    {
        for (int i = 1; i < 55; i++) //go through all the settlement spots
        {
            GameObject otherSettlementSpot = GameObject.Find("Settlement/City button " + i);
            //check if the given settlement spot isn't the currently checked settlement spot and
            //if the two settlement spots are next to each other and if the other settlement spot
            //has a settlement placed on it. If all are true, then this returns false.
            if (otherSettlementSpot != settlementSpot
                && SpotComparison(settlementSpot, otherSettlementSpot, 2)
                && otherSettlementSpot.GetComponent<Image>().sprite.name != "UISprite") { return false; }
        }
        return true;
    }

    //This methods returns the settlement that is next to the given road spot. This is only used in the second round
    //of the initial turn as you need to place a road next to the settlement that is placed in the second round.
    public GameObject GetSettlementConnectedWithRoad(GameObject roadSpot)
    {
        for (int j = 1; j < 55; j++) //go through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + j);
            //if they are next to each other and the settlement spot belongs to the player
            if (SpotComparison(roadSpot, settlementSpot, 1) && settlementSpot.GetComponent<Button>().colors.disabledColor == Color.red) { return settlementSpot; }
        }
        return null;
    }

    //-------------------------------------------------------------------------------
    //Methods that deactivate buttons on the board

    //This deactivates all of the settlement placement buttons on the board
    public void DeactivateSettlementPlacementsButtons()
    {
        for (int i = 1; i < 55; i++) //goes through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);
            Button theButton = settlementSpot.GetComponent<Button>();
            theButton.interactable = false;
        }
    }

    //This deactivates all of the road placement buttons on the board
    public void DeactivateRoadPlacementButtons()
    {
        for (int i = 1; i < 73; i++) //goes through all the road spots
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            theButton.interactable = false;
        }
    }

    //This deactivates all of the robber placement buttons on the board
    public void DeactivateRobberPlacementButtons()
    {
        for (int i = 1; i < 20; i++) //goes through all the robber spots
        {
            GameObject robberSpot = GameObject.Find("Move Robber Button " + i);
            robberSpot.GetComponent<Button>().interactable = false;
        }
    }

    //-------------------------------------------------------------------
    //Getter and setter helper methods 
    
    //Gets the colour of the given player
    public Color GetPlayerColour(int player)
    {
        if      (player == 1) { return Color.red;    }
        else if (player == 2) { return Color.yellow; }
        else if (player == 3) { return Color.white;  }
        else if (player == 4) { return Color.blue;   }
        else { return Color.black; }
    }

    //Gets the amount of resources from a hex for a given player when a dice is rolled and that 
    //roll corresponds to this hex. It takes in the player colour as it needs to compare the colour of the settlement
    //to the players colour
    public int GetAmountOfResourcesFromHex(GameObject hex, Color playerColour)
    {
        int amount = 0; //initialise the amount value to 0 in the case that the player doesn't have any settlements or cities around this hex.
        for (int i = 1; i < 55; i++) //goes through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);

            //if the settlement is next to the hex and if the settlement belongs to the player
            if (SpotComparison(settlementSpot, hex, 3) && settlementSpot.GetComponent<Button>().colors.disabledColor == playerColour)
            {
                if (settlementSpot.GetComponent<Image>().sprite.name == "Settlement White") { amount++; } //if its a settlement
                else { amount = amount + 2; }                                                             //if its a city
            }
        }
        return amount;
    }

    //When the robber is moved from a hex that isn't a desert hex, then this method gets what token was originally in that hex
    public string GetOriginalToken(int counter) { return PlayerPrefsManager.GetToken(counter); }

    //Methods to change the token of a hex from robber to its original token, and vice versa
    public void SetTokenToOriginal(int counter, string token) { GameObject.Find("Token " + counter).GetComponent<Image>().sprite = tokenPrefabs[GetIndexToken(token)]; }
    public void SetTokenToRobber(GameObject token) { token.GetComponent<Image>().sprite = tokenPrefabs[10]; }

    //------------------------------------------------------------------------
    //Method for stealing a random resource from when a robber is moved by a player to a hex

    //This method grabs checks for all the settlements that are around a hex that the robber has been moved to and gets a list of all
    //the opponent players that have a settlement next to this hex. Then it will pick a random opponent player from this list and proceed
    //to steal a random resource from this player.
    public int StealResource(GameObject hex, Color colour)
    {
        List<GameObject> listOfChoicesToStealFrom = new List<GameObject>();
        for (int i = 1; i < 55; i++) //goes through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);

            //This checks if the hex and the settlement spot are next to each other
            //and if the settlement spot doesn't belong to the player (aka belonging to the player)
            //and check if the player with that settlement has a resource to give. If all are true,
            //then that player is added to the list. At most there should be 3 elements added to the list (since a hex
            //can have at most 3 settlements around it.
            if (SpotComparison(hex, settlementSpot, 3)
                && settlementSpot.GetComponent<Button>().colors.disabledColor != colour
                && CheckIfPlayerHasResourcesToGive(settlementSpot)) { listOfChoicesToStealFrom.Add(settlementSpot); }
        }

        int size = listOfChoicesToStealFrom.Count;
        int resource = -1; //-1 is for when there are no players to steal from
        if (size != 0)
        {
            Color playerToStealFromColour = listOfChoicesToStealFrom[Random.RandomRange(0, size)].GetComponent<Button>().colors.disabledColor;
            resource = SelectResourceToSteal(playerToStealFromColour);
        }

        return resource;
    }

    //This checks whether the player who's settlement is at the settlement spot 
    //has any resources. If there are no settlements on the spot then this returns false.
    private bool CheckIfPlayerHasResourcesToGive(GameObject settlementSpot)
    {
        Color playerColour = settlementSpot.GetComponent<Button>().colors.disabledColor;
        if      (playerColour == Color.red)    { return player.GetTotalResourceCount() > 0;      }
        else if (playerColour == Color.yellow) { return compPlayer1.GetTotalResourceCount() > 0; }
        else if (playerColour == Color.white)  { return compPlayer2.GetTotalResourceCount() > 0; }
        else if (playerColour == Color.blue)   { return compPlayer3.GetTotalResourceCount() > 0; }
        else { return false; }
    }

    //This selects a resource to steal from a given player.
    private int SelectResourceToSteal(Color playerToStealFromColour)
    {
        int resource = Random.RandomRange(1, 6);
        if (playerToStealFromColour == Color.red && player.GetTotalResourceCount() > 0) //human player
        {
            while (!player.CheckIfGotOneResource(resource)) { resource = Random.RandomRange(1, 6); }
            player.GainOrLoseResource(resource, -1);
        }
        else if (playerToStealFromColour == Color.yellow && compPlayer1.GetTotalResourceCount() > 0) //computer player 1
        {
            while (!compPlayer1.CheckIfGotOneResource(resource)) { resource = Random.RandomRange(1, 6); }
            compPlayer1.GainOrLoseResource(resource, -1);
        }
        else if (playerToStealFromColour == Color.white && compPlayer2.GetTotalResourceCount() > 0)  //computer player 2
        {
            while (!compPlayer2.CheckIfGotOneResource(resource)) { resource = Random.RandomRange(1, 6); }
            compPlayer2.GainOrLoseResource(resource, -1);
        }
        else if (playerToStealFromColour == Color.blue && compPlayer3.GetTotalResourceCount() > 0)   //computer player 3
        {
            while (!compPlayer3.CheckIfGotOneResource(resource)) { resource = Random.RandomRange(1, 6); }
            compPlayer3.GainOrLoseResource(resource, -1);
        }
        else { resource = -1; }
        return resource;
    }

    //--------------------------------------------------------------------------------
    //Method for when a monopoly card is used in the player and computer classes

    //This gets the amount of a specific resource card that the player holds
    public int GetAllOfSpecificResourceOthersHold(int playerNumber, int card)
    {
        int totalAmount = 0;
        if (playerNumber != 1) { totalAmount = totalAmount + player.GetAmountOfResource(card); }
        if (playerNumber != 2) { totalAmount = totalAmount + compPlayer1.GetAmountOfResource(card); }
        if (playerNumber != 3) { totalAmount = totalAmount + compPlayer2.GetAmountOfResource(card); }
        if (playerNumber != 4) { totalAmount = totalAmount + compPlayer3.GetAmountOfResource(card); }
        return totalAmount;
    }

    //This removes all the specific given resource that every player other than the player
    //who is using the monopoly card has on them.
    public void MonopolyUsed(int playerNumber, int card)
    {
        if (playerNumber != 1) { player.LoseAllResourceMonopoly(card); }
        if (playerNumber != 2) { compPlayer1.LoseAllResourceMonopoly(card); }
        if (playerNumber != 3) { compPlayer2.LoseAllResourceMonopoly(card); }
        if (playerNumber != 4) { compPlayer3.LoseAllResourceMonopoly(card); }
    }

    //--------------------------------------------------------------------------------------------------------
    //Comparison method

    //This is the general comparison method that is used to compare two spots on the board.
    public bool SpotComparison(GameObject spot, GameObject otherSpot, int type)
    {
        float spotPosX = spot.GetComponent<RectTransform>().localPosition.x;
        float otherSpotPosX = otherSpot.GetComponent<RectTransform>().localPosition.x;
        float spotPosY = spot.GetComponent<RectTransform>().localPosition.y;
        float otherSpotPosY = otherSpot.GetComponent<RectTransform>().localPosition.y;

        float diffX = spotPosX - otherSpotPosX;
        float diffY = spotPosY - otherSpotPosY;
        bool isNeighbourXDir = false;
        bool isNeighbourYDir = false;

        if (type == 0) //road to road comparison
        {
            isNeighbourXDir = (diffX > -65 && diffX < -55) || (diffX > 55 && diffX < 65) || (diffX > -35 && diffX < -25) || (diffX > 25 && diffX < 35);
            isNeighbourYDir = (diffY > -50 && diffY < -40) || (diffY > 40 && diffY < 50) || (diffY > -5 && diffY < 5);
        }
        else if (type == 1) //road to settlement comparison
        {
            isNeighbourXDir = (diffX > -35 && diffX < -25) || (diffX > 25 && diffX < 35) || (diffX > -5 && diffX < 5);
            isNeighbourYDir = (diffY > -35 && diffY < -25) || (diffY > 25 && diffY < 35) || (diffY > 10 && diffY < 20) || (diffY > -20 && diffY < -10);
        }
        else if (type == 2) //settlement to settlement comparison
        {
            isNeighbourXDir = (diffX > -65 && diffX < -55) || (diffX > 55 && diffX < 65) || (diffX > -5 && diffX < 5);
            isNeighbourYDir = (diffY > -65 && diffY < -55) || (diffY > 55 && diffY < 65) || (diffY > 25 && diffY < 35) || (diffY > -35 && diffY < -25);
        }
        else if (type == 3) //settlement to hex comparison
        {
            isNeighbourXDir = (diffX > -65 && diffX < -55) || (diffX > 55 && diffX < 65) || (diffX > -5 && diffX < 5);
            isNeighbourYDir = (diffY > -65 && diffY < -55) || (diffY > 55 && diffY < 65) || (diffY > -35 && diffY < -25) || (diffY > 25 && diffY < 35);
        }

        return isNeighbourXDir && isNeighbourYDir;
    }

    //------------------------------------------------------------------------------
    //Method to calculate the longest road for a particular player

    //The following methods gets the longest road for given player.
    public int GetLongestRoad(int player)
    {
        Color playerColour = GetPlayerColour(player);
        int longest = 0;
        for (int i = 1; i < 73; i++) //goes through all the roads
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            if (theButton.colors.disabledColor == playerColour) //if the road belongs to the player 
            {
                List<GameObject> checkedRoadList = new List<GameObject>(); //since this is done via recursion, have to keep track of which roads have
                checkedRoadList.Add(roadSpot);                             //been checked. So this variable keeps track of it.
                int count = GetAmountOfConnectingRoadPieces(roadSpot, playerColour, checkedRoadList, -1);
                if (count > longest) { longest = count; }
            }
        }
        return longest;
    }

    //This is the main recursion method that gets the longest road length.
    private int GetAmountOfConnectingRoadPieces(GameObject roadSpot, Color playerColour, List<GameObject> checkedRoadList, int heading)
    {
        //this is the longest road count
        int totalCount = 1;
        //since roads can branch out, these methods keeps track of the length of the road going in branching directions.
        int upBranchCount = 0;
        int upRightBranchCount = 0;
        int rightBranchCount = 0;
        int downRightBranchCount = 0;
        int downBranchCount = 0;
        int downLeftBranchCount = 0;
        int leftBranchCount = 0;
        int upLeftBranchCount = 0;
        for (int i = 1; i < 73; i++) //goes through all the roads
        {
            GameObject otherRoadSpot = GameObject.Find("Road Button " + i);
            Button theButton = otherRoadSpot.GetComponent<Button>();
            //checks if the roadSpot isn't the same as the other road spot, and checks if the other road spot has already been checked
            //and if the other road spot belongs to the player
            if (roadSpot != otherRoadSpot && !checkedRoadList.Contains(otherRoadSpot) && theButton.colors.disabledColor == playerColour)
            {
                //checks if the two roadspots are next to each other
                //and if there is a opponent settlement not blocking the two road spots
                //and if the other road spot is going in the correct direction
                if (SpotComparison(roadSpot, otherRoadSpot, 0)
                    && !CheckIfOpponentsSettlementBlocksRoadPath(roadSpot, otherRoadSpot, playerColour)
                    && CheckIfGoingTowardsHeading(roadSpot, otherRoadSpot, heading))
                {
                    checkedRoadList.Add(otherRoadSpot); //add the other road spot to the checked road list.
                    int h = GetHeading(roadSpot, otherRoadSpot); //get the new direction heading going from road spot to the other road spot
                    //depending on what the heading is, then it increments one to the specific heading count and does a recursion
                    if (h == 0) { upBranchCount        = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 0); }
                    if (h == 1) { upRightBranchCount   = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 1); }
                    if (h == 2) { rightBranchCount     = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 2); }
                    if (h == 3) { downRightBranchCount = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 3); }
                    if (h == 4) { downBranchCount      = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 4); }
                    if (h == 5) { downLeftBranchCount  = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 5); }
                    if (h == 6) { leftBranchCount      = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 6); }
                    if (h == 7) { upLeftBranchCount    = 1 + GetAmountOfConnectingRoadPieces(otherRoadSpot, playerColour, checkedRoadList, 7); }
                }
            }
        }

        //Then after all that it gets whatever was the highest road length of the branches
        int[] allBranchCounts = new int[8];
        allBranchCounts[0] = upBranchCount;
        allBranchCounts[1] = upRightBranchCount;
        allBranchCounts[2] = rightBranchCount;
        allBranchCounts[3] = downRightBranchCount;
        allBranchCounts[4] = downBranchCount;
        allBranchCounts[5] = downLeftBranchCount;
        allBranchCounts[6] = leftBranchCount;
        allBranchCounts[7] = upLeftBranchCount;

        totalCount = GetTheMostBranch(allBranchCounts);
        return totalCount;
    }

    //This method gets the highest road length of the branches.
    private int GetTheMostBranch(int[] allBranchCounts)
    {
        int currMost = 0;
        int most = 0;
        for (int i = 1; i<allBranchCounts.Length; i++)
        {
            if (allBranchCounts[i] >= allBranchCounts[currMost])
            {
                most = allBranchCounts[i];
                currMost = i;
            }
            else { most = allBranchCounts[currMost]; }
        }
        if (most < 1) { most = 1; }
        return most;
    }

    //This method gets the heading direction going from one road spot to the other
    private int GetHeading(GameObject spot, GameObject otherSpot)
    {
        float spotPosX = spot.GetComponent<RectTransform>().localPosition.x;
        float otherSpotPosX = otherSpot.GetComponent<RectTransform>().localPosition.x;
        float spotPosY = spot.GetComponent<RectTransform>().localPosition.y;
        float otherSpotPosY = otherSpot.GetComponent<RectTransform>().localPosition.y;

        float diffX = spotPosX - otherSpotPosX;
        float diffY = spotPosY - otherSpotPosY;
        bool isFacingVertical= otherSpot.GetComponent<RectTransform>().eulerAngles.z == 90; // if its facing vertical then it has to either be going up or down
        bool up        = ((diffX > -35 && diffX < -25) || (diffX > 25  && diffX < 35)) && (diffY > -50 && diffY < -40)  && isFacingVertical;
        bool upRight   = ( diffX > -35 && diffX < -25) && (diffY > -50 && diffY < -40) && !isFacingVertical;
        bool right     = ( diffX > -65 && diffX < -55) && (diffY > -5  && diffY <   5) && !isFacingVertical;
        bool downRight = ( diffX > -35 && diffX < -25) && (diffY > 40  && diffY <  50) && !isFacingVertical;
        bool down      = ((diffX > -35 && diffX < -25) || (diffX > 25  && diffX < 35)) && (diffY > 40  && diffY < 50)   && isFacingVertical;
        bool downLeft  = ( diffX > 25  && diffX <  35) && (diffY > 40  && diffY <  50) && !isFacingVertical;
        bool left      = ( diffX > 55  && diffX <  65) && (diffY > -5  && diffY <   5) && !isFacingVertical;
        bool upLeft    = ( diffX > 25  && diffX <  35) && (diffY > -50 && diffY < -40) && !isFacingVertical;

        if      (up)        { return 0;  }
        else if (upRight)   { return 1;  }
        else if (right)     { return 2;  }
        else if (downRight) { return 3;  }
        else if (down)      { return 4;  }
        else if (downLeft)  { return 5;  }
        else if (left)      { return 6;  }
        else if (upLeft)    { return 7;  }
        else                { return -1; }
    }

    //Checks if going from one road spot to the other matches with what the current heading direction is.
    private bool CheckIfGoingTowardsHeading(GameObject spot, GameObject otherSpot, int heading)
    {
        int h = GetHeading(spot, otherSpot);

        if      (heading == 0 || heading == 4) { return isInCorrectDiagonalDirection(spot, otherSpot, heading, h); }
        else if (heading == 2 || heading == 6) { return ifOtherSpotIsAheadOrBehind(spot, otherSpot, heading, h); }
        else if (heading == 1)                 { return h == 0 || h == 2; }
        else if (heading == 3)                 { return h == 2 || h == 4; }
        else if (heading == 5)                 { return h == 4 || h == 6; }
        else if (heading == 7)                 { return h == 0 || h == 0; }
        else                                   { return heading == -1; }
    }

    //Checks if the going from one road spot to the other means that I'm going backwards compared in respect to the current heading.
    private bool ifOtherSpotIsAheadOrBehind(GameObject spot, GameObject otherSpot, int currHeading, int headingOfNextSpot)
    {
        float spotPosX = spot.GetComponent<RectTransform>().localPosition.x;
        float otherSpotPosX = otherSpot.GetComponent<RectTransform>().localPosition.x;
        float spotPosY = spot.GetComponent<RectTransform>().localPosition.y;
        float otherSpotPosY = otherSpot.GetComponent<RectTransform>().localPosition.y;

        float diffX = spotPosX - otherSpotPosX;
        float diffY = spotPosY - otherSpotPosY;
        if      (headingOfNextSpot == 0) { return (currHeading == 2 && diffX < 0) || (currHeading == 6 && diffX > 0) && diffY < 0; }
        else if (headingOfNextSpot == 4) { return (currHeading == 2 && diffX < 0) || (currHeading == 6 && diffX > 0) && diffY > 0; }
        else                             { return currHeading == headingOfNextSpot; }
    }

    //Checks if going from one road spot to the other is going in the correct diagonal direction
    private bool isInCorrectDiagonalDirection(GameObject spot, GameObject otherSpot, int currHeading, int headingOfNextSpot)
    {
        if      (currHeading == 0) { return headingOfNextSpot == 1 || headingOfNextSpot == 7; }
        else if (currHeading == 4) { return headingOfNextSpot == 3 || headingOfNextSpot == 5; }
        else return false;
    }

    //-------------------------------------------------------------------------------------------------------
    //These methods are to help set up the trading window

    //This method sets up the buttons and counters for the domestic and maritime section of the trade window
    public void GenerateLegalDMGiveButtons(int playerNumber, GameObject[] giveDMSelectionButtons, GameObject[] getDMSelectionButtons)
    {
        int playerBrickCount = player.GetBrickResourceCount();
        int playerGrainCount = player.GetGrainResourceCount();
        int playerLumberCount = player.GetLumberResourceCount();
        int playerOreCount = player.GetOreResourceCount();
        int playerWoolCount = player.GetWoolResourceCount();

        GameObject.Find("TradePanel").GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        giveDMSelectionButtons[0].GetComponentInChildren<Text>().text  = playerBrickCount.ToString();
        giveDMSelectionButtons[1].GetComponentInChildren<Text>().text  = playerGrainCount.ToString();
        giveDMSelectionButtons[2].GetComponentInChildren<Text>().text = playerLumberCount.ToString();
        giveDMSelectionButtons[3].GetComponentInChildren<Text>().text    = playerOreCount.ToString();
        giveDMSelectionButtons[4].GetComponentInChildren<Text>().text   = playerWoolCount.ToString();

        for (int i = 0; i< 5; i++)
        {
            giveDMSelectionButtons[i].GetComponent<Button>().interactable = false;
            giveDMSelectionButtons[i].GetComponent<Image>().color = Color.white;
            getDMSelectionButtons[i].GetComponent<Button>().interactable = false;
            getDMSelectionButtons[i].GetComponent<Image>().color = Color.white;
        }

        int minimumBrickNeededToGive  = getMinimumNeededToGiveForResource(1, playerNumber);
        int minimumGrainNeededToGive  = getMinimumNeededToGiveForResource(2, playerNumber);
        int minimumLumberNeededToGive = getMinimumNeededToGiveForResource(3, playerNumber);
        int minimumOreNeededToGive    = getMinimumNeededToGiveForResource(4, playerNumber);
        int minimumWoolNeededToGive   = getMinimumNeededToGiveForResource(5, playerNumber);

        GameObject.Find("Brick card DM min count").GetComponent<Text>().text  = minimumBrickNeededToGive.ToString();
        GameObject.Find("Grain card DM min count").GetComponent<Text>().text  = minimumGrainNeededToGive.ToString();
        GameObject.Find("Lumber card DM min count").GetComponent<Text>().text = minimumLumberNeededToGive.ToString();
        GameObject.Find("Ore card DM min count").GetComponent<Text>().text    = minimumOreNeededToGive.ToString();
        GameObject.Find("Wool card DM min count").GetComponent<Text>().text   = minimumWoolNeededToGive.ToString();

        if (playerBrickCount >= minimumBrickNeededToGive)   { GameObject.Find("Brick Card DM Give Button").GetComponent<Button>().interactable  = true; }
        if (playerGrainCount >= minimumGrainNeededToGive)   { GameObject.Find("Grain Card DM Give Button").GetComponent<Button>().interactable  = true; }
        if (playerLumberCount >= minimumLumberNeededToGive) { GameObject.Find("Lumber Card DM Give Button").GetComponent<Button>().interactable = true; }
        if (playerOreCount >= minimumOreNeededToGive)       { GameObject.Find("Ore Card DM Give Button").GetComponent<Button>().interactable    = true; }
        if (playerWoolCount >= minimumWoolNeededToGive)     { GameObject.Find("Wool Card DM Give Button").GetComponent<Button>().interactable   = true; }

    }

    //This method gets the minimum needed for the player to give in a domestic and maritime trade
    //for a specific resource. This does this by checking whether the player has any settlements in one of the
    //spots that are next to one of the harbours and gets the correct minimum value for that specific resource 
    //card based on which harbour it is next to.
    public int getMinimumNeededToGiveForResource(int card, int playerNumber)
    {
        int minimum = 4;
        string resource = "";
        if      (card == 1) { resource = "Brick";  }
        else if (card == 2) { resource = "Grain";  }
        else if (card == 3) { resource = "Lumber"; }
        else if (card == 4) { resource = "Ore";    }
        else if (card == 5) { resource = "Wool";   }

        foreach (KeyValuePair<GameObject, GameObject> keyValue in listOfSpotsAndTheirHarbours)
        {
            bool coloursMatch    = keyValue.Key.GetComponent<Button>().colors.disabledColor == GetPlayerColour(playerNumber);
            bool correctResource = keyValue.Value.GetComponent<Image>().sprite.name.Contains(resource);
            if (coloursMatch && correctResource)
            {
                return 2;
            }
            else if (coloursMatch && keyValue.Value.GetComponent<Image>().sprite.name.Contains("Any")){
                minimum = 3;
            }
        }
        return minimum;
    }

    //------------------------------------------------------------------------------
    //Computer making trade method. Didn't know where else to put this.

    //This method gets the resources that the computer wants to give and get and sends this trade offer out to
    //the other players. The computer players will respond quickly after thinking about the decision while the player 
    //responds via the computer trade window that appears
    public bool[] ComputerTradeOffer(int[] resourcesGiven, int[] resourcesWanted, int playerNumber)
    {
        bool[] responses = new bool[2];
        int count = 0;
        if (playerNumber != 2) { responses[count] = compPlayer1.GetTradeOffer(resourcesWanted, resourcesGiven, playerNumber); count++; }
        if (playerNumber != 3) { responses[count] = compPlayer2.GetTradeOffer(resourcesWanted, resourcesGiven, playerNumber); count++; }
        if (playerNumber != 4) { responses[count] = compPlayer3.GetTradeOffer(resourcesWanted, resourcesGiven, playerNumber); count++; }
        player.GetTradeOffer(resourcesGiven, resourcesWanted, playerNumber);
        return responses;
    }

    //This is the method that is called when the player confirms who they want to finalise the trade with.
    public void ComputerFinaliseTrade(int card, int resourceGiven, int resourceWanted, int playerNumber, int oppNumber)
    {
        if (oppNumber == 1)
        {
            player.GainOrLoseResource(card, resourceGiven);
            player.GainOrLoseResource(card, resourceWanted * -1);
        }
        else if (oppNumber == 2)
        {
            compPlayer1.GainOrLoseResource(card, resourceGiven);
            compPlayer1.GainOrLoseResource(card, resourceWanted * -1);
        }
        else if (oppNumber == 3)
        {
            compPlayer2.GainOrLoseResource(card, resourceGiven);
            compPlayer2.GainOrLoseResource(card, resourceWanted * -1);
        }
        else if (oppNumber == 4)
        {
            compPlayer3.GainOrLoseResource(card, resourceGiven);
            compPlayer3.GainOrLoseResource(card, resourceWanted * -1);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    //This is for setting up the board at the start of the game

    //These methods sets up the board at the start of the game. It does this by using the 
    //values that were saved in the player prefs manager.
    public void SetUpBoard()
    {
        for (int i = 1; i < 20; i++)
        {
            //get terrain tiles
            string terrain = PlayerPrefsManager.GetTerrainTile(i);
            GameObject.Find("Hex " + i).GetComponent<Image>().sprite = terrainPrefabs[GetIndexTerrain(terrain)];

            //get tokens
            string token = PlayerPrefsManager.GetToken(i);
            GameObject.Find("Token " + i).GetComponent<Image>().sprite = tokenPrefabs[GetIndexToken(token)];

            //save harbours
            if (i < 19)
            {
                string harbour = PlayerPrefsManager.GetHarbour(i);
                if (harbour == "null")
                {
                    GameObject.Find("Harbour " + i).GetComponent<Image>().enabled     = false;
                    GameObject.Find("Arrow " + i + "a").GetComponent<Image>().enabled = false;
                    GameObject.Find("Arrow " + i + "b").GetComponent<Image>().enabled = false;
                    if (GameObject.Find("Arrow " + i + "c")) { GameObject.Find("Arrow " + i + "c").GetComponent<Image>().enabled = false; }
                }
                else {
                    GameObject.Find("Harbour " + i).GetComponent<Image>().sprite = harbourPrefabs[GetIndexHarbour(harbour)];
                    if(i % 2 == 0) { evenHarbour = true; }
                }
            }
        }
        GenerateListOfSpotsNextToHarbours();
    }

    //These are helper methods that helps the set up board method get the correct type.
    private int GetIndexTerrain(string terrain)
    {
        if      (terrain == "Desert tile")   { return 0;  }
        else if (terrain == "Field tile")    { return 1;  }
        else if (terrain == "Forest tile")   { return 2;  }
        else if (terrain == "Hill tile")     { return 3;  }
        else if (terrain == "Mountain tile") { return 4;  }
        else if (terrain == "Pasture tile")  { return 5;  }
        else                                 { return -1; }
    }

    private int GetIndexToken(string token)
    {
        if      (token == "2 token")  { return 0;  }
        else if (token == "3 token")  { return 1;  }
        else if (token == "4 token")  { return 2;  }
        else if (token == "5 token")  { return 3;  }
        else if (token == "6 token")  { return 4;  }
        else if (token == "8 token")  { return 5;  }
        else if (token == "9 token")  { return 6;  }
        else if (token == "10 token") { return 7;  }
        else if (token == "11 token") { return 8;  }
        else if (token == "12 token") { return 9;  }
        else if (token == "Robber")   { return 10; }
        else                          { return -1; }
    }

    private int GetIndexHarbour(string harbour)
    {
        if      (harbour == "2 for 1 Brick")  { return 0;  }
        else if (harbour == "2 for 1 Grain")  { return 1;  }
        else if (harbour == "2 for 1 Lumber") { return 2;  }
        else if (harbour == "2 for 1 Ore")    { return 3;  }
        else if (harbour == "2 for 1 Wool")   { return 4;  }
        else if (harbour == "3 for 1 Any")    { return 5;  }
        else                                  { return -1; }
    }

    //These methods generates the list of settlements that are next to the harbours such that when 
    //it comes to getting the minimum resources needed for a domestic maritime trade then it will look through
    //this list.
    private void GenerateListOfSpotsNextToHarbours()
    {
        listOfSpotsAndTheirHarbours = new Dictionary<GameObject, GameObject>();
        for (int i = 1; i < 19; i++)
        {
            if((evenHarbour && i % 2 == 0) || (!evenHarbour && i % 2 != 0))
            {
                GameObject harbour = GameObject.Find("Harbour " + i);
                int[] spotsNearHarbour = GetSpotsNearHarbour(i);
                for(int j=0; j < spotsNearHarbour.Length; j++)
                {
                    if(spotsNearHarbour[j] != -1) {
                        GameObject spot = GameObject.Find("Settlement/City button " + spotsNearHarbour[j]);
                        listOfSpotsAndTheirHarbours.Add(spot, harbour);
                    }
                }
            }
        }
    }

    //This has all the possible spots that are near a harbour (represented by the position variable)
    private int[] GetSpotsNearHarbour(int position)
    {
        int[] spotsNearHarbour = new int[3];
        if      (position == 1)  { spotsNearHarbour[0] = 17; spotsNearHarbour[1] = 28; spotsNearHarbour[2] = -1; }
        else if (position == 2)  { spotsNearHarbour[0] =  8; spotsNearHarbour[1] = 17; spotsNearHarbour[2] = 18; }
        else if (position == 3)  { spotsNearHarbour[0] =  1; spotsNearHarbour[1] =  8; spotsNearHarbour[2] =  9; }
        else if (position == 4)  { spotsNearHarbour[0] =  1; spotsNearHarbour[1] =  2; spotsNearHarbour[2] = -1; }
        else if (position == 5)  { spotsNearHarbour[0] =  2; spotsNearHarbour[1] =  3; spotsNearHarbour[2] =  4; }
        else if (position == 6)  { spotsNearHarbour[0] =  4; spotsNearHarbour[1] =  5; spotsNearHarbour[2] =  6; }
        else if (position == 7)  { spotsNearHarbour[0] =  6; spotsNearHarbour[1] =  7; spotsNearHarbour[2] = -1; }
        else if (position == 8)  { spotsNearHarbour[0] =  7; spotsNearHarbour[1] = 15; spotsNearHarbour[2] = 16; }
        else if (position == 9)  { spotsNearHarbour[0] = 16; spotsNearHarbour[1] = 26; spotsNearHarbour[2] = 27; }
        else if (position == 10) { spotsNearHarbour[0] = 27; spotsNearHarbour[1] = 38; spotsNearHarbour[2] = -1; }
        else if (position == 11) { spotsNearHarbour[0] = 37; spotsNearHarbour[1] = 38; spotsNearHarbour[2] = 47; }
        else if (position == 12) { spotsNearHarbour[0] = 46; spotsNearHarbour[1] = 47; spotsNearHarbour[2] = 54; }
        else if (position == 13) { spotsNearHarbour[0] = 53; spotsNearHarbour[1] = 54; spotsNearHarbour[2] = -1; }
        else if (position == 14) { spotsNearHarbour[0] = 51; spotsNearHarbour[1] = 52; spotsNearHarbour[2] = 53; }
        else if (position == 15) { spotsNearHarbour[0] = 49; spotsNearHarbour[1] = 50; spotsNearHarbour[2] = 51; }
        else if (position == 16) { spotsNearHarbour[0] = 48; spotsNearHarbour[1] = 49; spotsNearHarbour[2] = -1; }
        else if (position == 17) { spotsNearHarbour[0] = 39; spotsNearHarbour[1] = 40; spotsNearHarbour[2] = 48; }
        else if (position == 18) { spotsNearHarbour[0] = 28; spotsNearHarbour[1] = 29; spotsNearHarbour[2] = 39; }
        else                     { spotsNearHarbour[0] = -1; spotsNearHarbour[1] = -1; spotsNearHarbour[2] = -1; }

        return spotsNearHarbour;
    }

    //-------------------------------------------------------------------------
    //These methods are for the AI class to call

    //This gets the weight value for a specific settlement spot
    public int GetWeightForSettlementCitySpot(GameObject settlementSpot, bool[] resourcesWanted, bool isSettlementPlacement, string aiSetting, int playerNumber)
    {
        int weight = 0; //weight is initialised to 0

        //these are for the All Around Thinker ai setting
        int roadBuildWeight = 0;
        bool[] roadBuildCheck = { false, false }; //order of brick and lumber

        int settlementBuildWeight = 0;
        bool[] settlementBuildCheck = { false, false, false, false }; //order of brick, lumber, wool and grain

        int cityBuildWeight = 0;
        int cityOreCount = 3;
        int cityGrainCount = 2;

        int devCardBuildWeight = 0;
        bool[] devCardBuildCheck = { false, false, false }; //order of ore, wool and grain

        //for the All Around Thinker AI this gets the numbered tokens that have been already covered by a settlement placement
        //so that later it can prioritise numbered tokens that haven't been covered.
        List<int> listOfNumberedTokensUsed = GetNumberedTokensAlreadyGot(playerNumber);
        if (!isSettlementPlacement || aiSetting != "All Around Thinker") { listOfNumberedTokensUsed.RemoveRange(0, listOfNumberedTokensUsed.Count); };

        //this gets the weights of each hex
        for (int i=1; i<20; i++) //goes through all the hexes
        {
            GameObject hex = GameObject.Find("Hex " + i);
            GameObject token = GameObject.Find("Token " + i);
            if (SpotComparison(settlementSpot, hex, 3)) //if the settlement spot is bordering the hex
            {
                if (hex.GetComponent<Image>().sprite.name != "Desert tile") //if its a desert hex tile then its got a weight value of 0
                {
                    if (((hex.GetComponent<Image>().sprite.name  == "Hill tile"     && resourcesWanted[0])  //brick resource
                       ||(hex.GetComponent<Image>().sprite.name  == "Field tile"    && resourcesWanted[1])  //grain resource
                       ||(hex.GetComponent<Image>().sprite.name  == "Forest tile"   && resourcesWanted[2])  //lumber resource
                       ||(hex.GetComponent<Image>().sprite.name  == "Mountain tile" && resourcesWanted[3])  //ore resource
                       ||(hex.GetComponent<Image>().sprite.name  == "Pasture tile"  && resourcesWanted[4])) //wool resource
                       && aiSetting != "Quick Thinker") { weight = weight + 20; } //if the hex contains a resource that the player wants then it adds on a larger weight
                    else                                { weight = weight + 10; } //than if it was a hex that contains a resource that the player didn't want

                    //this checks for which numbered token is on the hex. It will add more weight for favourable numbered tokens
                    //If the robber is it then weight is lost
                    if (aiSetting != "Quick Thinker")
                    {
                        bool hasRobber = (token.GetComponent<Image>().sprite.name == "Robber");
                        if (hasRobber) { weight = weight - 5; } 
                        else if ((token.GetComponent<Image>().sprite.name == "6 token"  && !listOfNumberedTokensUsed.Contains(6)) 
                              || (token.GetComponent<Image>().sprite.name == "8 token"  && !listOfNumberedTokensUsed.Contains(8)))  { weight = weight + 10; }

                        else if ((token.GetComponent<Image>().sprite.name == "5 token"  && !listOfNumberedTokensUsed.Contains(5)) 
                              || (token.GetComponent<Image>().sprite.name == "9 token"  && !listOfNumberedTokensUsed.Contains(9)))  { weight = weight + 8; }

                        else if ((token.GetComponent<Image>().sprite.name == "4 token"  && !listOfNumberedTokensUsed.Contains(4)) 
                              || (token.GetComponent<Image>().sprite.name == "10 token" && !listOfNumberedTokensUsed.Contains(10))) { weight = weight + 6; }

                        else if ((token.GetComponent<Image>().sprite.name == "3 token"  && !listOfNumberedTokensUsed.Contains(3)) 
                              || (token.GetComponent<Image>().sprite.name == "11 token" && !listOfNumberedTokensUsed.Contains(11))) { weight = weight + 4; }

                        else if ((token.GetComponent<Image>().sprite.name == "2 token"  && !listOfNumberedTokensUsed.Contains(2)) 
                              || (token.GetComponent<Image>().sprite.name == "12 token" && !listOfNumberedTokensUsed.Contains(12))) { weight = weight + 2; }
                    }

                    //this checks for the hex which build plans it contributes toward. This is for the All Around Thinker AI agent
                    if (aiSetting == "All Around Thinker")
                    {
                        if (hex.GetComponent<Image>().sprite.name == "Hill tile"  && !roadBuildCheck[0] && !settlementBuildCheck[0])
                        {
                            roadBuildCheck[0] = true;
                            roadBuildWeight = roadBuildWeight + 10;
                            settlementBuildCheck[0] = true;
                            settlementBuildWeight = settlementBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Forest tile" && !roadBuildCheck[1] && !settlementBuildCheck[1])
                        {
                            roadBuildCheck[1] = true;
                            roadBuildWeight = roadBuildWeight + 10;
                            settlementBuildCheck[1] = true;
                            settlementBuildWeight = settlementBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Pasture tile" && !settlementBuildCheck[2] && !devCardBuildCheck[1])
                        {
                            settlementBuildCheck[2] = true;
                            settlementBuildWeight = settlementBuildWeight + 10;
                            devCardBuildCheck[1] = true;
                            devCardBuildWeight = devCardBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Field tile" && !settlementBuildCheck[3] && !devCardBuildCheck[2])
                        {
                            settlementBuildCheck[3] = true;
                            settlementBuildWeight = settlementBuildWeight + 10;
                            devCardBuildCheck[2] = true;
                            devCardBuildWeight = devCardBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Mountain tile" && !devCardBuildCheck[0])
                        {
                            devCardBuildCheck[0] = true;
                            devCardBuildWeight = devCardBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Mountain tile" && cityOreCount > 0)
                        {
                            cityOreCount--;
                            cityBuildWeight = cityBuildWeight + 10;
                        }
                        if (hex.GetComponent<Image>().sprite.name == "Field tile" && cityGrainCount > 0)
                        {
                            cityGrainCount--;
                            cityBuildWeight = cityBuildWeight + 10;
                        }

                    }

                    // The All Around Thinker AI agent will priortise hexes that don't already have one of their settlements on it.
                    if (IsThereAnotherSettlementOnThisHex(hex, settlementSpot, playerNumber) && aiSetting == "All Around Thinker") { weight = weight - 50; }
                }
            }
        }
        if (isSettlementPlacement && aiSetting != "Quick Thinker") //in the event that a city is being placed then this part about checking the harbours is ignored as it doesn't change anything if a city is next to a harbour
        {
            for (int h = 1; h < 19; h++) //goes through all harbours 
            {
                GameObject harbour = GameObject.Find("Harbour " + h);
                if (harbour.GetComponent<Image>().enabled)
                {
                    string harbourTradeResource = harbour.GetComponent<Image>().sprite.name.Substring(8);
                    int[] spotsNearHarbour = GetSpotsNearHarbour(h);
                    int spotNumber = int.Parse(settlementSpot.name.Substring(23));
                    if (spotsNearHarbour[0] == spotNumber || spotsNearHarbour[1] == spotNumber || spotsNearHarbour[2] == spotNumber) //if the settlement spot is next to a harbour then add a weight
                    {
                        weight = weight + 15;
                    }
                }
            }
        }
        //for when a settlement spot has hexes that fufills the road build plan, then add 10 to the weight
        if (roadBuildWeight == 20) { roadBuildWeight = roadBuildWeight + 10; }
        //then get the highest weight from the different build plans. The most should be 30
        int[] weights = { roadBuildWeight, settlementBuildWeight, cityBuildWeight, devCardBuildWeight };
        int most = GetTheMostWeight(weights);
        weight = weight + most;
        return weight;
    }

    //A helper method to get the highest weight from a list of weights
    private int GetTheMostWeight(int[] weights)
    {
        int currMost = 0;
        int most = 0;
        for (int i = 1; i < weights.Length; i++)
        {
            if (weights[i] >= weights[currMost])
            {
                most = weights[i];
                currMost = i;
            }
            else { most = weights[currMost]; }
        }
        return most;
    }

    //This method gets a list of the numbered tokens that have already been covered by
    //settlement placements
    private List<int> GetNumberedTokensAlreadyGot(int playerNumber)
    {
        Color playerColour = GetPlayerColour(playerNumber);
        List<int> listOfTokensUsed = new List<int>();
        for (int j = 1; j < 55; j++) //go through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + j);

            if (settlementSpot.GetComponent<Button>().colors.disabledColor == playerColour)
            {
                for (int i = 1; i < 20; i++) //goes through all the hexes
                {
                    GameObject hex = GameObject.Find("Hex " + i);
                    GameObject token = GameObject.Find("Token " + i);
                    if (SpotComparison(settlementSpot, hex, 3) && (hex.GetComponent<Image>().sprite.name != "Desert tile") && token.GetComponent<Image>().sprite.name != "Robber")
                    {
                        listOfTokensUsed.Add(int.Parse(token.GetComponent<Image>().sprite.name.Substring(0, 2)));
                    }
                }
            }
        }
        return listOfTokensUsed;
    }

    //This method checks if there is another settlement that belongs to the player that is bordering a given hex
    private bool IsThereAnotherSettlementOnThisHex(GameObject hex, GameObject settlementSpot, int playerNumber)
    {
        Color playerColour = GetPlayerColour(playerNumber);
        for (int j = 1; j < 55; j++) //go through all the settlement spots
        {
            GameObject otherSettlementSpot = GameObject.Find("Settlement/City button " + j);
            if (SpotComparison(otherSettlementSpot, hex, 3) && settlementSpot != otherSettlementSpot && otherSettlementSpot.GetComponent<Button>().colors.disabledColor == playerColour)
            {
                return true;
            }
        }
        return false;
    }

    //This gets the weight value for a specific road spot
    public int GetWeightForRoadSpot(GameObject roadSpot, bool[] resourcesWanted, int playerNumber, string priority, string aiSetting)
    {
        int weight = 0; //weight is initialised to 0
        Color playerColour = GetPlayerColour(playerNumber);
        int currentLRscore = int.Parse(GameObject.Find("Roads P" + playerNumber).GetComponent<Text>().text); //gets the current longest road score

        if (aiSetting != "Quick Thinker" && !ScoreBoard.DoesPlayerHoldLongestRoad(playerNumber))
        {
            List<GameObject> checkedRoadList = new List<GameObject>();
            checkedRoadList.Add(roadSpot);
            int count = GetAmountOfConnectingRoadPieces(roadSpot, playerColour, checkedRoadList, -1); //This gets the longest road from this new road spot.

            if (count > currentLRscore)     { weight = weight + 10; } //if placing a new road on this road spot causes the longest road score for that player to improve, then add a weight to it.
            if (count - currentLRscore > 1) { weight = weight + 5;  } //if placing a new road on this road spot causes the longest road score to increase by more than 1, then add an extra weight value onto it.
        }

        for (int i = 1; i < 55; i++) //go through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);
            //if the settlement spot is next to the road spot and it doesn't have a settlement on it and it obeys the distance rule, then add a weight to it.
            if (ObeysDistanceRule(settlementSpot) && SpotComparison(roadSpot,settlementSpot,1) 
                && settlementSpot.GetComponent<Image>().sprite.name == "UISprite")
            {
                if (priority == "settlement") { weight = weight + 20; } //if the player wants to build a settlement, then add extra weight to it
                else                          { weight = weight + 10; }
            }
        }
        return weight;
    }

    //The way that getting the weight of the robber spot works is that the computer prioritises 
    //picking a robber spot that has the most opponent settlement or city pieces around it.
    //If the robber spot has settlements or cities that belong to the player around it then it makes
    //sure to subtract a larger weight from this.
    public int GetWeightForRobberSpot(GameObject robberSpot, int playerNumber, string aiSetting)
    {
        int weight = 0;
        float robberSpotX = robberSpot.GetComponentInParent<RectTransform>().localPosition.x;
        float robberSpotY = robberSpot.GetComponentInParent<RectTransform>().localPosition.y;
        GameObject hex = new GameObject();
        hex.AddComponent<RectTransform>();
        hex.GetComponent<RectTransform>().localPosition = new Vector3(robberSpotX,robberSpotY,0);
        Color playerColour = GetPlayerColour(playerNumber);
        GameObject token = GameObject.Find("Token " + int.Parse(robberSpot.name.Substring(6)));
        for (int i = 1; i < 55; i++) //goes through all the settlement spots
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);
            if (SpotComparison(settlementSpot, hex, 3) && settlementSpot.GetComponent<Image>().sprite.name != "UIsprite")
            {
                bool isCity = (settlementSpot.GetComponent<Image>().sprite.name == "City White"); //check if the settlement spot has a city on it. if it does then a bigger weight gets added or subtracted
                if (settlementSpot.GetComponent<Button>().colors.disabledColor == playerColour) //if the settlement or city belongs to the player then take away weight
                {
                    if (isCity) { weight = weight - 40; }
                    else        { weight = weight - 20; }
                }
                else //else if it doesn't the add on some weight
                {
                    if (isCity) { weight = weight + 20; }
                    else        { weight = weight + 10; }
                }

                if (aiSetting != "Quick Thinker")
                {
                    //also prioritise picking a spot that has a better numbered token on it.
                    if      (token.GetComponent<Image>().sprite.name == "6 token" || token.GetComponent<Image>().sprite.name == "8 token")  { weight = weight + 10; }
                    else if (token.GetComponent<Image>().sprite.name == "5 token" || token.GetComponent<Image>().sprite.name == "9 token")  { weight = weight + 8;  }
                    else if (token.GetComponent<Image>().sprite.name == "4 token" || token.GetComponent<Image>().sprite.name == "10 token") { weight = weight + 6;  }
                    else if (token.GetComponent<Image>().sprite.name == "3 token" || token.GetComponent<Image>().sprite.name == "11 token") { weight = weight + 4;  }
                    else if (token.GetComponent<Image>().sprite.name == "2 token" || token.GetComponent<Image>().sprite.name == "12 token") { weight = weight + 2;  }

                    //also prioritises picking a spot that targets the leader.
                    if ((playerNumber != 1 && settlementSpot.GetComponent<Button>().colors.disabledColor == GetPlayerColour(1) && ScoreBoard.DoesPlayerHaveHighestTotalScore(1))
                    ||  (playerNumber != 2 && settlementSpot.GetComponent<Button>().colors.disabledColor == GetPlayerColour(2) && ScoreBoard.DoesPlayerHaveHighestTotalScore(2))
                    ||  (playerNumber != 3 && settlementSpot.GetComponent<Button>().colors.disabledColor == GetPlayerColour(3) && ScoreBoard.DoesPlayerHaveHighestTotalScore(3))
                    ||  (playerNumber != 4 && settlementSpot.GetComponent<Button>().colors.disabledColor == GetPlayerColour(4) && ScoreBoard.DoesPlayerHaveHighestTotalScore(4)))
                    { weight = weight + 10; }
                }
            }
        }
        return weight;
    }

    //This checks if for a legal road spot, whether placing a road in that spot will lead to 
    //the player getting the longest road card.
    public bool DoesLegalRoadsLeadToGettingLongestRoadCard(int playerNumber)
    {
        Color playerColour = GetPlayerColour(playerNumber);
        List<GameObject> listOfValidSpots = new List<GameObject>();
        int currentLRscore = int.Parse(GameObject.Find("Roads P" + playerNumber).GetComponent<Text>().text); //gets the current longest road score
        for (int i = 1; i < 73; i++)
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            if (theButton.colors.disabledColor == black)
            {
                if (CheckForConnectingSettlements(roadSpot, playerColour)
                || CheckForConnectingRoads(roadSpot, 0, Game.GetInitial1(), Game.GetInitial2(), playerColour))
                {
                    listOfValidSpots.Add(roadSpot);
                    List<GameObject> checkedRoadList = new List<GameObject>();
                    checkedRoadList.Add(roadSpot);
                    int count = GetAmountOfConnectingRoadPieces(roadSpot, playerColour, checkedRoadList, -1); //This gets the longest road from this new road spot.
                    if (ScoreBoard.WillAddingThisRoadGivePlayerLongestRoad(playerNumber,count)) { return true; }
                }
            }
        }
        return false;
    }
}