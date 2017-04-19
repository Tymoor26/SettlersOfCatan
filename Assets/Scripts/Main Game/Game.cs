﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

//This class handles all the main game functionality.
//This ranges from rolling the dice, giving resources from a dice roll,
//setting down the pieces, keeping track of the turn order, keeping track of the amount of development card
//using development cards, trades, checking if the game has been won and ending the game if it has.
//The way these classes has been structured, this Game class is the centre of everything.
//This class has objects that are made up of the other classes as its type. For example when the player clicks on 
//a settlement spot, then it will call a method in the Player class via the player object that will deal with this
//action.
public class Game : MonoBehaviour {
    public Board board;
    public Player player;
    public Computer compPlayer1;
    public Computer compPlayer2;
    public Computer compPlayer3;
    public GameObject[] giveDMSelectionButtons;
    public GameObject[] getDMSelectionButtons;
    public Slider[] givePlayerSelectionSliders;
    public InputField[] wantPlayerSelectInputField;
    public LevelManager levelManager;
    public static bool computerDoingPlayerTrade;
    private int giveDMCardSelectedNumber;
    private int getDMCardSelectedNumber;
    private GameObject giveDMCardSelected;
    private GameObject getDMCardSelected;
    private bool doingTurn;
    private Button rollDiceButton;
    private Button endTurnButton;
    private Text currentTurnText;
    private static int currentTurn;
    private static bool initialRound1;
    private static bool initialRound2;
    private static bool generated;
    private static int totalDevCardsRemaining;
    private static int remainingRoadBuildingCards, remainingMonopolyCards, remainingYearOfPlentyCards, remainingKnightCards, remainingVictoryPointCards;

    // Use this for initialization
    // Setting up the initial game state, the variables and the computer objects.
    private void Start () {
        board.SetUpBoard();
        currentTurn = 1;
        initialRound1 = true;
        initialRound2 = false;
        computerDoingPlayerTrade = false;
        doingTurn = false;
        compPlayer1.SetSettings(PlayerPrefsManager.GetOpponentSetting(1), Color.yellow, 2);
        compPlayer2.SetSettings(PlayerPrefsManager.GetOpponentSetting(2), Color.white,  3);
        compPlayer3.SetSettings(PlayerPrefsManager.GetOpponentSetting(3), Color.blue,   4);
        board.DeactivateRoadPlacementButtons();
        board.DeactivateRobberPlacementButtons();
        generated = false;
        DeactivatePlayerButtons();
        DeactivateResourceCardButtons();
        currentTurnText = GameObject.Find("Current Turn Text").GetComponent<Text>();
        rollDiceButton  = GameObject.Find("Roll Dice Button").GetComponent<Button>();
        endTurnButton   = GameObject.Find("End Turn Button").GetComponent<Button>();
        rollDiceButton.interactable = false;
        endTurnButton.interactable  = false;
        totalDevCardsRemaining = 25;
        remainingRoadBuildingCards = 2;
        remainingMonopolyCards = 2;
        remainingYearOfPlentyCards = 2;
        remainingKnightCards = 14;
        remainingVictoryPointCards = 5;
    }

    //---------------------------------------------------------------------------------------
    // Methods dealing with the turn flow

    // Update is called once per frame
    // This deals with the current turn order, the rolling of the dice for the computer,
    // doing the computers turn by calling the computer turn method from the computer class repeatedly 
    // until the computer decides to end the turn and checking if the game has been won by a player.
    // It also makes sure to change the label at the top to show who's turn it currently is.
    // The first two if statements are for the initial turns for when the players have to place their first 
    // two settlements and road.
    private void Update () {
        //Do the initial turns
        if      (initialRound1) { InitialTurnRound1(); }
        else if (initialRound2) { InitialTurnRound2(); }
        else if (currentTurn == 1) //human players turn
        {
            currentTurnText.text = "Player 1"; //changing the label at the top to say that its player 1's turn and display their colour
            currentTurnText.color = Color.red;
            if (!generated) //making sure that only the dice roll button is pressable at the beginning of the turn.
            {
                DeactivatePlayerButtons();
                rollDiceButton.interactable = true;
                endTurnButton.interactable = false;
            }
            else  if (!player.GetIsMovingRobber() && !player.IsUsingDevCard()) { ActivatePlayerButtons(); } //once the dice rolls have happened then activate the other buttons
            if (CheckIfWon(1)) { EndGame(1); } //check if this player has won the game
        }
        //the following are for when its the computers turn. They each follow the same structure, but operate on different objects.
        else if (currentTurn == 2)
        {
            currentTurnText.text = "Player 2"; //changing the label at the top to say that its player 2's turn and display their colour
            currentTurnText.color = Color.yellow;
            if (!doingTurn) //since this is going to be looping every frame, multiple dice rolls aren't needed. So thats why this if statement exists. Dice rolls once
            {
                RollDice();
                doingTurn = true;
            }
            bool exhaustedPossibleMoves = compPlayer1.CompTurn(); //computer does an action within this method. If they decide to end the turn this becomes true.
            //to make sure computer doesn't end their turn during a trade, make sure to check if they are currently doing a player trade during the turn
            if (!computerDoingPlayerTrade && exhaustedPossibleMoves)
            {
                DoneWithTurn();
                doingTurn = false;
            }
            if (CheckIfWon(2)) { EndGame(2); }
        }
        else if (currentTurn == 3)
        {
            currentTurnText.text = "Player 3"; //changing the label at the top to say that its player 3's turn and display their colour
            currentTurnText.color = Color.white;
            if (!doingTurn)
            {
                RollDice();
                doingTurn = true;
            }
            bool exhaustedPossibleMoves = compPlayer2.CompTurn();
            if (!computerDoingPlayerTrade && exhaustedPossibleMoves)
            {
                DoneWithTurn();
                doingTurn = false;
            }
            if (CheckIfWon(3)) { EndGame(3); }
        }
        else if (currentTurn == 4)
        {
            currentTurnText.text = "Player 4"; //changing the label at the top to say that its player 4's turn and display their colour
            currentTurnText.color = Color.blue;
            if (!doingTurn)
            {
                RollDice();
                doingTurn = true;
            }
            bool exhaustedPossibleMoves = compPlayer3.CompTurn();
            if (!computerDoingPlayerTrade && exhaustedPossibleMoves)
            {
                DoneWithTurn();
                doingTurn = false;
            }
            if (CheckIfWon(4)) { EndGame(4); }
        }
        else if (currentTurn > 4) { currentTurn = 1; } //once currentTurn has incremented to be above 4 then reset it to 1 as it goes back to the human players turn
    }

