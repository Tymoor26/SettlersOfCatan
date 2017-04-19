using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//This class deals with everything to do with the player actions.
//This ranges from generating the legal spots for building pieces
//such as settlements, roads and cities and spots to move the robber. It also handles when a piece is placed
//onto the board. It also updates all the counters to do with the player such as 
//the amount of each resource they are holding as well as dev cards.
//Another thing it handles is when a trade offer is received from another player.
//This class is similarly structured to the Computer class except this handles things thats
//specifically tuned to the player
public class Player : MonoBehaviour {
    public Sprite[] boardPieces;
    public Board board;
    public ScoreBoard scoreBoard;
    private Color playerColour = Color.red;
    //private int brickResourceCount = 50, grainResourceCount = 50, lumberResourceCount = 50, oreResourceCount = 50, woolResourceCount = 50;
    private int brickResourceCount = 0, grainResourceCount = 0, lumberResourceCount = 0, oreResourceCount = 0, woolResourceCount = 0;
    private int roadBuildingDevCount = 0, monopolyDevCount = 0, yearOfPlentyDevCount = 0, knightDevCount = 0, victoryPointDevCount = 0;
    private int totalResourceCardsInHand = 0;
    private int totalDevCardsInHand = 0;
    private int remainingRoadsToBuild = 15;
    private int remainingSettlementsToBuild = 5;
    private int remainingCitiesToBuild = 4;
    private bool isMovingRobber = false;
    private int legalSettlements = 0;
    private bool usingRoadBuildingCard = false;
    private int roadBuildingCardRoadsPlaced = 0;
    private bool usingDevCard = false;
    private bool usingMonopolyCard = false;
    private bool usingYearOfPlentyCard = false;
    private int yearOfPlentyCardsSelected = 0;
    private bool usingKnightCard = false;

    //---------------------------------------------------------------------------------
    //Piece building and placement methods

    //This method is called when a legal settlement spot is selected by the player.
    //This will update the counts accordingly as this will check whether its a
    //settlement or city is being built. Then it will add a sprite on that spot 
    //and set it to the players colour and disable that spot such that it can't be clicked again
    //unless its a settlement being upgraded to a city
    public void PlayerPlaceSettlementCity(int number)
    {
        GameObject settlementSpot = GameObject.Find("Settlement/City button " + number);
        Button theButton = settlementSpot.GetComponent<Button>();
        ColorBlock theColor = settlementSpot.GetComponent<Button>().colors;
        theColor.disabledColor = playerColour;
        theButton.colors = theColor;

        if (settlementSpot.GetComponent<Image>().sprite.name == "UISprite") //check if theres nothing on it. If so then a settlement is being built, otherwise its a city
        {
            settlementSpot.GetComponent<Image>().sprite = boardPieces[0];
            scoreBoard.SettlementPointScored(1);
            if (!Game.GetInitial1() && !Game.GetInitial2()) //if its not the initial turns, then make sure to update the resource counts
            {
                brickResourceCount--;
                lumberResourceCount--;
                grainResourceCount--;
                woolResourceCount--;
                Summary.AddToSummary("P1: Built settlement");
            }
            remainingSettlementsToBuild--;
        }
        else
        {
            settlementSpot.GetComponent<Image>().sprite = boardPieces[1];
            scoreBoard.CityPointScored(1);
            oreResourceCount = oreResourceCount - 3;
            grainResourceCount = grainResourceCount - 2;
            remainingSettlementsToBuild++;
            remainingCitiesToBuild--;
            Summary.AddToSummary("P1: Built city");
        }
        theButton.interactable = false;

        board.DeactivateSettlementPlacementsButtons();

        if      (Game.GetInitial1()) { GeneratePlayerLegalRoadPlacements(false); }
        else if (Game.GetInitial2()) { GeneratePlayerSecondRoadPlacements(settlementSpot); }

        UpdateCounters();
    }