    // This increments the turn counter and deactivates all the buttons so that the player can't do actions during the computers turn.
    public void DoneWithTurn()
    {
        Summary.AddToSummary("P" + currentTurn + ": End turn");
        currentTurn++;
        generated = false;
        DeactivatePlayerButtons();
        board.DeactivateSettlementPlacementsButtons();
        board.DeactivateRoadPlacementButtons();
        endTurnButton.interactable = false;
    }

    // This checks if a particular player has won the game by taking the value of their total points scored on the summary table
    private bool CheckIfWon(int playerNumber)
    {
        if      (playerNumber == 1) { return int.Parse(GameObject.Find("Total P1").GetComponent<Text>().text) >= 10; }
        else if (playerNumber == 2) { return int.Parse(GameObject.Find("Total P2").GetComponent<Text>().text) >= 10; }
        else if (playerNumber == 3) { return int.Parse(GameObject.Find("Total P3").GetComponent<Text>().text) >= 10; }
        else if (playerNumber == 4) { return int.Parse(GameObject.Find("Total P4").GetComponent<Text>().text) >= 10; }
        return false;
    }

    // This ends the game by deactivating all the buttons and getting all the stats of the game for each player as well as the winner and saving this
    // to the player prefs manager. Then it loads the next screen which is the results screen.
    private void EndGame(int winner)
    {
        DeactivateEverythingOtherThanDevCardButtons();
        DeactivatePlayerButtons();
        DeactivateResourceCardButtons();
        PlayerPrefsManager.SetResults(ScoreBoard.GetPlayerStats(1), ScoreBoard.GetPlayerStats(2), ScoreBoard.GetPlayerStats(3), ScoreBoard.GetPlayerStats(4), winner);
        levelManager.LoadNextLevel();
    }

    //----------------------------------------------------------------------------------
    //These methods deal with buttons on the left hand panel of the screen (the one with the board)

    //To simulate rolling two dice, I have two int variables that both get a random int value from 1 to 6.
    //Then I add these up and output these onto the screen. Then if the total is not 7 then it
    //generates the necessary resources based on the total roll. If a 7 is rolled then the rolled 7 
    //method is called.
    public void RollDice()
    {
        generated = true; //generated becoming true causes the update method to stop letting the dice roll button be interactable
        int diceRoll1 = Random.RandomRange(1, 7);
        int diceRoll2 = Random.RandomRange(1, 7);
        int totalRoll = diceRoll1 + diceRoll2;
        GameObject.Find("Dice Roll Text").GetComponent<Text>().text = totalRoll.ToString();
        Summary.AddToSummary("P" + currentTurn + ": Rolled a " + totalRoll);

        if (currentTurn == 1) { rollDiceButton.interactable = false; } //if its the players turn then make the roll dice button uninteractable

        //if total roll isn't 7 then it will generate the resources and deactivates the dice roll button and activates the end turn button if its the players turn.
        if (totalRoll != 7)
        {
            GenerateResources(totalRoll);
            if (currentTurn == 1)
            {
                rollDiceButton.interactable = false;
                endTurnButton.interactable = true;
            }
        }
        else //however if the total roll was 7 then it will deactivate all of the buttons including the end turn button and does the rolled 7 method
        {
            DeactivatePlayerButtons();
            endTurnButton.interactable = false;
            Rolled7();
        }
    }

    //This does the get resources method in the player and computer classes
    private void GenerateResources(int roll)
    {
        player.GetResources(roll);
        compPlayer1.GetResources(roll);
        compPlayer2.GetResources(roll);
        compPlayer3.GetResources(roll);
    }

    //First thing that happens when a 7 is rolled is that every player loses half their cards if they currently hold more than 7 cards
    //Then whoever rolled the 7 gets to move the robber.
    private void Rolled7()
    {
        player.LoseHalfCards();
        compPlayer1.LoseHalfCards();
        compPlayer2.LoseHalfCards();
        compPlayer3.LoseHalfCards();
        if (currentTurn == 1)
        {
            player.SetMovingRobber(true);
            player.GenerateRobberPlacements(false);
        }
        else if (currentTurn == 2) { compPlayer1.MoveRobber(); }
        else if (currentTurn == 3) { compPlayer2.MoveRobber(); }
        else if (currentTurn == 4) { compPlayer3.MoveRobber(); }
    }

    //This is a static variable that is called from the player class when they have finished moving the robber
    public static void SetEndTurnButtonBackOn() { GameObject.Find("End Turn Button").GetComponent<Button>().interactable = true; }

    //--------------------------------------------------------------------------
    //These methods activate and deactivates the buttons the player can press

    //This deactivates all of the main action buttons that the player can do in their turn
    public static void DeactivatePlayerButtons()
    {
        GameObject.Find("Trade Button").GetComponent<Button>().interactable                  = false;
        GameObject.Find("Build Road Button").GetComponent<Button>().interactable             = false;
        GameObject.Find("Build Settlement Button").GetComponent<Button>().interactable       = false;
        GameObject.Find("Build City Button").GetComponent<Button>().interactable             = false;
        GameObject.Find("Build Development Card Button").GetComponent<Button>().interactable = false;

        GameObject.Find("Road Building Card Button").GetComponent<Button>().interactable     = false;
        GameObject.Find("Monopoly Card Button").GetComponent<Button>().interactable          = false;
        GameObject.Find("Year of Plenty Card Button").GetComponent<Button>().interactable    = false;
        GameObject.Find("Knight Card Button").GetComponent<Button>().interactable            = false;
        GameObject.Find("Victory Point Card Button").GetComponent<Button>().interactable     = false;
    }

    //This activates the main action buttons that the player can do in their turn provided that they are allowed to do so with what they have
    private void ActivatePlayerButtons()
    {
        GameObject.Find("Trade Button").GetComponent<Button>().interactable                  = player.CanPlayerTrade();
        GameObject.Find("Build Road Button").GetComponent<Button>().interactable             = player.CanPlayerBuildRoad();
        GameObject.Find("Build Settlement Button").GetComponent<Button>().interactable       = player.CanPlayerBuildSettlement();
        GameObject.Find("Build City Button").GetComponent<Button>().interactable             = player.CanPlayerBuildCity(); 
        GameObject.Find("Build Development Card Button").GetComponent<Button>().interactable = player.CanPlayerBuildDevCard();

        GameObject.Find("Road Building Card Button").GetComponent<Button>().interactable     = player.DoesPlayerHaveRoadBuildingCard();
        GameObject.Find("Monopoly Card Button").GetComponent<Button>().interactable          = player.DoesPlayerHaveMonopolyCard();
        GameObject.Find("Year of Plenty Card Button").GetComponent<Button>().interactable    = player.DoesPlayerHaveYearOfPlentyCard();
        GameObject.Find("Knight Card Button").GetComponent<Button>().interactable            = player.DoesPlayerHaveKnightCard();
        GameObject.Find("Victory Point Card Button").GetComponent<Button>().interactable     = player.DoesPlayerHaveVictoryPointCard();
    }

    //These activates the resource card buttons when the player uses either the monopoly or year of plenty dev cards
    private void ActivateResourceCardButtons()
    {
        GameObject.Find("Brick Card Image").GetComponent<Button>().interactable  = true;
        GameObject.Find("Grain Card Image").GetComponent<Button>().interactable  = true;
        GameObject.Find("Lumber Card Image").GetComponent<Button>().interactable = true;
        GameObject.Find("Ore Card Image").GetComponent<Button>().interactable    = true;
        GameObject.Find("Wool Card Image").GetComponent<Button>().interactable   = true;
    }

    //These deactivates the resource card buttons when the player is finished using either the monopoly or year of plenty dev cards 
    private void DeactivateResourceCardButtons()
    {
        GameObject.Find("Brick Card Image").GetComponent<Button>().interactable  = false;
        GameObject.Find("Grain Card Image").GetComponent<Button>().interactable  = false;   
        GameObject.Find("Lumber Card Image").GetComponent<Button>().interactable = false;
        GameObject.Find("Ore Card Image").GetComponent<Button>().interactable    = false;
        GameObject.Find("Wool Card Image").GetComponent<Button>().interactable   = false;
    }

    //These deactivates the dev card buttons when the player is currently using either the road building dev card after placing the first road
    //or the year of plenty dev card after selecting the first resource card
    private void DeactivateDevCardButtons()
    {
        GameObject.Find("Road Building Card Button").GetComponent<Button>().interactable  = false;
        GameObject.Find("Monopoly Card Button").GetComponent<Button>().interactable       = false;
        GameObject.Find("Year of Plenty Card Button").GetComponent<Button>().interactable = false;
        GameObject.Find("Knight Card Button").GetComponent<Button>().interactable         = false;
        GameObject.Find("Victory Point Card Button").GetComponent<Button>().interactable  = false;
    }

    //This is a general deactivationg method that deactivates everything other than the dev card buttons.
    private void DeactivateEverythingOtherThanDevCardButtons()
    {
        board.DeactivateSettlementPlacementsButtons();
        board.DeactivateRoadPlacementButtons();
        board.DeactivateRobberPlacementButtons();
        player.SetNotUsingMonopolyCard();
        DeactivateResourceCardButtons();
    }

    //----------------------------------------------------------------------------------
    //These methods deal with the four main build buttons