    //This method is called when a legal road spot is selected by the player.
    //This will update the counts accordingly depending on whether the player is using
    //the road building card or not. Then it will change the colour of that spot
    //to match the players colour and then disable the button such that it can't be clicked
    //again.
    public void PlayerPlaceRoad(int number)
    {
        GameObject roadSpot = GameObject.Find("Road Button " + number);
        Button theButton = roadSpot.GetComponent<Button>();
        ColorBlock theColor = roadSpot.GetComponent<Button>().colors;
        theColor.disabledColor = playerColour;
        theButton.colors = theColor;
        theButton.interactable = false;
        scoreBoard.RoadPlaced(1);
        remainingRoadsToBuild--;
        board.DeactivateRoadPlacementButtons();

        if (!Game.GetInitial1() && !Game.GetInitial2() && !usingRoadBuildingCard) //if its not the initial turns and the player isn't using a road building card
        {
            brickResourceCount--;
            lumberResourceCount--;
            Summary.AddToSummary("P1: Built road");
        }
        else if (roadBuildingCardRoadsPlaced<1 && usingRoadBuildingCard) //else if the road building card is used and its the first piece being placed
        {
            roadBuildingCardRoadsPlaced++;
            GeneratePlayerLegalRoadPlacements(true);
            Game.DeactivatePlayerButtons();
            roadBuildingDevCount--;
            UpdateCounters();
            usingDevCard = true;
            GameObject.Find("End Turn Button").GetComponent<Button>().interactable = false;
            Summary.AddToSummary("P1: Road building card used");
            Summary.AddToSummary("P1: Built road");
        }
        else if (roadBuildingCardRoadsPlaced == 1 && usingRoadBuildingCard) //else if the road building card is used and its the second piece being placed
        {
            usingRoadBuildingCard = false;
            usingDevCard = false;
            roadBuildingCardRoadsPlaced = 0;
            GameObject.Find("End Turn Button").GetComponent<Button>().interactable = true;
            Summary.AddToSummary("P1: Built road");
        }

        if (usingRoadBuildingCard && remainingRoadsToBuild == 0)
        {
            usingRoadBuildingCard = false;
            usingDevCard = false;
            roadBuildingCardRoadsPlaced = 0;
            GameObject.Find("End Turn Button").GetComponent<Button>().interactable = true;
        }

        if (Game.GetInitial2())                       { GetInitialResources(board.GetSettlementConnectedWithRoad(roadSpot)); }
        if (Game.GetInitial1() || Game.GetInitial2()) { Game.InitialTurnDone(); }

        UpdateCounters();
    }

    //This method is called when the player builds a development card
    //What happens is that it gets all the remaining development cards,
    //puts them in a list in random order and then selects one from the list and
    //this is what the player gets.
    public void PlayerBuildDevCard()
    {
        int[] listOfDevCards = GetListOfAvailableDevCards();
        int p = Random.RandomRange(0, Game.GetTotalDevCardsRemaining());
        int card = listOfDevCards[p];

        oreResourceCount--;
        woolResourceCount--;
        grainResourceCount--;
        UpdateCounters();

        if (card == 1) //road buiding card recieved
        {
            roadBuildingDevCount++;
            Game.RoadBuildingCardBuilt();
        }
        else if(card == 2) //monopoly card recieved
        {
            monopolyDevCount++;
            Game.MonopolyCardBuilt();
        }
        else if (card == 3) //year of plenty card recieved
        {
            yearOfPlentyDevCount++;
            Game.YearOfPlentyCardBuilt();
        }
        else if (card == 4) //knight card recieved
        {
            knightDevCount++;
            Game.KnightCardBuilt();
        }
        else if (card == 5) //victory point card recieved
        {
            victoryPointDevCount++;
            Game.VictoryPointCardBuilt();
        }
        Summary.AddToSummary("P1: Built dev card");
        UpdateCounters();
    }

    //-----------------------------------------------------------------
    //Generation and helper methods for the piece build methods above

    //This gets the list of the available development cards. This puts them
    //in a list thats in random order.
    private int[] GetListOfAvailableDevCards()
    {
        int remainingDevCards = Game.GetTotalDevCardsRemaining();
        int[] listOfDevCards = new int[remainingDevCards];
        for (int i = 0; i < remainingDevCards; i++)
        {
            bool checker = false;
            while (!checker)
            {
                int r = Random.RandomRange(1, 6);
                if (r == 1 && Game.GetRoadBuildingCardsRemaining() > 0)
                {
                    listOfDevCards[i] = 1;
                    checker = true;
                }
                else if (r == 2 && Game.GetMonopolyCardsRemaining() > 0)
                {
                    listOfDevCards[i] = 2;
                    checker = true;
                }
                else if (r == 3 && Game.GetYearOfPlentyCardsRemaining() > 0)
                {
                    listOfDevCards[i] = 3;
                    checker = true;
                }
                else if (r == 4 && Game.GetKnightCardsRemaining() > 0)
                {
                    listOfDevCards[i] = 4;
                    checker = true;
                }
                else if (r == 5 && Game.GetVictoryPointCardsRemaining() > 0)
                {
                    listOfDevCards[i] = 5;
                    checker = true;
                }
            }
        }
        return listOfDevCards;
    }

    //This method generates all the legal settlement placement spots on the board for the player
    public void GeneratePlayerLegalSettlementPlacements()
    {
        for (int i = 1; i < 55; i++)
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);
            Button theButton = settlementSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            theButton.interactable = (theButton.colors.disabledColor == black) 
                && (board.ObeysDistanceRule(settlementSpot)) 
                && (board.CheckForConnectingRoads(settlementSpot, 1, Game.GetInitial1(), Game.GetInitial2(), playerColour));
        }
    }

    //This method generates all the legal road placement spots on the board for the 
    //player. If the road building card is being used then this makes sure to set the
    //roadBuildCardBeingUsed variables to true
    public void GeneratePlayerLegalRoadPlacements(bool roadBuildingCardBeingUsed)
    {
        if (roadBuildingCardBeingUsed) { usingRoadBuildingCard = true; }
        else
        {
            usingRoadBuildingCard = false;
            roadBuildingCardRoadsPlaced = 0;
        }
        for (int i = 1; i < 73; i++)
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            if (theButton.colors.disabledColor == black)
            {
                bool checkIfRoadSpotIsNearPlayerRoadOrSettlement = board.CheckForConnectingSettlements(roadSpot, playerColour) ||
                    board.CheckForConnectingRoads(roadSpot, 0, Game.GetInitial1(), Game.GetInitial2(), playerColour);
                if (checkIfRoadSpotIsNearPlayerRoadOrSettlement) { theButton.interactable = true;  }
                else                                             { theButton.interactable = false; }
            }
        }
    }

    //This method generates all the legal city placement spots for the player
    public void GenerateLegalCityPlacements()
    {
        for (int i = 1; i < 55; i++)
        {
            GameObject citySpot = GameObject.Find("Settlement/City button " + i);
            Button theButton = citySpot.GetComponent<Button>();
            if (theButton.colors.disabledColor == playerColour && citySpot.GetComponent<Image>().sprite == boardPieces[0]) { theButton.interactable = true;  }
            else                                                                                                           { theButton.interactable = false; }
        }
    }

    //--------------------------------------------------------------------------
    //These methods handles all the resource management

    //This method gets called when the dice roll happens at the start of each turn
    //If no resources are produced, then a messgae saying so is added to the summary table
    public void GetResources(int roll)
    {
        bool check = true;
        for (int i = 1; i < 20; i++) //goes through all the hexes
        {
            GameObject token = GameObject.Find("Token " + i);
            string tokenName = roll + " token";
            if (token.GetComponent<Image>().sprite.name == tokenName)
            {
                GameObject hex = GameObject.Find("Hex " + i);
                int amountOfResources = board.GetAmountOfResourcesFromHex(hex, playerColour);
                UpdateResourceCountsWithGivenHex(hex, amountOfResources);
                check = false;
            }
        }
        if (check) { Summary.AddToSummary("P1: Produced nothing"); }
    }

    //This method updates the resource counters for a given hex
    private void UpdateResourceCountsWithGivenHex(GameObject hex, int amountOfResources)
    {
        if (amountOfResources > 0)
        {
            if      (hex.GetComponent<Image>().sprite.name == "Pasture tile")  { woolResourceCount   = woolResourceCount   + amountOfResources; Summary.AddToSummary("P1: Produced " + amountOfResources + " wool resource");   }
            else if (hex.GetComponent<Image>().sprite.name == "Field tile")    { grainResourceCount  = grainResourceCount  + amountOfResources; Summary.AddToSummary("P1: Produced " + amountOfResources + " grain resource");  }
            else if (hex.GetComponent<Image>().sprite.name == "Forest tile")   { lumberResourceCount = lumberResourceCount + amountOfResources; Summary.AddToSummary("P1: Produced " + amountOfResources + " lumber resource"); }
            else if (hex.GetComponent<Image>().sprite.name == "Hill tile")     { brickResourceCount  = brickResourceCount  + amountOfResources; Summary.AddToSummary("P1: Produced " + amountOfResources + " brick resource");  }
            else if (hex.GetComponent<Image>().sprite.name == "Mountain tile") { oreResourceCount    = oreResourceCount    + amountOfResources; Summary.AddToSummary("P1: Produced " + amountOfResources + " ore resource");    }
            UpdateCounters();
        }
    }

    //This method updates the resource counters for a given resource card
    public void GainOrLoseResource(int card, int amount)
    {
        string gainOrLose = "";
        if (amount < 0) { gainOrLose = "lost"; }
        else { gainOrLose = "gained"; }
        if (amount != 0)
        {
            if      (card == 1) { brickResourceCount  = brickResourceCount  + amount; Summary.AddToSummary("P1: " + amount + " Brick resource "  + gainOrLose); }
            else if (card == 2) { grainResourceCount  = grainResourceCount  + amount; Summary.AddToSummary("P1: " + amount + " Grain resource "  + gainOrLose); }
            else if (card == 3) { lumberResourceCount = lumberResourceCount + amount; Summary.AddToSummary("P1: " + amount + " Lumber resource " + gainOrLose); }
            else if (card == 4) { oreResourceCount    = oreResourceCount    + amount; Summary.AddToSummary("P1: " + amount + " Ore resource "    + gainOrLose); }
            else if (card == 5) { woolResourceCount   = woolResourceCount   + amount; Summary.AddToSummary("P1: " + amount + " Wool resource "   + gainOrLose); }
        }
        UpdateCounters();
    }

    //This method is called when a 7 is rolled. In the case that the player is holding
    //more than 7 cards, then half of the card will be picked at random and be removed.
    public void LoseHalfCards()
    {
        if (totalResourceCardsInHand > 7)
        {
            int count = totalResourceCardsInHand / 2;
            for (int i = count; i > 0; i--)
            {
                int card = Random.RandomRange(1, 6);
                while (!CheckIfGotOneResource(card)) { card = Random.RandomRange(1, 6); }

                GainOrLoseResource(card, -1);
            }
        }
        UpdateCounters();
    }

    //This method checks whether the player has at least one of a given resource
    public bool CheckIfGotOneResource(int card)
    {
        if      (card == 1) { return (brickResourceCount  > 0); }
        else if (card == 2) { return (grainResourceCount  > 0); }
        else if (card == 3) { return (lumberResourceCount > 0); }
        else if (card == 4) { return (oreResourceCount    > 0); }
        else if (card == 5) { return (woolResourceCount   > 0); }
        else { return false; }
    }

    //-------------------------------------------------------------------------------
    //These methods deal with moving the robber

    //This method is called when the player selects a robber spot to move the robber.
    //It checks to see whether the hex the robber is currently on isn't a desert hex
    //and finds the numbered token that was on that hex. Then it makes sure to set that back
    //and move the robber to the new spot. Then in the event a knight card was used,
    //it will update the score board with the new knight card used score.
    public void MoveRobber(int number)
    {
        GameObject newRobberSpot = GameObject.Find("Token " + number);
        GameObject oldRobberSpot = GameObject.Find("Token 1");
        int counter = 0;

        for (int i = 1; i < 20; i++)
        {
            GameObject token = GameObject.Find("Token " + i);
            if (token.GetComponent<Image>().sprite.name == "Robber" && !(token.GetComponent<Image>().color == new Color(1, 1, 1, 0)))
            {
                oldRobberSpot = token;
                counter = i;
            }
        }

        string oldRobberSpotToken = board.GetOriginalToken(counter);
        if(oldRobberSpotToken == "Robber") { oldRobberSpot.GetComponent<Image>().color = new Color(1, 1, 1, 0); }
        else                               { board.SetTokenToOriginal(counter, oldRobberSpotToken);             }


        if (usingKnightCard)
        {
            scoreBoard.KnightCardUsed(1);
            knightDevCount--;
            UpdateCounters();
            Summary.AddToSummary("P1: Used knight card");
        }
        Summary.AddToSummary("P1: Moved robber");

        board.SetTokenToRobber(newRobberSpot);
        newRobberSpot.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        int resource = board.StealResource(GameObject.Find("Hex " + number), playerColour);
        GainOrLoseResource(resource,1);
        board.DeactivateRobberPlacementButtons();
        Game.SetEndTurnButtonBackOn();
        isMovingRobber = false;
    }

    //This method generates the robber placement spots.
    public void GenerateRobberPlacements(bool knightCardUsed)
    {
        usingKnightCard = knightCardUsed;
        for (int i = 1; i < 20; i++)
        {
            GameObject robberSpot = GameObject.Find("Move Robber Button " + i);
            GameObject token = GameObject.Find("Token " + i);
            if (token.GetComponent<Image>().sprite.name != "Robber" 
                || token.GetComponent<Image>().color == new Color(1, 1, 1, 0)) { robberSpot.GetComponent<Button>().interactable = true; }
        }
    }

    //---------------------------------------------------------------------------
    //Getters and setters for variables
    public int GetRemainingRoadBuildCount()       { return remainingRoadsToBuild;       }
    public int GetRemainingSettlementBuildCount() { return remainingSettlementsToBuild; }
    public int GetRemainingCitiesBuildCount()     { return remainingCitiesToBuild;      }

    public int GetBrickResourceCount()  { return brickResourceCount;  }
    public int GetGrainResourceCount()  { return grainResourceCount;  }
    public int GetLumberResourceCount() { return lumberResourceCount; }
    public int GetOreResourceCount()    { return oreResourceCount;    }
    public int GetWoolResourceCount()   { return woolResourceCount;   }
    
    public int GetAmountOfResource(int card)
    {
        if      (card == 1) { return brickResourceCount;  }
        else if (card == 2) { return grainResourceCount;  }
        else if (card == 3) { return lumberResourceCount; }
        else if (card == 4) { return oreResourceCount;    }
        else if (card == 5) { return woolResourceCount;   }
        else                { return -1;                  }
    }

    public void SetMovingRobber(bool set) { isMovingRobber = set;  }
    public bool GetIsMovingRobber()       { return isMovingRobber; }

    //------------------------------------------------------------------------------------------------
    //methods that changes the text on the game screen
    private void UpdateCounters()
    {
        GameObject.Find("Wool card count").GetComponent<Text>().text   = woolResourceCount.ToString();
        GameObject.Find("Grain card count").GetComponent<Text>().text  = grainResourceCount.ToString();
        GameObject.Find("Lumber card count").GetComponent<Text>().text = lumberResourceCount.ToString();
        GameObject.Find("Brick card count").GetComponent<Text>().text  = brickResourceCount.ToString();
        GameObject.Find("Ore card count").GetComponent<Text>().text    = oreResourceCount.ToString();
        totalResourceCardsInHand = brickResourceCount + grainResourceCount + lumberResourceCount + oreResourceCount + woolResourceCount;

        GameObject.Find("Settlement Count Text").GetComponent<Text>().text = remainingSettlementsToBuild.ToString();
        GameObject.Find("City Count Text").GetComponent<Text>().text       = remainingCitiesToBuild.ToString();
        GameObject.Find("Road Count Text").GetComponent<Text>().text       = remainingRoadsToBuild.ToString();

        GameObject.Find("Road Building card count").GetComponent<Text>().text  = roadBuildingDevCount.ToString();
        GameObject.Find("Monopoly card count").GetComponent<Text>().text       = monopolyDevCount.ToString();
        GameObject.Find("Year of Plenty card count").GetComponent<Text>().text = yearOfPlentyDevCount.ToString();
        GameObject.Find("Knight card count").GetComponent<Text>().text         = knightDevCount.ToString();
        GameObject.Find("Victory Point card count").GetComponent<Text>().text  = victoryPointDevCount.ToString();
    }

    //------------------------------------------------------------------------------------------------
    //only called by the board class.
    public int GetTotalDevCardCount()  { return totalDevCardsInHand;      }
    public int GetTotalResourceCount() { return totalResourceCardsInHand; }

    //-----------------------------------------------------------------------
    //checking what the player can do with the resources they have on them
    public bool CanPlayerTrade()           { return totalResourceCardsInHand > 0;                                                                                                                                                                 }
    public bool CanPlayerBuildRoad()       { return brickResourceCount > 0 && lumberResourceCount > 0 && remainingRoadsToBuild  > 0 && CheckIfPlayerLegalRoadPlacementExist();                                                                    }
    public bool CanPlayerBuildSettlement() { return brickResourceCount > 0 && lumberResourceCount > 0 && grainResourceCount     > 0 && woolResourceCount > 0 && remainingSettlementsToBuild > 0 &&  CheckIfPlayerLegalSettlementPlacementExist(); }
    public bool CanPlayerBuildCity()       { return oreResourceCount   > 2 && grainResourceCount  > 1 && remainingCitiesToBuild > 0 && CheckIfPlayerLegalCityPlacementExist();                                                                    }
    public bool CanPlayerBuildDevCard()    { return oreResourceCount   > 0 && woolResourceCount   > 0 && grainResourceCount     > 0 && Game.GetTotalDevCardsRemaining() > 0;                                                                      }

    public bool DoesPlayerHaveRoadBuildingCard() { return roadBuildingDevCount > 0 && remainingRoadsToBuild > 0; }
    public bool DoesPlayerHaveMonopolyCard()     { return monopolyDevCount     > 0; }
    public bool DoesPlayerHaveYearOfPlentyCard() { return yearOfPlentyDevCount > 0; }
    public bool DoesPlayerHaveKnightCard()       { return knightDevCount       > 0; }
    public bool DoesPlayerHaveVictoryPointCard() { return victoryPointDevCount > 0; }

    public bool CheckIfPlayerLegalSettlementPlacementExist()
    {
        for (int i = 1; i < 55; i++)
        {
            GameObject settlementSpot = GameObject.Find("Settlement/City button " + i);
            Button theButton = settlementSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            if (theButton.colors.disabledColor == black && board.ObeysDistanceRule(settlementSpot)
                && board.CheckForConnectingRoads(settlementSpot, 1, Game.GetInitial1(), Game.GetInitial2(), playerColour)) { return true; }
        }
        return false;
    }

    public bool CheckIfPlayerLegalRoadPlacementExist()
    {
        for (int i = 1; i < 73; i++)
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            if (theButton.colors.disabledColor == black)
            {
                if (board.CheckForConnectingSettlements(roadSpot, playerColour) ||
                    board.CheckForConnectingRoads(roadSpot, 0, Game.GetInitial1(), Game.GetInitial2(), playerColour)) { return true; }
            }
        }
        return true;
    }

    public bool CheckIfPlayerLegalCityPlacementExist()
    {
        for (int i = 1; i < 55; i++)
        {
            GameObject citySpot = GameObject.Find("Settlement/City button " + i);
            Button theButton = citySpot.GetComponent<Button>();
            if (theButton.colors.disabledColor == playerColour && citySpot.GetComponent<Image>().sprite == boardPieces[0]) { return true; }
        }
        return false;
    }

    //--------------------------------------------------------------------------------------
    //Method for when the player recieves a trade offer. This method sets up the trade offer menu panel with
    //all the relevant information. It will also check whether the player can even do the trade. If they can't then
    //the accept button is disabled
    public void GetTradeOffer(int[] resourcesGiven, int[] resourcesWanted, int oppNumber)
    {
        GameObject.Find("Trade Offer Panel").GetComponent<RectTransform>().localPosition = new Vector3(-173, 0, 0);
        if (oppNumber == 2)
        {
            GameObject.Find("Player Label").GetComponent<Text>().text = "P2";
            GameObject.Find("Player Label").GetComponent<Text>().color = Color.yellow;
        }
        else if (oppNumber == 3)
        {
            GameObject.Find("Player Label").GetComponent<Text>().text = "P3";
            GameObject.Find("Player Label").GetComponent<Text>().color = Color.white;
        }
        else if (oppNumber == 4)
        {
            GameObject.Find("Player Label").GetComponent<Text>().text = "P4";
            GameObject.Find("Player Label").GetComponent<Text>().color = Color.blue;
        }

        GameObject.Find("Brick card WTT count").GetComponent<Text>().text  = resourcesGiven [0].ToString();
        GameObject.Find("Brick card For count").GetComponent<Text>().text  = resourcesWanted[0].ToString();
        GameObject.Find("Grain card WTT count").GetComponent<Text>().text  = resourcesGiven [1].ToString();
        GameObject.Find("Grain card For count").GetComponent<Text>().text  = resourcesWanted[1].ToString();
        GameObject.Find("Lumber card WTT count").GetComponent<Text>().text = resourcesGiven [2].ToString();
        GameObject.Find("Lumber card For count").GetComponent<Text>().text = resourcesWanted[2].ToString();
        GameObject.Find("Ore card WTT count").GetComponent<Text>().text    = resourcesGiven [3].ToString();
        GameObject.Find("Ore card For count").GetComponent<Text>().text    = resourcesWanted[3].ToString();
        GameObject.Find("Wool card WTT count").GetComponent<Text>().text   = resourcesGiven [4].ToString();
        GameObject.Find("Wool card For count").GetComponent<Text>().text   = resourcesWanted[4].ToString();

        if (brickResourceCount >= resourcesWanted[0] && grainResourceCount >= resourcesWanted[1]
            && lumberResourceCount >= resourcesWanted[2] && oreResourceCount >=resourcesWanted[3]
            && woolResourceCount >= resourcesWanted[4])
        {
            GameObject.Find("Accept Button").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("Accept Button").GetComponent<Button>().interactable = false;
        }
        GameObject.Find("Decline Button").GetComponent<Button>().interactable = true;

    }

    //---------------------------------------------------------------------------------
    //These methods handles all the functionality with the development cards.
    public void UseMonopolyCard(int card)
    {
        int totalResourcesGained = board.GetAllOfSpecificResourceOthersHold(1, card);
        GainOrLoseResource(card, totalResourcesGained);
        board.MonopolyUsed(1, card);
        monopolyDevCount--;
        Summary.AddToSummary("P1: Used monopoly card");
        UpdateCounters();
    }

    public void LoseAllResourceMonopoly(int card)
    {
        if      (card == 1) { brickResourceCount  = 0; Summary.AddToSummary("P1: Lost all brick resource cards") ; }
        else if (card == 2) { grainResourceCount  = 0; Summary.AddToSummary("P1: Lost all grain resource cards") ; }
        else if (card == 3) { lumberResourceCount = 0; Summary.AddToSummary("P1: Lost all lumber resource cards"); }
        else if (card == 4) { oreResourceCount    = 0; Summary.AddToSummary("P1: Lost all ore resource cards")   ; }
        else if (card == 5) { woolResourceCount   = 0; Summary.AddToSummary("P1: Lost all wool resource cards")  ; }
        UpdateCounters();
    }

    public void UseVictoryPointCard()
    {
        scoreBoard.VictoryPointCardUsed(1);
        victoryPointDevCount--;
        Summary.AddToSummary("P1: Used victory point card");
        UpdateCounters();
    }

    public void SetUsingMonopolyCard()        { usingMonopolyCard = true;      }
    public void SetNotUsingMonopolyCard()     { usingMonopolyCard = false;     }
    public void SetUsingYearOfPlentyCard()    { usingYearOfPlentyCard = true;  }

    public void SetNotUsingYearOfPlentyCard()
    {
        usingYearOfPlentyCard = false;
        usingDevCard = false;
        yearOfPlentyCardsSelected = 0;
    }

    public void IncrementAmountOfYearOfPlentyCardsSelected()
    {
        yearOfPlentyCardsSelected++;
        yearOfPlentyDevCount--;
        usingDevCard = true;
        UpdateCounters();
    }

    public bool IsUsingDevCard()                      { return usingDevCard;              }
    public bool IsUsingMonopolyCard()                 { return usingMonopolyCard;         }
    public bool IsUsingYearOfPlentyCard()             { return usingYearOfPlentyCard;     }
    public int GetAmountOfYearOfPlentyCardsSelected() { return yearOfPlentyCardsSelected; }
    //---------------------------------------------------------------------------------
    //only occurs once, at the beginning of the game
    private void GeneratePlayerSecondRoadPlacements(GameObject settlementSpot)
    {
        for (int i = 1; i < 73; i++)
        {
            GameObject roadSpot = GameObject.Find("Road Button " + i);
            Button theButton = roadSpot.GetComponent<Button>();
            Color black = new Color(0, 0, 0, 0);
            if (theButton.colors.disabledColor == black 
                && board.SpotComparison(settlementSpot,roadSpot,1)) { roadSpot.GetComponent<Button>().interactable = true; }
        }
    }

    private void GetInitialResources(GameObject settlementSpot)
    {
        for (int i = 1; i < 20; i++)
        {
            GameObject hex = GameObject.Find("Hex " + i);
            if (board.SpotComparison(settlementSpot,hex,3)) { UpdateResourceCountsWithGivenHex(hex, 1); }
        }
        UpdateCounters();
    }
}