    //When each of these methods are called, all the buttons apart from the dev card buttons are deactivated
    //and then it will call the relevant methods from within the player class.
    public void BuildRoadButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.GeneratePlayerLegalRoadPlacements(false); //false because its not being done using a road building card
    }

    public void BuildSettlementButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.GeneratePlayerLegalSettlementPlacements();
    }

    public void BuildCityButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.GenerateLegalCityPlacements();
    }

    public void BuildDevCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.PlayerBuildDevCard();
    }

    //-------------------------------------------------------------------------------
    //These methods deal with pressing the development card buttons

    //When each of these methods are called, all the buttons apart from the dev card buttons are deactivated
    //and then it will call the relevant methods from within the player class.
    public void UseRoadBuildingCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.GeneratePlayerLegalRoadPlacements(true);
    }

    public void UseMonopolyCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        ActivateResourceCardButtons();
        player.SetUsingMonopolyCard();
    }

    public void UseYearOfPlentyCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        ActivateResourceCardButtons();
        player.SetUsingYearOfPlentyCard();
    }

    public void UseKnightCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.GenerateRobberPlacements(true);
    }

    public void UseVictoryPointCardButtonPressed()
    {
        DeactivateEverythingOtherThanDevCardButtons();
        player.UseVictoryPointCard();
    }

    //When a resource card is selected during the use of the monopoly card and the year of plenty card
    //then this will do the relevant action depending on which of the two cards are being used
    public void CardSelected(int card)
    {
        if (player.IsUsingMonopolyCard())
        {
            player.UseMonopolyCard(card);
            player.SetNotUsingMonopolyCard();
        }
        else if (player.IsUsingYearOfPlentyCard())
        {
            if (player.GetAmountOfYearOfPlentyCardsSelected() < 1)
            {
                Summary.AddToSummary("P1: Used year of plenty card");
                player.IncrementAmountOfYearOfPlentyCardsSelected();
                endTurnButton.interactable = false;
                DeactivateDevCardButtons();
                DeactivatePlayerButtons();
            }
            else
            {
                player.SetNotUsingYearOfPlentyCard();
                endTurnButton.interactable = true;
            }
            player.GainOrLoseResource(card, 1);
        }

        //This makes sure that when they are both not being used anymore, that everything except for the dev card buttons. Update will activate the other buttons again
        if (!player.IsUsingMonopolyCard() && !player.IsUsingYearOfPlentyCard()) { DeactivateEverythingOtherThanDevCardButtons(); }
    }

    //-------------------------------------------------------------------------
    //Method to deal with the trade button and buttons within the trade menu

    //When the make trade button is pressed, this button sets up everything
    public void MakeTradeButtonPressed()
    {
        //this will deactivate all board placement buttons and other dev card buttons.
        DeactivateEverythingOtherThanDevCardButtons();
        //this will set all the buttons and the values on the screen underneath the buttons for the domestic and maritime trade section
        board.GenerateLegalDMGiveButtons(1, giveDMSelectionButtons, getDMSelectionButtons);

        //these will set all the sliders and buttons for the player trade section
        DeactivateTradeScreenButtonsAndSliders(true);
        //GameObject.Find("P2 Trade Accept Button").GetComponent<Button>().interactable = false;
        //GameObject.Find("P3 Trade Accept Button").GetComponent<Button>().interactable = false;
        //GameObject.Find("P4 Trade Accept Button").GetComponent<Button>().interactable = false;
        SetPlayerTradeSliders();
        if (GameObject.Find("Trade Result Text").GetComponent<Text>().text == "Trade your resources")
        {
            GameObject.Find("P2 Trade Accept Button").GetComponentInChildren<Text>().text = "P2";
            GameObject.Find("P3 Trade Accept Button").GetComponentInChildren<Text>().text = "P3";
            GameObject.Find("P4 Trade Accept Button").GetComponentInChildren<Text>().text = "P4";
        }
    }

    //These methods are for the domestic and maritime trading section of the trade window
    //When the exit trade button is pressed then this method will put the trade window away
    public void ExitTradeWindowButtonPressed()
    {
        GameObject.Find("TradePanel").GetComponent<RectTransform>().localPosition = new Vector3(0, -1500, 0);
        GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Trade your resources";
    }

    //This is called when a card from the give section of the domestic and maritime trade is selected
    //It will make sure that the same card from the get section becomes uninteractable as the player shouldn't be allowed
    //to trade the resource they are giving for that same resource. Since card is from 1 to 5 and the index values of the array
    //are from 0 to 4, card - 1 is used to match it.
    public void GiveDMCardSelected(int card)
    {
        for(int i = 0; i < 5; i++)
        {
            giveDMSelectionButtons[i].GetComponent<Image>().color = Color.white;
            getDMSelectionButtons[i].GetComponent<Image>().color = Color.white;
            getDMSelectionButtons[i].GetComponent<Button>().interactable = (card - 1 != i); //making sure thar the same card can't be selected in the get section
        }
        giveDMSelectionButtons[card - 1].GetComponent<Image>().color = Color.red; //turn selected card to red to indicate it has been selected
        giveDMCardSelected = giveDMSelectionButtons[card - 1];                    //keep track of which card has been selected
        giveDMCardSelectedNumber = card;
        GameObject.Find("Make DM Trade button").GetComponent<Button>().interactable = false; //In the event that the user changes the give card selected, make sure to deactivate make trade button
    }

    //This is called when a card from the get section of the domestic and maritime trade is selected
    //When called, the make trade button in this section will become interactable. In the event that
    //the trade that the player is about to do is a maritime trade, the button will say it is a maritime trade.
    //If not then it will say that its a domestic trade.
    public void GetDMCardSelected(int card)
    {
        for (int i = 0; i < 5; i++)
        {
            getDMSelectionButtons[i].GetComponent<Image>().color = Color.white;
        }
        getDMSelectionButtons[card - 1].GetComponent<Image>().color = Color.red;
        getDMCardSelected = getDMSelectionButtons[card - 1];
        getDMCardSelectedNumber = card;
        GameObject.Find("Make DM Trade button").GetComponent<Button>().interactable = true;

        //min for a domestic trade is 4. If this is less than 4 then its a maritime trade.
        int min = board.getMinimumNeededToGiveForResource(giveDMCardSelectedNumber, 1);
        if(min < 4) { GameObject.Find("Make DM Trade button").GetComponentInChildren<Text>().text = "Make Maritime Trade"; }
        else        { GameObject.Find("Make DM Trade button").GetComponentInChildren<Text>().text = "Make Domestic Trade"; }
    }

    //Called when the make trade button on the domestic and maritime trade section is pressed.
    //This will make the trade happen by taking away the resources that the player is giving from the players hand
    //and adding the resource that the player wanted into their hand. Then it will display a message saying it was successful and then
    //refresh the trade window
    public void MakeDMTradeButtonPressed()
    {
        int amountGiven = board.getMinimumNeededToGiveForResource(giveDMCardSelectedNumber, 1);
        if (amountGiven == 4) { Summary.AddToSummary("P1: made a Domestic Trade"); }
        else                  { Summary.AddToSummary("P1: made a Maritime Trade"); }
        player.GainOrLoseResource(giveDMCardSelectedNumber, amountGiven * -1);
        player.GainOrLoseResource(getDMCardSelectedNumber, 1);
        if(amountGiven < 4) { GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Successfully made Maritime trade"; }
        else                { GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Successfully made Domestic trade"; }

        MakeTradeButtonPressed();
    }

    //These methods are for the player trading section of the trade window
    //This method sets up the sliders for what the player is willing to give in a trade. The sliders will be set such that the max
    //value is however much of that particular resource the player currently holds. If they do not hold any of that resource then
    //the slider becomes uninteractable.
    private void SetPlayerTradeSliders()
    {
        for (int i = 0; i<5; i++) { //reset sliders
            givePlayerSelectionSliders[i].interactable = false;
            //wantPlayerSelectInputField[i].interactable = false;
        }
        if (player.GetBrickResourceCount() > 0)
        {
            givePlayerSelectionSliders[0].interactable = true;
            givePlayerSelectionSliders[0].maxValue = player.GetBrickResourceCount();
        }
        if (player.GetGrainResourceCount() > 0)
        {
            givePlayerSelectionSliders[1].interactable = true;
            givePlayerSelectionSliders[1].maxValue = player.GetGrainResourceCount();
        }
        if (player.GetLumberResourceCount() > 0)
        {
            givePlayerSelectionSliders[2].interactable = true;
            givePlayerSelectionSliders[2].maxValue = player.GetLumberResourceCount();
        }
        if (player.GetOreResourceCount() > 0)
        {
            givePlayerSelectionSliders[3].interactable = true;
            givePlayerSelectionSliders[3].maxValue = player.GetOreResourceCount();
        }
        if (player.GetWoolResourceCount() > 0)
        {
            givePlayerSelectionSliders[4].interactable = true;
            givePlayerSelectionSliders[4].maxValue = player.GetWoolResourceCount();
        }
    }

    //When the slider value changes, this method changes the counter next to its respective slider to show what number the slider is currently on.
    //It also calls the UpdateGiveInputFields method
    public void WantSliderChangedValue(int card)
    {
        if (card == 1) { GameObject.Find("Brick card slider count").GetComponent<Text>().text  = givePlayerSelectionSliders[0].value.ToString(); wantPlayerSelectInputField[0].text = "0"; }
        if (card == 2) { GameObject.Find("Grain card slider count").GetComponent<Text>().text  = givePlayerSelectionSliders[1].value.ToString(); wantPlayerSelectInputField[1].text = "0"; }
        if (card == 3) { GameObject.Find("Lumber card slider count").GetComponent<Text>().text = givePlayerSelectionSliders[2].value.ToString(); wantPlayerSelectInputField[2].text = "0"; }
        if (card == 4) { GameObject.Find("Ore card slider count").GetComponent<Text>().text    = givePlayerSelectionSliders[3].value.ToString(); wantPlayerSelectInputField[3].text = "0"; }
        if (card == 5) { GameObject.Find("Wool card slider count").GetComponent<Text>().text   = givePlayerSelectionSliders[4].value.ToString(); wantPlayerSelectInputField[4].text = "0"; }
        UpdateGiveInputFields(card);
    }

    //When the slider value changes, this method makes sure that in the resources wanted section, you can't input a value
    //into a resource that you are willing to trade.
    private void UpdateGiveInputFields(int card)
    {
        int countWant = 0;
        float countSliders = 0;
        for (int i = 0; i < 5; i++)
        {
            wantPlayerSelectInputField[i].interactable = false;
            countWant = countWant + int.Parse(wantPlayerSelectInputField[i].text);
            countSliders = countSliders + givePlayerSelectionSliders[i].value;
            if (givePlayerSelectionSliders[i].value == 0) { wantPlayerSelectInputField[i].interactable = true;  }
            else                                          { wantPlayerSelectInputField[i].interactable = false; }
        }

        if(countWant == 0 || countSliders == 0) { GameObject.Find("Make Player Trade button").GetComponent<Button>().interactable = false; }
        else                                    { GameObject.Find("Make Player Trade button").GetComponent<Button>().interactable = true;  }
    }

    //When an input field in the want trade part of the player trade section is edited, this method is called.
    //This will activate the make trade button provided that at least one of the input fields has a value thats more than 0
    public void InputFieldEdited()
    {
        int countWant = 0;
        float countSliders = 0;
        for (int i = 0; i < 5; i++)
        {
            countWant = countWant + int.Parse(wantPlayerSelectInputField[i].text);
            countSliders = countSliders + givePlayerSelectionSliders[i].value;
        }

        if (countWant > 0 && countSliders > 0) { GameObject.Find("Make Player Trade button").GetComponent<Button>().interactable = true;  }
        else                                   { GameObject.Find("Make Player Trade button").GetComponent<Button>().interactable = false; }
    }

    //This is called when the make player trade button is pressed. This gathers all the values from the 
    //sliders and the inputfields and puts them in their respective arrays. Then it sends these arrays to the
    //computer players. Each computer player responds to the trade with a accept or a decline.
    //If all 3 decline then the player trade unsuccessful appears. If at least 1 accepts then
    //the button to accept the trade from that player will be active as well as a button to cancel the
    //trade if the player doesn't want to trade with that particular player. In the event that one of the 
    //computer players accepts the trade, it will also deactivate the sliders and the input fields as
    //you don't want to alter these mid trade.
    public void MakePlayerTradeButtonPressed()
    {
        int[] resourcesGiven = new int[5];
        int[] resourcesWanted = new int[5];
        for (int i = 0; i<5; i++)
        {
            resourcesGiven[i]  = (int)givePlayerSelectionSliders[i].value; 
            resourcesWanted[i] = int.Parse(wantPlayerSelectInputField[i].GetComponentInChildren<Text>().text);
        }

        bool comp1accept = compPlayer1.GetTradeOffer(resourcesWanted, resourcesGiven, 1);
        bool comp2accept = compPlayer2.GetTradeOffer(resourcesWanted, resourcesGiven, 1);
        bool comp3accept = compPlayer3.GetTradeOffer(resourcesWanted, resourcesGiven, 1);
        if(!comp1accept && !comp2accept && !comp3accept)
        {
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Player Trade Unsuccessful";
            Summary.AddToSummary("P1: Player trade failed");
            MakeTradeButtonPressed();
        }
        else
        {
            if (comp1accept) { GameObject.Find("P2 Trade Accept Button").GetComponent<Button>().interactable = true; }
            if (comp2accept) { GameObject.Find("P3 Trade Accept Button").GetComponent<Button>().interactable = true; }
            if (comp3accept) { GameObject.Find("P4 Trade Accept Button").GetComponent<Button>().interactable = true; }
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Select who to trade with or cancel";
            GameObject.Find("Cancel Trade Button").GetComponent<Button>().interactable = true;
            DeactivateTradeScreenButtonsAndSliders(false);
        }
    }

    //When the player chooses which player to finalise the trade with, this method is called.
    //This gets the slider and input values. If the player decided to cancel the trade because they
    //didn't want to trade with any of the players who accepted the trade, then it will simply output a
    //message saying that the trade was cancelled and refresh the trade window. If the player decided to 
    //finalise the trade with any of the other players then the resource trade will happen and the players
    //resource cards will be updated accordingly. Then a message saying that the trade with that player was sucessfull
    //will display
    public void FinaliseTradeButtonPressed(int withPlayerNumber)
    {
        int[] resourcesGiven = new int[5];
        int[] resourcesWanted = new int[5];
        for (int i = 0; i < 5; i++)
        {
            resourcesGiven[i] = (int)givePlayerSelectionSliders[i].value;
            resourcesWanted[i] = int.Parse(wantPlayerSelectInputField[i].GetComponentInChildren<Text>().text);
        }

        if (withPlayerNumber == -1)
        {
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Player Trade Cancelled";
            Summary.AddToSummary("P1: Cancelled trade");
            MakeTradeButtonPressed();
        }
        else if(withPlayerNumber == 1)
        {
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Trade with P2 successful";
            Summary.AddToSummary("P1: Successfully made a trade with P2");
            for (int i = 0; i < 5; i++)
            {
                player.GainOrLoseResource(i + 1, resourcesWanted[i]);
                player.GainOrLoseResource(i + 1, resourcesGiven[i] * -1);
                compPlayer1.GainOrLoseResource(i + 1, resourcesWanted[i] * -1);
                compPlayer1.GainOrLoseResource(i + 1, resourcesGiven[i]);
            }
            MakeTradeButtonPressed();
        }
        else if (withPlayerNumber == 2)
        {
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Trade with P3 successful";
            Summary.AddToSummary("P1: Successfully made a trade with P3");
            for (int i = 0; i < 5; i++)
            {
                player.GainOrLoseResource(i + 1, resourcesWanted[i]);
                player.GainOrLoseResource(i + 1, resourcesGiven[i] * -1);
                compPlayer2.GainOrLoseResource(i + 1, resourcesWanted[i] * -1);
                compPlayer2.GainOrLoseResource(i + 1, resourcesGiven[i]);
            }
            MakeTradeButtonPressed();
        }
        else if (withPlayerNumber == 3)
        {
            GameObject.Find("Trade Result Text").GetComponent<Text>().text = "Trade with P4 successful";
            Summary.AddToSummary("P1: Successfully made a trade with P4");
            for (int i = 0; i < 5; i++)
            {
                player.GainOrLoseResource(i + 1, resourcesWanted[i]);
                player.GainOrLoseResource(i + 1, resourcesGiven[i] * -1);
                compPlayer3.GainOrLoseResource(i + 1, resourcesWanted[i] * -1);
                compPlayer3.GainOrLoseResource(i + 1, resourcesGiven[i]);
            }
            MakeTradeButtonPressed();
        }
        GameObject.Find("P2 Trade Accept Button").GetComponentInChildren<Text>().text = "P2";
        GameObject.Find("P3 Trade Accept Button").GetComponentInChildren<Text>().text = "P3";
        GameObject.Find("P4 Trade Accept Button").GetComponentInChildren<Text>().text = "P4";

    }

    //return the string of the card
    private string GetCard(int card)
    {
        if      (card == 1) { return "Brick";  }
        else if (card == 2) { return "Grain";  }
        else if (card == 3) { return "Lumber"; }
        else if (card == 4) { return "Ore";    }
        else if (card == 5) { return "Wool";   }
        else return "this shouldn't happen";
    }

    //This method deactivates the sliders and input fields on the player trade section.
    //The bool first is just an identifier for when this is being used as it is used
    //when going into this trade window and when a player trade is sent out. We want it to be true
    //when it is doing the former.
    private void DeactivateTradeScreenButtonsAndSliders(bool first)
    {
        GameObject.Find("Make DM Trade button").GetComponent<Button>().interactable = false;
        GameObject.Find("Make Player Trade button").GetComponent<Button>().interactable = false;
        for (int i = 0; i < 5; i++)
        {
            givePlayerSelectionSliders[i].interactable = false;
            //wantPlayerSelectInputField[i].interactable = false;
            if (first)
            {
                givePlayerSelectionSliders[i].value = 0;
                //wantPlayerSelectInputField[i].GetComponentInChildren<Text>().text = "0";
                wantPlayerSelectInputField[i].text = "0";
            }
        }
        if (first)
        {
            GameObject.Find("P2 Trade Accept Button").GetComponent<Button>().interactable = false;
            GameObject.Find("P3 Trade Accept Button").GetComponent<Button>().interactable = false;
            GameObject.Find("P4 Trade Accept Button").GetComponent<Button>().interactable = false;
            GameObject.Find("Cancel Trade Button").GetComponent<Button>().interactable    = false;
        }
    }

    //This method is for when the player recieves a trade from another player.
    //This is called when the player chooses whether they accept the trade or not.
    //Similar to how the player chooses from whoever accepted the trade to finalise the
    //trade with, the computer does the same. So even though the player may say accept, that
    //doesn't mean that the trade will happen with them.
    public void TradeResponse(bool playerResponse)
    {
        string player = GameObject.Find("Player Label").GetComponent<Text>().text;
        int[] resourcesGiven = new int[5];
        int[] resourcesWanted = new int[5];
        resourcesGiven[0] = int.Parse(GameObject.Find("Brick card WTT count" ).GetComponent<Text>().text);
        resourcesGiven[1] = int.Parse(GameObject.Find("Grain card WTT count" ).GetComponent<Text>().text);
        resourcesGiven[2] = int.Parse(GameObject.Find("Lumber card WTT count").GetComponent<Text>().text);
        resourcesGiven[3] = int.Parse(GameObject.Find("Ore card WTT count"   ).GetComponent<Text>().text);
        resourcesGiven[4] = int.Parse(GameObject.Find("Wool card WTT count"  ).GetComponent<Text>().text);

        resourcesWanted[0] = int.Parse(GameObject.Find("Brick card For count" ).GetComponent<Text>().text);
        resourcesWanted[1] = int.Parse(GameObject.Find("Grain card For count" ).GetComponent<Text>().text);
        resourcesWanted[2] = int.Parse(GameObject.Find("Lumber card For count").GetComponent<Text>().text);
        resourcesWanted[3] = int.Parse(GameObject.Find("Ore card For count"   ).GetComponent<Text>().text);
        resourcesWanted[4] = int.Parse(GameObject.Find("Wool card For count"  ).GetComponent<Text>().text);


        if      (player == "P2") { compPlayer1.FinaliseTrade(playerResponse, resourcesGiven, resourcesWanted); }
        else if (player == "P3") { compPlayer2.FinaliseTrade(playerResponse, resourcesGiven, resourcesWanted); }
        else if (player == "P4") { compPlayer3.FinaliseTrade(playerResponse, resourcesGiven, resourcesWanted); }
        GameObject.Find("Trade Offer Panel").GetComponent<RectTransform>().localPosition = new Vector3(-173, 1500, 0);
    }

    //--------------------------------------------------------------------------
    //Get and set methods for the development cards, dev card build buttons and other buttons

    //For each of these methods, I decrement 1 from the remaining amount of that dev card counter as well as decrement 1 from the
    //total dev cards remaining count. This is so that on the game screen, it updates the remaining dev cards to build counter.
    public static void RoadBuildingCardBuilt()
    {
        remainingRoadBuildingCards--;
        totalDevCardsRemaining--;
        GameObject.Find("Development Card Count Text").GetComponent<Text>().text = totalDevCardsRemaining.ToString();
    }

    public static void MonopolyCardBuilt()
    {
        remainingMonopolyCards--;
        totalDevCardsRemaining--;
        GameObject.Find("Development Card Count Text").GetComponent<Text>().text = totalDevCardsRemaining.ToString();
    }

    public static void YearOfPlentyCardBuilt()
    {
        remainingYearOfPlentyCards--;
        totalDevCardsRemaining--;
        GameObject.Find("Development Card Count Text").GetComponent<Text>().text = totalDevCardsRemaining.ToString();
    }

    public static void KnightCardBuilt()
    {
        remainingKnightCards--;
        totalDevCardsRemaining--;
        GameObject.Find("Development Card Count Text").GetComponent<Text>().text = totalDevCardsRemaining.ToString();
    }

    public static void VictoryPointCardBuilt()
    {
        remainingVictoryPointCards--;
        totalDevCardsRemaining--;
        GameObject.Find("Development Card Count Text").GetComponent<Text>().text = totalDevCardsRemaining.ToString();
    }

    //Getter methods for the dev cards that other classes can call
    public static int GetTotalDevCardsRemaining()     { return totalDevCardsRemaining;     }
    public static int GetRoadBuildingCardsRemaining() { return remainingRoadBuildingCards; }
    public static int GetMonopolyCardsRemaining()     { return remainingMonopolyCards;     }
    public static int GetYearOfPlentyCardsRemaining() { return remainingYearOfPlentyCards; }
    public static int GetKnightCardsRemaining()       { return remainingKnightCards;       }
    public static int GetVictoryPointCardsRemaining() { return remainingVictoryPointCards; }

    //--------------------------------------------------------------------------
    //These methods are for the help button which brings up the rules of the game and the navigation buttons within them
    public void HelpButtonPressed()
    {
        GameObject.Find("Help Panel 1").GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    public void CloseButtonPressed(int currentPanel)
    {
        GameObject.Find("Help Panel " + currentPanel).GetComponent<RectTransform>().localPosition = new Vector3(0, 1500 + (1000 * currentPanel), 0);
    }

    public void NextButtonPressed(int currentPanel)
    {
        GameObject.Find("Help Panel " + currentPanel).GetComponent<RectTransform>().localPosition = new Vector3(0, 1500 + (1000 * currentPanel), 0);
        GameObject.Find("Help Panel " + (currentPanel + 1)).GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    public void PreviousButtonPressed(int currentPanel)
    {
        GameObject.Find("Help Panel " + currentPanel).GetComponent<RectTransform>().localPosition = new Vector3(0, 1500 + (1000 * currentPanel), 0);
        GameObject.Find("Help Panel " + (currentPanel - 1)).GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
    }

    //--------------------------------------------------------------------------
    //initial turn round methods
    //These methods deal with the initial round of turns. Initial round 1 goes in order of
    //player 1, player 2, player 3 and player 4. Initial round 2 goes in the reverse order. In
    //these initial turns, the players each place a settlement and a road next to it.
    private void InitialTurnRound1()
    {
        if (currentTurn == 1)
        {
            if (!generated) { player.GeneratePlayerLegalSettlementPlacements(); }
            currentTurnText.text = "Player 1 initial placement";
            currentTurnText.color = Color.red;
            generated = true;
        }
        else if (currentTurn == 2)
        {
            currentTurnText.text = "Player 2 initial placement";
            currentTurnText.color = Color.yellow;
            compPlayer1.CompTurn();
            currentTurn++;
        }
        else if (currentTurn == 3)
        {
            currentTurnText.text = "Player 3 initial placement";
            currentTurnText.color = Color.white;
            compPlayer2.CompTurn();
            currentTurn++;
        }
        else if (currentTurn == 4)
        {
            currentTurnText.text = "Player 4 initial placement";
            currentTurnText.color = Color.blue;
            compPlayer3.CompTurn();
            currentTurn++;
        }
        else if (currentTurn > 4)
        {
            currentTurn = 4;
            initialRound1 = false;
            initialRound2 = true;
        }
    }

    private void InitialTurnRound2()
    {
        if (currentTurn == 1)
        {
            if (!generated) { player.GeneratePlayerLegalSettlementPlacements(); }
            currentTurnText.text = "Player 1 second placement";
            currentTurnText.color = Color.red;
            generated = true;
        }
        else if (currentTurn == 2)
        {
            currentTurnText.text = "Player 2 second placement";
            currentTurnText.color = Color.yellow;
            compPlayer1.CompTurn();
            currentTurn--;
        }
        else if (currentTurn == 3)
        {
            currentTurnText.text = "Player 3 second placement";
            currentTurnText.color = Color.white;
            compPlayer2.CompTurn();
            currentTurn--;
        }
        else if (currentTurn == 4)
        {
            currentTurnText.text = "Player 4 second placement";
            currentTurnText.color = Color.blue;
            compPlayer3.CompTurn();
            currentTurn--;
        }
        else if (currentTurn <1)
        {
            currentTurn = 1;
            initialRound2 = false;
        }
    }

    public static bool GetInitial1() { return initialRound1; }
    public static bool GetInitial2() { return initialRound2; }

    public static void InitialTurnDone()
    {
        if (initialRound2) { currentTurn--; }
        else               { currentTurn++; }
        generated = false;
    }
}
