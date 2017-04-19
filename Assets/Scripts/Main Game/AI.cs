using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//This class deals with how the computer player acts in a turn.
//This class decides what action should the computer player do from a
//list of available legal moves. The way it decides this is determined
//by what AI setting the computer player has been set to.
public class AI : MonoBehaviour {

    private string aiSetting;
    private int playerNumber;
    private Board board;
    private bool[] resourcesWanted = new bool[5];
    private bool[] resourcesWantedDMtrade = new bool[5];
    private bool[] resourcesWillingToGiveDMtrade = new bool[5];
    private int[] resourcesWantedPtrade = new int[5];
    private int[] resourcesWillingToGivePtrade = new int[5];
    private int[] previousPlayerTradeWanted = new int[5];
    private int[] previousPlayerTradeGiven = new int[5];
    private string priority;
    private bool lastPlayerTradeCancelled;
    private bool lastPlayerTradeFailed;
    private int tradeCounter = -1;
    private static int devCardsBuilt = 0;

    //Initial setup of AI
    public void SetSettings(string setting, Board givenBoard, int pNumber)
    {
        aiSetting = setting;
        print(setting);
        playerNumber = pNumber;
        board = givenBoard;
        priority = "any";
        lastPlayerTradeCancelled = false;
        lastPlayerTradeFailed = false;
        ResetResourceArrays();
        if (setting == "Quick Thinker" || setting == "Random") { tradeCounter = 0; }
        if (setting == "Smart Thinker") { SetResourceWantedArray("settlement"); }
        for(int i=0; i<5; i++)
        {
            previousPlayerTradeWanted[i] = 0;
            previousPlayerTradeGiven[i] = 0;
        }
    }

    //--------------------------------------------------------------
    //Reset array methods. These are use to reset the various arrays that are in this class.
    private void ResetResourceArrays()
    {
        for (int i = 0; i < 5; i++)
        {
            resourcesWanted[i] = false;
            resourcesWantedDMtrade[i] = false;
            resourcesWillingToGiveDMtrade[i] = false;
            resourcesWantedPtrade[i] = 0;
            resourcesWillingToGivePtrade[i] = 0;
        }
    }

    private void ResetTradeWantResourceArrays()
    {
        for (int i = 0; i < 5; i++)
        {
            resourcesWantedDMtrade[i] = false;
            resourcesWantedPtrade[i] = 0;
        }
    }

    private void ResetPreviousTradeArrays()
    {
        for (int i = 0; i < 5; i++)
        {
            previousPlayerTradeGiven[i] = 0;
            previousPlayerTradeWanted[i] = 0;
        }
    }

    //----------------------------------------------------------------------------------
    //These methods deals with deciding what move to do.

    //This method selects a move for the computer player 
    //from a list of available legal moves to do. The way it is set up is that 
    //if the computer player has any development cards, then it will prioritise doing that action first
    //as it can only benefit the computer player. In the event that they don't have a development card,
    //it will set a priority for trading and then proceed to generate a build plan. 
    public int SelectMoveToMake(List<int> listOfLegalMoves, 
        int amountOfLegalSettlementPlacements, int amountOfLegalRoadPlacements, int amountOfLegalCityPlacements, int totalDevCardsRemaining,
        int[] resourcesCurrentlyHolding, int totalResourceCardsInHand,
        int remainingRoadsToBuild, int remainingSettlementsToBuild, int remainingCitiesToBuild)
    {
        ResetResourceArrays();
        priority = SetPriorityForTrading(amountOfLegalSettlementPlacements, amountOfLegalRoadPlacements, amountOfLegalCityPlacements,
            totalDevCardsRemaining, resourcesCurrentlyHolding, remainingRoadsToBuild, remainingSettlementsToBuild, remainingCitiesToBuild);
        SetResourceWantedArray(priority);

        if      (listOfLegalMoves.Contains(5)) { return 5; }
        else if (listOfLegalMoves.Contains(6)) { return 6; }
        else if (listOfLegalMoves.Contains(7)) { return 7; }
        else if (listOfLegalMoves.Contains(8)) { return 8; }
        else if (listOfLegalMoves.Contains(9)) { return 9; }
        else
        {
            return GenerateBuildPlan(listOfLegalMoves,
                amountOfLegalSettlementPlacements, amountOfLegalRoadPlacements, amountOfLegalCityPlacements, totalDevCardsRemaining,
                resourcesCurrentlyHolding, totalResourceCardsInHand,
                remainingRoadsToBuild, remainingSettlementsToBuild, remainingCitiesToBuild, priority);
        }
    }

    //This method sets the priority for trading. It does this by checking how close to each
    //piece to build the computer AI are by checking what they currently hold and comparing it
    //to what is needed to build the piece. This comparison check varies depending on the AI 
    //setting of the computer player. For the Random player, it just returns any, for the Quick Thinker
    //it will check for which one of the build plans is one resource away from being built (prioritising in the order
    //city>settlement>road>dev card), for the Smart Thinker is that it checks for which build plan they have at least
    //one resource of (with the same priority ranking as the Quick Thinker) and for the All Around Thinker finds which of the build plans
    //its closest to building (such that every build plan is more likely to get picked).
    string SetPriorityForTrading(int amountOfLegalSettlementPlacements, int amountOfLegalRoadPlacements, int amountOfLegalCityPlacements, 
        int totalDevCardsRemaining, int[] resourcesCurrentlyHolding, int remainingRoadsToBuild, int remainingSettlementsToBuild, int remainingCitiesToBuild)
    {
        if (aiSetting == "Random") { return "any"; }

        if (aiSetting == "Quick Thinker")
        {
            if (checkHowManyDifferentResourcesNeededForCity(resourcesCurrentlyHolding) == 1 && amountOfLegalCityPlacements > 0 && remainingCitiesToBuild > 0) { return "city"; }
            else if (checkHowManyDifferentResourcesNeededForSettlement(resourcesCurrentlyHolding) == 1 && amountOfLegalSettlementPlacements > 0 && remainingSettlementsToBuild > 0) { return "settlement"; }
            else if (checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding) == 1 && amountOfLegalSettlementPlacements == 0 && amountOfLegalRoadPlacements > 0 && remainingRoadsToBuild > 0) { return "road"; }
            else if (checkHowManyDifferentResourcesNeededForDevCard(resourcesCurrentlyHolding) == 1 && totalDevCardsRemaining > 0) { return "dev card"; }
        }
        else if (aiSetting == "Smart Thinker")
        {
            if (checkHowManyDifferentResourcesNeededForCity(resourcesCurrentlyHolding) < 5 && amountOfLegalCityPlacements > 0 && remainingCitiesToBuild > 0) { return "city"; }
            else if (checkHowManyDifferentResourcesNeededForSettlement(resourcesCurrentlyHolding) < 4 && amountOfLegalSettlementPlacements > 0 && remainingSettlementsToBuild > 0) { return "settlement"; }
            else if (checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding) < 2 && amountOfLegalSettlementPlacements == 0 && amountOfLegalRoadPlacements > 0 && remainingRoadsToBuild > 0) { return "road"; }
            else if (checkHowManyDifferentResourcesNeededForDevCard(resourcesCurrentlyHolding) < 3 && totalDevCardsRemaining > 0) { return "dev card"; }
        }
        else if (aiSetting == "All Around Thinker")
        {
            int[] list = { checkHowManyDifferentResourcesNeededForSettlement(resourcesCurrentlyHolding),
                           checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding),
                           checkHowManyDifferentResourcesNeededForCity(resourcesCurrentlyHolding),
                           checkHowManyDifferentResourcesNeededForDevCard(resourcesCurrentlyHolding) };
            int least = GetTheLeast(list);

            while (true)
            {
                if (checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding) == least && amountOfLegalRoadPlacements > 0 && remainingRoadsToBuild > 0 && board.DoesLegalRoadsLeadToGettingLongestRoadCard(playerNumber)) { return "road"; }
                else if (checkHowManyDifferentResourcesNeededForCity(resourcesCurrentlyHolding) == least && amountOfLegalCityPlacements > 0 && remainingCitiesToBuild > 0) { return "city"; }
                else if (checkHowManyDifferentResourcesNeededForSettlement(resourcesCurrentlyHolding) == least && amountOfLegalSettlementPlacements > 0 && remainingSettlementsToBuild > 0) { return "settlement"; }
                else if (checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding) == least && amountOfLegalSettlementPlacements == 0 && amountOfLegalRoadPlacements > 0 && remainingRoadsToBuild > 0) { return "road"; }
                else if (checkHowManyDifferentResourcesNeededForDevCard(resourcesCurrentlyHolding) == least && totalDevCardsRemaining > 0) { return "dev card"; }
                else { least++; }
            }
        }
        return "any";
    }

    //This is a helper method that gets the least from a given list
    private int GetTheLeast(int[] list)
    {
        int currLeast = 0;
        int least = 10;
        for (int i = 1; i < list.Length; i++)
        {
            if (list[i] < list[currLeast])
            {
                least = list[i];
                currLeast = i;
            }
            else { least = list[currLeast]; }
        }
        return least;
    }

    //This method sets the resources wanted array based on what the priority for trading has been set to
    private void SetResourceWantedArray(string pieceToBuild)
    {
        if (pieceToBuild == "settlement")
        {
            resourcesWanted[0] = true;
            resourcesWanted[1] = true;
            resourcesWanted[2] = true;
            resourcesWanted[4] = true;
        }
        else if (pieceToBuild == "road")
        {
            resourcesWanted[0] = true;
            resourcesWanted[2] = true;
        }
        else if (pieceToBuild == "city")
        {
            resourcesWanted[1] = true;
            resourcesWanted[3] = true;
        }
        else if (pieceToBuild == "dev card")
        {
            resourcesWanted[1] = true;
            resourcesWanted[3] = true;
            resourcesWanted[4] = true;
        }
    }

    //This method generates the build plan. This gets called when the computer player doesn't have any development cards on them.
    //The way this method works is that it checks from a ranked priority of moves which build moves it can do. If there are no
    //build moves that can be done, then it will try and set up a trade. For the Random player it just selects a random move in the
    //list of legal moves to do. In the event that it decides to try a trade, then it will set up a possible trade.
    //For the other AI settings, there is a random probabilty for trading that is set and each AI setting has a different
    //threshold for deciding whether they want to trade. Quick Thinker has a higher threshold (making it less likely to trade),
    //Smart Thinker has a lower threshold (making it more likely to trade) and All Around Thinker has no threshold (meaning it will always trade
    //when it has the opportunity).
    private int GenerateBuildPlan(List<int> listOfLegalMoves,
        int amountOfLegalSettlementPlacements, int amountOfLegalRoadPlacements, int amountOfLegalCityPlacements, int totalDevCardsRemaining,
        int[] resourcesCurrentlyHolding, int totalResourceCardsInHand,
        int remainingRoadsToBuild, int remainingSettlementsToBuild, int remainingCitiesToBuild, string priority)
    {
        int tradeType = -1;
        float d = Random.RandomRange(0f, 1f); //probability for building a dev card
        float p = Random.RandomRange(0f, 1f); //probability for trading

        if (aiSetting == "Random")
        {
            int m = listOfLegalMoves[Random.RandomRange(0, listOfLegalMoves.Count)];
            if ((m == 10 || m == 11) && (p > 0.5)) { tradeType = SetUpPossibleTrade(resourcesCurrentlyHolding, priority, totalResourceCardsInHand); }
            else { return m; }

            if (tradeType == 0) { return 10; }
            else if (tradeType == 1) { return 11; }
        }

        if (aiSetting == "Quick Thinker" || aiSetting == "Smart Thinker" || aiSetting == "All Around Thinker")
        {
            bool buildDevCard = (aiSetting == "Quick Thinker" && d > 0.75) || (aiSetting == "Smart Thinker" && d > 0.5) || (aiSetting == "All Around Thinker");
            bool willTrade = (aiSetting == "Quick Thinker" && p > 0.75) || (aiSetting == "Smart Thinker" && p > 0.25) || (aiSetting == "All Around Thinker");
            
            if      (aiSetting == "All Around Thinker" && board.DoesLegalRoadsLeadToGettingLongestRoadCard(playerNumber) && listOfLegalMoves.Contains(1)) { return 1; } //build road if the road that is being built leads to getting the longest road.
            else if (listOfLegalMoves.Contains(3)) { return 3; } //build city
            else if (listOfLegalMoves.Contains(2)) { return 2; } //build settlement
            else if (listOfLegalMoves.Contains(1) && amountOfLegalSettlementPlacements == 0) { return 1; } //build road
            else if (listOfLegalMoves.Contains(4) && buildDevCard) { return 4; } //build dev card
            else if (tradeCounter != 2 && !lastPlayerTradeFailed && willTrade && !lastPlayerTradeCancelled) { tradeType = SetUpPossibleTrade(resourcesCurrentlyHolding, priority, totalResourceCardsInHand); }

            if      (tradeType == 0) { return 10; }
            else if (tradeType == 1) { return 11; }
        }
        return 0;

    }

    //------------------------------------------------------------------------------------------
    //These are called by the Computer class

    //This method happens when everyone has rejected the trade offer
    public void LastPlayerTradeFailed()
    {
        if (aiSetting != "Quick Thinker") { tradeCounter++; }
        else                              { lastPlayerTradeFailed = true; }
    }

    //This method resets the trading variables
    public void ResetLastPlayerTradeCheck()
    {
        lastPlayerTradeFailed = false;
        lastPlayerTradeCancelled = false;
        if (aiSetting != "Quick Thinker") { tradeCounter = -1;  }
        else                              { tradeCounter =  0;  }
    }

    public void LastPlayerTradeCancelled() { lastPlayerTradeCancelled = true; }

    //-----------------------------------------------------------------------------------------
    //These methods are called by the SetPriorityForTrading method to check for how many resources
    //the computer player are away from building different pieces. It also sets the resources wanted arrays with
    //the resources that are needed to make the piece.
    private int checkHowManyDifferentResourcesNeededForSettlement(int[] resourcesCurrentlyHolding)
    {
        ResetTradeWantResourceArrays();
        int count = 0;
        if (resourcesCurrentlyHolding[0] == 0) { count++; resourcesWantedDMtrade[0] = true; resourcesWantedPtrade[0] = 1; }
        if (resourcesCurrentlyHolding[1] == 0) { count++; resourcesWantedDMtrade[1] = true; resourcesWantedPtrade[1] = 1; }
        if (resourcesCurrentlyHolding[2] == 0) { count++; resourcesWantedDMtrade[2] = true; resourcesWantedPtrade[2] = 1; }
        if (resourcesCurrentlyHolding[4] == 0) { count++; resourcesWantedDMtrade[4] = true; resourcesWantedPtrade[4] = 1; }
        return count;
    }

    private int checkHowManyDifferentResourcesNeededForCity(int[] resourcesCurrentlyHolding)
    {
        ResetTradeWantResourceArrays();
        int count = 0;
        if (resourcesCurrentlyHolding[1] < 2) { count = count + (2 - resourcesCurrentlyHolding[1]); resourcesWantedDMtrade[1] = true; resourcesWantedPtrade[1] = 2 - resourcesCurrentlyHolding[1]; }
        if (resourcesCurrentlyHolding[3] < 3) { count = count + (3 - resourcesCurrentlyHolding[3]); resourcesWantedDMtrade[3] = true; resourcesWantedPtrade[3] = 3 - resourcesCurrentlyHolding[3]; }
        return count;
    }

    private int checkHowManyDifferentResourcesNeededForRoad(int[] resourcesCurrentlyHolding)
    {
        ResetTradeWantResourceArrays();
        int count = 0;
        if (resourcesCurrentlyHolding[0] == 0) { count++; resourcesWantedDMtrade[0] = true; resourcesWantedPtrade[0] = 1; }
        if (resourcesCurrentlyHolding[2] == 0) { count++; resourcesWantedDMtrade[2] = true; resourcesWantedPtrade[2] = 1; }
        return count;
    }

    private int checkHowManyDifferentResourcesNeededForDevCard(int[] resourcesCurrentlyHolding)
    {
        ResetTradeWantResourceArrays();
        int count = 0;
        if (resourcesCurrentlyHolding[1] == 0) { count++; resourcesWantedDMtrade[1] = true; resourcesWantedPtrade[1] = 1; }
        if (resourcesCurrentlyHolding[3] == 0) { count++; resourcesWantedDMtrade[3] = true; resourcesWantedPtrade[3] = 1; }
        if (resourcesCurrentlyHolding[4] == 0) { count++; resourcesWantedDMtrade[4] = true; resourcesWantedPtrade[4] = 1; }
        return count;
    }

    //------------------------------------------------------------------------------------------------------
    //These methods set up the possible trades that the computer player could make

    //This method checks whether the computer player can make a Domestic/Maritime trade or a player trade, checking
    //them in that order.
    private int SetUpPossibleTrade(int[] resourcesCurrentlyHolding, string pieceToBuild, int totalResourceCardsInHand)
    {
        if      (IsDMtradePossible(resourcesCurrentlyHolding, pieceToBuild, totalResourceCardsInHand))     { return 0; }
        else if (IsPlayerTradePossible(resourcesCurrentlyHolding, pieceToBuild, totalResourceCardsInHand)) { return 1; }
        else { return -1; }
    }

    //This method checks to see if a Domestic/Maritime trade is possible. It does this by getting what the minimum is needed
    //to be given for each resource in this type of trade. Then for each build plan it checks to see which of the resources to give in.
    //For each case it prioritises resources that aren't needed to build the piece in question. For example, to build a settlement
    //you need 1 brick, 1 grain, 1 lumber and 1 wool. So it will check to see if the computer player has any ore to trade for this.
    //If not then it will check for the rest of the resources whether the computer players have any of those resources to spare to give
    //in to this trade. For the case where the build plan is any, then it will trade the highest resource that the computer player holds in
    //exchange for a resource that they hold the least of. If they can't find a possible trade from this, then this returns false.
    private bool IsDMtradePossible(int[] resourcesCurrentlyHolding, string pieceToBuild, int totalResourceCardsInHand)
    {
        int minimumDMbrick  = board.getMinimumNeededToGiveForResource(1, playerNumber);
        int minimumDMgrain  = board.getMinimumNeededToGiveForResource(2, playerNumber);
        int minimumDMlumber = board.getMinimumNeededToGiveForResource(3, playerNumber);
        int minimumDMore    = board.getMinimumNeededToGiveForResource(4, playerNumber);
        int minimumDMwool   = board.getMinimumNeededToGiveForResource(5, playerNumber);

        if (pieceToBuild == "settlement")
        {
            if      (resourcesCurrentlyHolding[3]     >= minimumDMore    && !resourcesWantedDMtrade[3]) { resourcesWillingToGiveDMtrade[3] = true; } //Ore
            else if (resourcesCurrentlyHolding[0] - 1 >= minimumDMbrick  && !resourcesWantedDMtrade[0]) { resourcesWillingToGiveDMtrade[0] = true; } //Brick
            else if (resourcesCurrentlyHolding[1] - 1 >= minimumDMgrain  && !resourcesWantedDMtrade[1]) { resourcesWillingToGiveDMtrade[1] = true; } //Grain
            else if (resourcesCurrentlyHolding[2] - 1 >= minimumDMlumber && !resourcesWantedDMtrade[2]) { resourcesWillingToGiveDMtrade[2] = true; } //Lumber
            else if (resourcesCurrentlyHolding[4] - 1 >= minimumDMwool   && !resourcesWantedDMtrade[4]) { resourcesWillingToGiveDMtrade[4] = true; } //Wool
        }
        else if (pieceToBuild == "road")
        {
            if      (resourcesCurrentlyHolding[1]     >= minimumDMgrain  && !resourcesWantedDMtrade[1]) { resourcesWillingToGiveDMtrade[1] = true; } //Grain
            else if (resourcesCurrentlyHolding[3]     >= minimumDMore    && !resourcesWantedDMtrade[3]) { resourcesWillingToGiveDMtrade[3] = true; } //Ore
            else if (resourcesCurrentlyHolding[4]     >= minimumDMwool   && !resourcesWantedDMtrade[4]) { resourcesWillingToGiveDMtrade[4] = true; } //Wool
            else if (resourcesCurrentlyHolding[0] - 1 >= minimumDMbrick  && !resourcesWantedDMtrade[0]) { resourcesWillingToGiveDMtrade[0] = true; } //Brick
            else if (resourcesCurrentlyHolding[2] - 1 >= minimumDMlumber && !resourcesWantedDMtrade[2]) { resourcesWillingToGiveDMtrade[2] = true; } //Lumber
        }
        else if (pieceToBuild == "city")
        {
            if      (resourcesCurrentlyHolding[0]     >= minimumDMbrick  && !resourcesWantedDMtrade[0]) { resourcesWillingToGiveDMtrade[0] = true; } //Brick
            else if (resourcesCurrentlyHolding[2]     >= minimumDMlumber && !resourcesWantedDMtrade[2]) { resourcesWillingToGiveDMtrade[2] = true; } //Lumber
            else if (resourcesCurrentlyHolding[4]     >= minimumDMwool   && !resourcesWantedDMtrade[4]) { resourcesWillingToGiveDMtrade[4] = true; } //Wool
            else if (resourcesCurrentlyHolding[1] - 2 >= minimumDMgrain  && !resourcesWantedDMtrade[1]) { resourcesWillingToGiveDMtrade[1] = true; } //Grain
            else if (resourcesCurrentlyHolding[3] - 3 >= minimumDMore    && !resourcesWantedDMtrade[3]) { resourcesWillingToGiveDMtrade[3] = true; } //Ore
        }
        else if (pieceToBuild == "dev card")
        {
            if      (resourcesCurrentlyHolding[0]     >= minimumDMbrick  && !resourcesWantedDMtrade[0]) { resourcesWillingToGiveDMtrade[0] = true; } //Brick
            else if (resourcesCurrentlyHolding[2]     >= minimumDMlumber && !resourcesWantedDMtrade[2]) { resourcesWillingToGiveDMtrade[2] = true; } //Lumber
            else if (resourcesCurrentlyHolding[1] - 1 >= minimumDMgrain  && !resourcesWantedDMtrade[1]) { resourcesWillingToGiveDMtrade[1] = true; } //Grain
            else if (resourcesCurrentlyHolding[3] - 1 >= minimumDMore    && !resourcesWantedDMtrade[3]) { resourcesWillingToGiveDMtrade[3] = true; } //Ore
            else if (resourcesCurrentlyHolding[4] - 1 >= minimumDMwool   && !resourcesWantedDMtrade[4]) { resourcesWillingToGiveDMtrade[4] = true; } //Wool
        }
        else if (pieceToBuild == "any" && (aiSetting == "Quick Thinker" || aiSetting == "Random") && totalResourceCardsInHand > 7)
        {
            int cardHighest = -1;
            for (int i=0; i<5; i++)
            {
                if(resourcesCurrentlyHolding[i] > cardHighest) { cardHighest = i; }
            }
            if      (resourcesCurrentlyHolding[0] >= minimumDMbrick  && cardHighest == 0) { resourcesWillingToGiveDMtrade[0] = true; } //Brick
            else if (resourcesCurrentlyHolding[1] >= minimumDMgrain  && cardHighest == 1) { resourcesWillingToGiveDMtrade[1] = true; } //Grain
            else if (resourcesCurrentlyHolding[2] >= minimumDMlumber && cardHighest == 2) { resourcesWillingToGiveDMtrade[2] = true; } //Lumber
            else if (resourcesCurrentlyHolding[3] >= minimumDMore    && cardHighest == 3) { resourcesWillingToGiveDMtrade[3] = true; } //Ore
            else if (resourcesCurrentlyHolding[4] >= minimumDMwool   && cardHighest == 4) { resourcesWillingToGiveDMtrade[4] = true; } //Wool
        }

        for (int i = 0; i< 5; i++)
        {
            if (resourcesWillingToGiveDMtrade[i])
            {
                ResetPreviousTradeArrays();
                return true;
            }
        }
        return false;
    }

    //This method checks to see if a player trade is possible. It does this by setting up what the computer player is
    //willing to give in the trade. They will not put in a a resource that the player wants. The amount of resources that the
    //computer players give in is dependant on how many resources they need to build the piece they want to build with the addition
    //or subtraction of whether the computer is doing an unfair, fair or generous trade. In the event that the build plan is any
    //it will put in one of the highest resource being held in exchange for the lowest. If a trade can't be set up then this will return false
    private bool IsPlayerTradePossible(int[] resourcesCurrentlyHolding, string pieceToBuild, int totalResourceCardsInHand)
    {
        if (pieceToBuild == "settlement")
        {
            int count = checkHowManyDifferentResourcesNeededForSettlement(resourcesCurrentlyHolding);
            if (count <= 1 && tradeCounter < 0) { tradeCounter = 0; }
            count = count + tradeCounter;
            while (count > 0)
            {
                if      (resourcesCurrentlyHolding[3]     - resourcesWillingToGivePtrade[3] > 0 && resourcesWantedPtrade[3] == 0) { resourcesWillingToGivePtrade[3]++; } //Ore
                else if (resourcesCurrentlyHolding[0] - 1 - resourcesWillingToGivePtrade[0] > 0 && resourcesWantedPtrade[0] == 0) { resourcesWillingToGivePtrade[0]++; } //Brick
                else if (resourcesCurrentlyHolding[1] - 1 - resourcesWillingToGivePtrade[1] > 0 && resourcesWantedPtrade[1] == 0) { resourcesWillingToGivePtrade[1]++; } //Grain
                else if (resourcesCurrentlyHolding[2] - 1 - resourcesWillingToGivePtrade[2] > 0 && resourcesWantedPtrade[2] == 0) { resourcesWillingToGivePtrade[2]++; } //Lumber
                else if (resourcesCurrentlyHolding[4] - 1 - resourcesWillingToGivePtrade[4] > 0 && resourcesWantedPtrade[4] == 0) { resourcesWillingToGivePtrade[4]++; } //Wool
                else { count = 0; }
                count--;
            }
        }
        else if (pieceToBuild == "road")
        {
            int count = checkHowManyDifferentResourcesNeededForRoad(resourcesCurrentlyHolding);
            if (count <= 1 && tradeCounter < 0) { tradeCounter = 0; }
            count = count + tradeCounter;

            while (count > 0)
            {
                if      (resourcesCurrentlyHolding[1]     - resourcesWillingToGivePtrade[1] > 0 && resourcesWantedPtrade[1] == 0) { resourcesWillingToGivePtrade[1]++; } //Grain
                else if (resourcesCurrentlyHolding[3]     - resourcesWillingToGivePtrade[3] > 0 && resourcesWantedPtrade[3] == 0) { resourcesWillingToGivePtrade[3]++; } //Ore
                else if (resourcesCurrentlyHolding[4]     - resourcesWillingToGivePtrade[4] > 0 && resourcesWantedPtrade[4] == 0) { resourcesWillingToGivePtrade[4]++; } //Wool
                else if (resourcesCurrentlyHolding[0] - 1 - resourcesWillingToGivePtrade[0] > 0 && resourcesWantedPtrade[0] == 0) { resourcesWillingToGivePtrade[0]++; } //Brick
                else if (resourcesCurrentlyHolding[2] - 1 - resourcesWillingToGivePtrade[2] > 0 && resourcesWantedPtrade[2] == 0) { resourcesWillingToGivePtrade[2]++; } //Lumber
                else { count = 0; }
                count--;
            }
        }
        else if (pieceToBuild == "city")
        {
            int count = checkHowManyDifferentResourcesNeededForCity(resourcesCurrentlyHolding);
            if (count <= 1 && tradeCounter < 0) { tradeCounter = 0; }
            count = count + tradeCounter;

            while (count > 0)
            {
                if      (resourcesCurrentlyHolding[0]     - resourcesWillingToGivePtrade[0] > 0 && resourcesWantedPtrade[0] == 0) { resourcesWillingToGivePtrade[0]++; } //Brick
                else if (resourcesCurrentlyHolding[2]     - resourcesWillingToGivePtrade[2] > 0 && resourcesWantedPtrade[2] == 0) { resourcesWillingToGivePtrade[2]++; } //Lumber
                else if (resourcesCurrentlyHolding[4]     - resourcesWillingToGivePtrade[4] > 0 && resourcesWantedPtrade[4] == 0) { resourcesWillingToGivePtrade[4]++; } //Wool
                else if (resourcesCurrentlyHolding[1] - 2 - resourcesWillingToGivePtrade[1] > 0 && resourcesWantedPtrade[1] == 0) { resourcesWillingToGivePtrade[1]++; } //Grain
                else if (resourcesCurrentlyHolding[3] - 3 - resourcesWillingToGivePtrade[3] > 0 && resourcesWantedPtrade[3] == 0) { resourcesWillingToGivePtrade[3]++; } //Ore
                else { count = 0; }
                count--;
            }
        }
        else if (pieceToBuild == "dev card")
        {
            int count = checkHowManyDifferentResourcesNeededForDevCard(resourcesCurrentlyHolding);
            if (count <= 1 && tradeCounter < 0) { tradeCounter = 0; }
            count = count + tradeCounter;

            while (count > 0)
            {
                if      (resourcesCurrentlyHolding[0]     - resourcesWillingToGivePtrade[0] > 0 && resourcesWantedPtrade[0] == 0) { resourcesWillingToGivePtrade[0]++; } //Brick
                else if (resourcesCurrentlyHolding[2]     - resourcesWillingToGivePtrade[2] > 0 && resourcesWantedPtrade[2] == 0) { resourcesWillingToGivePtrade[2]++; } //Lumber
                else if (resourcesCurrentlyHolding[1] - 1 - resourcesWillingToGivePtrade[1] > 0 && resourcesWantedPtrade[1] == 0) { resourcesWillingToGivePtrade[1]++; } //Wool
                else if (resourcesCurrentlyHolding[3] - 1 - resourcesWillingToGivePtrade[3] > 0 && resourcesWantedPtrade[3] == 0) { resourcesWillingToGivePtrade[3]++; } //Ore
                else if (resourcesCurrentlyHolding[4] - 1 - resourcesWillingToGivePtrade[4] > 0 && resourcesWantedPtrade[4] == 0) { resourcesWillingToGivePtrade[4]++; } //Wool
                else { count = 0; }
                count--;
            }
        }
        else if (pieceToBuild == "any" && (aiSetting == "Quick Thinker" || aiSetting == "Random") && totalResourceCardsInHand > 7)
        {
            int cardHighest = -1;
            int highestCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (resourcesCurrentlyHolding[i] > highestCount) { cardHighest = i; }
            }
            if      (resourcesCurrentlyHolding[0] > 0 && cardHighest == 0) { resourcesWillingToGivePtrade[0] = 1; } //Brick
            else if (resourcesCurrentlyHolding[1] > 0 && cardHighest == 1) { resourcesWillingToGivePtrade[1] = 1; } //Grain
            else if (resourcesCurrentlyHolding[2] > 0 && cardHighest == 2) { resourcesWillingToGivePtrade[2] = 1; } //Lumber
            else if (resourcesCurrentlyHolding[3] > 0 && cardHighest == 3) { resourcesWillingToGivePtrade[3] = 1; } //Ore
            else if (resourcesCurrentlyHolding[4] > 0 && cardHighest == 4) { resourcesWillingToGivePtrade[4] = 1; } //Wool
        }

        SetUpFairTrade(pieceToBuild, resourcesCurrentlyHolding);

        int totalResourcesWillingToGive = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] + resourcesWillingToGivePtrade[2]
                                        + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4];
        int totalResourcesWanted = resourcesWantedPtrade[0] + resourcesWantedPtrade[1] + resourcesWantedPtrade[2]
                                 + resourcesWantedPtrade[3] + resourcesWantedPtrade[4];
        
        if (!(previousPlayerTradeWanted == resourcesWantedPtrade && previousPlayerTradeGiven == resourcesWillingToGivePtrade)
          &&!(totalResourcesWanted == 0 || totalResourcesWillingToGive == 0) && !lastPlayerTradeCancelled)
        {
            for (int i = 0; i < 5; i++)
            {
                if (resourcesWillingToGivePtrade[i] > 0) { return true; }
            }
        }
        return false;
    }

    //This method sets up a fairer trade for the resources wanted. This will differ depending on
    //whether the computer player is going for a unfair, fair or generous trade.
    private void SetUpFairTrade(string pieceToBuild, int[] resourcesCurrentlyHolding)
    {
        if (pieceToBuild == "settlement")
        {
            int p = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] +
                    resourcesWillingToGivePtrade[2] + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4] - tradeCounter;
            resourcesWantedPtrade[0] = 0;
            resourcesWantedPtrade[1] = 0;
            resourcesWantedPtrade[2] = 0;
            resourcesWantedPtrade[4] = 0;
            while (p > 0)
            {
                if      (resourcesWantedPtrade[0] == 0 && resourcesCurrentlyHolding[0] == 0) { resourcesWantedPtrade[0]++; }
                else if (resourcesWantedPtrade[1] == 0 && resourcesCurrentlyHolding[1] == 0) { resourcesWantedPtrade[1]++; }
                else if (resourcesWantedPtrade[2] == 0 && resourcesCurrentlyHolding[2] == 0) { resourcesWantedPtrade[2]++; }
                else if (resourcesWantedPtrade[4] == 0 && resourcesCurrentlyHolding[4] == 0) { resourcesWantedPtrade[4]++; }
                p--;
            }
        }
        else if (pieceToBuild == "road")
        {
            int p = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] +
                    resourcesWillingToGivePtrade[2] + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4] - tradeCounter;
            resourcesWantedPtrade[0] = 0;
            resourcesWantedPtrade[2] = 0;
            while (p > 0)
            {
                if      (resourcesWantedPtrade[0] == 0 && resourcesCurrentlyHolding[0] == 0) { resourcesWantedPtrade[0]++; }
                else if (resourcesWantedPtrade[2] == 0 && resourcesCurrentlyHolding[2] == 0) { resourcesWantedPtrade[2]++; }
                p--;
            }
        }
        else if (pieceToBuild == "city")
        {
            int p = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] +
                    resourcesWillingToGivePtrade[2] + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4] - tradeCounter;
            resourcesWantedPtrade[1] = 0;
            resourcesWantedPtrade[3] = 0;
            while (p > 0)
            {
                if      (resourcesWantedPtrade[1] < 2 && resourcesCurrentlyHolding[1] < 2) { resourcesWantedPtrade[1]++; }
                else if (resourcesWantedPtrade[3] < 3 && resourcesCurrentlyHolding[3] < 3) { resourcesWantedPtrade[3]++; }
                p--;
            }
        }
        else if (pieceToBuild == "dev card")
        {
            int p = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] +
                    resourcesWillingToGivePtrade[2] + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4] - tradeCounter;
            resourcesWantedPtrade[1] = 0;
            resourcesWantedPtrade[3] = 0;
            resourcesWantedPtrade[4] = 0;
            while (p > 0)
            {
                if      (resourcesWantedPtrade[1] == 0 && resourcesCurrentlyHolding[1] == 0) { resourcesWantedPtrade[1]++; }
                else if (resourcesWantedPtrade[3] == 0 && resourcesCurrentlyHolding[3] == 0) { resourcesWantedPtrade[3]++; }
                else if (resourcesWantedPtrade[4] == 0 && resourcesCurrentlyHolding[4] == 0) { resourcesWantedPtrade[4]++; }
                p--;
            }
        }
        else if (pieceToBuild == "any")
        {
            int p = resourcesWillingToGivePtrade[0] + resourcesWillingToGivePtrade[1] +
                    resourcesWillingToGivePtrade[2] + resourcesWillingToGivePtrade[3] + resourcesWillingToGivePtrade[4];
            resourcesWantedPtrade[0] = 0;
            resourcesWantedPtrade[1] = 0;
            resourcesWantedPtrade[2] = 0;
            resourcesWantedPtrade[3] = 0;
            resourcesWantedPtrade[4] = 0;
            while (p > 0)
            {
                if      (resourcesWantedPtrade[0] == 0 && resourcesCurrentlyHolding[0] == 0) { resourcesWantedPtrade[0]++; }
                else if (resourcesWantedPtrade[1] == 0 && resourcesCurrentlyHolding[1] == 0) { resourcesWantedPtrade[1]++; }
                else if (resourcesWantedPtrade[2] == 0 && resourcesCurrentlyHolding[2] == 0) { resourcesWantedPtrade[2]++; }
                else if (resourcesWantedPtrade[3] == 0 && resourcesCurrentlyHolding[3] == 0) { resourcesWantedPtrade[3]++; }
                else if (resourcesWantedPtrade[4] == 0 && resourcesCurrentlyHolding[4] == 0) { resourcesWantedPtrade[4]++; }
                p--;
            }
        }
    }

    //----------------------------------------------------------------------------------------
    //This method deals with when the computer players uses either the monopoly or year of plenty development card.
    //It does this by checking which build plan it is on and goes for that resource if it doesn't have it.
    public int ChooseResourceForMonopolyOrYopCard(int[] resourcesCurrentlyHolding)
    {
        if (aiSetting == "Random") { return Random.RandomRange(1, 6); }

        if      (resourcesCurrentlyHolding[0]     < 1  && (priority == "settlement" || priority == "road")) { return 1; }
        else if ((resourcesCurrentlyHolding[1]    < 1  && (priority == "settlement" || priority == "dev card")) 
                || (resourcesCurrentlyHolding[1]  < 2  &&  priority == "city")) { return 2; }
        else if (resourcesCurrentlyHolding[2]     < 1  && (priority == "settlement" || priority == "road")) { return 3; }
        else if ((resourcesCurrentlyHolding[3]    < 1  &&  priority == "dev card")  || (resourcesCurrentlyHolding[3] < 3 && priority == "city")) { return 4; }
        else if (resourcesCurrentlyHolding[4]     < 1  && (priority == "settlement" || priority == "dev card")) { return 5; }

        else return Random.RandomRange(1, 6);
    }

    //----------------------------------------------------------------------------------------
    //These methods sets up all the trading stuff for the Computer class to use
    public int SelectDomesticMaritimeResourceToGive()
    {
        for (int i = 0; i < 5; i++)
        {
            if (resourcesWillingToGiveDMtrade[i]) { return i + 1; }
        }
        return -1;
    }

    public int SelectDomesticMaritimeResourceToGet()
    {
        if (aiSetting == "Random")
        {
            int g = Random.RandomRange(0, 5);
            while (resourcesWillingToGiveDMtrade[g]) { g = Random.RandomRange(0, 5); }
            return g + 1;
        }
        for (int i = 0; i < 5; i++)
        {
            if (resourcesWantedDMtrade[i]) { return i + 1; }
        }
        return -1;
    }

    public int[] SelectPlayerTradeToGive()
    {
        int[] resourcesGiven = new int[5];
        for (int i = 0; i < 5; i++)
        {
            resourcesGiven[i] = resourcesWillingToGivePtrade[i];
            previousPlayerTradeGiven[i] = resourcesWillingToGivePtrade[i];
        }
        return resourcesGiven;
    }

    public int[] SelectPlayerTradeToWant()
    {
        int[] playerResourcesWanted = new int[5];
        for (int i = 0; i < 5; i++) { playerResourcesWanted[i] = 0; }
        if (aiSetting == "Random")
        {
            int g = Random.RandomRange(0, 5);
            while (resourcesWillingToGivePtrade[g] != 0) { g = Random.RandomRange(0, 5); }
            for (int i = 0; i < 5; i++)
            {
                if (g == i) { playerResourcesWanted[i] = 1; }
                else        { playerResourcesWanted[i] = 0; }
            }
            return playerResourcesWanted;
        }

        for (int i = 0; i < 5; i++)
        {
            playerResourcesWanted[i] = resourcesWantedPtrade[i];
            previousPlayerTradeWanted[i] = resourcesWantedPtrade[i];
        }
        return playerResourcesWanted;
    }

    public int SelectPlayerToTradeWith(List<int> listOfPeopleToTradeWith)
    {
        int size = listOfPeopleToTradeWith.Count;
        if (aiSetting == "Random") { return listOfPeopleToTradeWith[Random.RandomRange(0, size)]; }
        else if (aiSetting == "Quick Thinker" || aiSetting == "Smart Thinker" || aiSetting == "All Around Thinker")
        {
            List<int> listOfLastPlacePlayers = ScoreBoard.GetListOfLastPlacePlayers(playerNumber, listOfPeopleToTradeWith);
            int lowestScoringListSize = listOfLastPlacePlayers.Count;
            if (lowestScoringListSize > 0) { return listOfLastPlacePlayers[Random.RandomRange(0, lowestScoringListSize)]; }
            else
            {
                for (int i = 1; i < size; i++)
                {
                    int p = 0;
                    if (listOfPeopleToTradeWith[i] == playerNumber) { p = listOfPeopleToTradeWith[i] + 1; }
                    else { p = listOfPeopleToTradeWith[i]; }
                    if (!ScoreBoard.DoesPlayerHaveHighestTotalScore(p)) { return listOfPeopleToTradeWith[i]; }
                }
            }
        }
        return 0;
    }

    public int SelectResponseToTrade(int[] resourcesCurrentlyHolding, int offeringPlayer, int[] resourcesWantedByOfferingPlayer, int[] resourcesGivenByOfferingPlayer)
    {
        if (aiSetting == "Random") { return Random.RandomRange(0, 2); }
        else if (aiSetting == "Quick Thinker")
        {
            if (ScoreBoard.DoesPlayerHaveHighestTotalScore(offeringPlayer)) { return 0; } //reject trade
            else { return 1; } //accept trade
        }
        else if (aiSetting == "Smart Thinker" || aiSetting == "All Around Thinker")
        {
            int p = 0;

            if (DoesPlayerOfferHelpFulfillBuildPlan(resourcesCurrentlyHolding, resourcesGivenByOfferingPlayer))   { p = p + 5; } //if the resources given help fulfill some build plan, then add a big weight to it
            if (DoesPlayerOfferHelpPreventBuildPlan(resourcesCurrentlyHolding, resourcesWantedByOfferingPlayer))  { p = p - 5; } //if the resources wanted help prevent some build plan, then subtrace a big weight to it

            if (ScoreBoard.DoesPlayerHaveHighestTotalScore(offeringPlayer)) { p = p - 3; } //if the offering player is leading, then subract a weight to it

            if ((p >= 5 && aiSetting == "Smart Thinker") || (p >= 2 && aiSetting == "All Around Thinker")) { return 1; } //reject trade. All around thinker is will trade if it benefits their build plan even if offering player is winning. Smart Thinker won't trade if offering player is winning.
            else                                                                                           { return 0; } //accept trade
        }
        return 0;
    }

    //This methods checks for a build plan that the AI is currently on, if the resources given by the offering player in a trade
    //contributes to completing that build plan.
    private bool DoesPlayerOfferHelpFulfillBuildPlan(int[] resourcesCurrentlyHolding, int[] resourcesGivenByOfferingPlayer)
    {
        if (priority == "settlement")
        {
            if (resourcesGivenByOfferingPlayer[0] > 0 && resourcesCurrentlyHolding[0] == 0) { return true; } //If it provides the Brick resource needed
            if (resourcesGivenByOfferingPlayer[1] > 0 && resourcesCurrentlyHolding[1] == 0) { return true; } //If it provides the Grain resource needed
            if (resourcesGivenByOfferingPlayer[2] > 0 && resourcesCurrentlyHolding[2] == 0) { return true; } //If it provides the Lumber resource needed
            if (resourcesGivenByOfferingPlayer[4] > 0 && resourcesCurrentlyHolding[4] == 0) { return true; } //If it provides the Wool resource needed
            return false;
        }
        else if (priority == "road")
        {
            if (resourcesGivenByOfferingPlayer[0] > 0 && resourcesCurrentlyHolding[0] == 0) { return true; } //If it provides the Brick resource needed
            if (resourcesGivenByOfferingPlayer[2] > 0 && resourcesCurrentlyHolding[2] == 0) { return true; } //If it provides the Lumber resource needed
            return false;
        }
        else if (priority == "city")
        {
            int grainNeeded = 2 - resourcesCurrentlyHolding[1];
            int oreNeeded = 3 - resourcesCurrentlyHolding[3];
            if (resourcesGivenByOfferingPlayer[1] > 0 && grainNeeded > 0) { return true; } //If it provides some of the Grain resources needed
            if (resourcesGivenByOfferingPlayer[3] > 0 && oreNeeded   > 0) { return true; } //If it provides some of the Ore resources needed
            return false;    
        }
        else if (priority == "dev card")
        {
            if (resourcesGivenByOfferingPlayer[1] > 0 && resourcesCurrentlyHolding[1] == 0) { return true; } //If it provides the Grain resource needed
            if (resourcesGivenByOfferingPlayer[3] > 0 && resourcesCurrentlyHolding[3] == 0) { return true; } //If it provides the Ore resource needed
            if (resourcesGivenByOfferingPlayer[4] > 0 && resourcesCurrentlyHolding[4] == 0) { return true; } //If it provides the Wool resource needed
            return false;
        }
        return false;
    }

    //This method checks for a build that the AI is on, if the resources that the offering player wants from the AI
    //helps prevents the build plan from being done
    private bool DoesPlayerOfferHelpPreventBuildPlan(int[] resourcesCurrentlyHolding, int[] resourcesWantedByOfferingPlayer)
    {
        if (priority == "settlement")
        {
            if (resourcesCurrentlyHolding[0] - resourcesWantedByOfferingPlayer[0] == 0) { return true; } //If it takes away the Brick resource needed
            if (resourcesCurrentlyHolding[1] - resourcesWantedByOfferingPlayer[1] == 0) { return true; } //If it takes away the Grain resource needed
            if (resourcesCurrentlyHolding[2] - resourcesWantedByOfferingPlayer[2] == 0) { return true; } //If it takes away the Lumber resource needed
            if (resourcesCurrentlyHolding[4] - resourcesWantedByOfferingPlayer[4] == 0) { return true; } //If it takes away the Wool resource needed
            return false;
        }
        else if (priority == "road")
        {
            if (resourcesCurrentlyHolding[0] - resourcesWantedByOfferingPlayer[0] == 0) { return true; } //If it takes away the Brick resource needed
            if (resourcesCurrentlyHolding[2] - resourcesWantedByOfferingPlayer[2] == 0) { return true; } //If it takes away the Lumber resource needed
            return false;
        }
        else if (priority == "city")
        {
            int grainNeeded = 2 - resourcesCurrentlyHolding[1];
            int oreNeeded = 3 - resourcesCurrentlyHolding[3];
            if (grainNeeded - resourcesWantedByOfferingPlayer[1] < grainNeeded) { return true; } //If it takes away some of the Grain resources needed
            if (oreNeeded   - resourcesWantedByOfferingPlayer[3] < oreNeeded)   { return true; } //If it takes away some of the Ore resources needed
            return false;
        }
        else if (priority == "dev card")
        {
            if (resourcesCurrentlyHolding[1] - resourcesWantedByOfferingPlayer[1] == 0) { return true; } //If it takes away the Grain resource needed
            if (resourcesCurrentlyHolding[3] - resourcesWantedByOfferingPlayer[3] == 0) { return true; } //If it takes away the Ore resource needed
            if (resourcesCurrentlyHolding[4] - resourcesWantedByOfferingPlayer[4] == 0) { return true; } //If it takes away the Wool resource needed
            return false;
        }
        return false;
    }

    //-----------------------------------------------------------------------------
    //These methods deal with selecting which spot on the board to place pieces on.

    //This method selects which spot on the board to place a settlement on
    public GameObject SelectSettlementPlacement(List<GameObject> listOfValidSettlementSpots)
    {
        int size = listOfValidSettlementSpots.Count;
        if (aiSetting == "Random") { return listOfValidSettlementSpots[Random.Range(0, size)]; }
        else { return listOfValidSettlementSpots[PickSettlementCitySpotFromWeights(listOfValidSettlementSpots, true)]; }
    }

    //This method selects which spot on the board to place a road on
    public GameObject SelectRoadPlacement(List<GameObject> listOfValidRoadSpots)
    {
        int size = listOfValidRoadSpots.Count;
        if      (aiSetting == "Random")        { return listOfValidRoadSpots[Random.Range(0, size)]; }
        else { return listOfValidRoadSpots[PickRoadSpotFromWeights(listOfValidRoadSpots)]; }
    }

    //This method selects which spot on the board to place a city on
    public GameObject SelectCityPlacement(List<GameObject> listOfValidCitySpots)
    {
        int size = listOfValidCitySpots.Count;
        if      (aiSetting == "Random")        { return listOfValidCitySpots[Random.Range(0, size)]; }
        else { return listOfValidCitySpots[PickSettlementCitySpotFromWeights(listOfValidCitySpots, false)]; }
    }

    //This method selects which spot on the board to place the robber on
    public GameObject SelectNewRobberLocation(List<GameObject> listOfValidRobberSpots)
    {
        int size = listOfValidRobberSpots.Count;
        if (aiSetting == "Random") { return listOfValidRobberSpots[Random.Range(0, size)]; }
        else { return listOfValidRobberSpots[PickRobberSpotFromWeights(listOfValidRobberSpots)]; }
    }

    //------------------------------------------------------------------------
    //These method pick a spot for the above methods to return based on the weight values for each spot
    private int PickSettlementCitySpotFromWeights(List<GameObject> listOfValidSpots, bool isSettlementPlacement)
    {
        int highestWeight = -1000;
        List<int> highestWeightedSpots = new List<int>();
        for(int i=0; i<listOfValidSpots.Count; i++)
        {
            int w = board.GetWeightForSettlementCitySpot(listOfValidSpots[i], resourcesWanted, isSettlementPlacement, aiSetting, playerNumber);
            if(w > highestWeight)
            {
                highestWeightedSpots.RemoveRange(0, highestWeightedSpots.Count);
                highestWeight = w;
                highestWeightedSpots.Add(i);
            }
            else if(w == highestWeight)
            {
                highestWeightedSpots.Add(i);
            }
        }
        if (highestWeightedSpots.Count > 1) { return highestWeightedSpots[Random.RandomRange(0, highestWeightedSpots.Count)]; }
        else { return highestWeightedSpots[0]; }
    }

    private int PickRoadSpotFromWeights(List<GameObject> listOfValidRoadSpots)
    {
        int highestWeight = -1000;
        List<int> highestWeightedSpots = new List<int>();
        for (int i = 0; i < listOfValidRoadSpots.Count; i++)
        {
            int w = board.GetWeightForRoadSpot(listOfValidRoadSpots[i], resourcesWanted, playerNumber, priority, aiSetting);
            if (w > highestWeight)
            {
                highestWeightedSpots.RemoveRange(0, highestWeightedSpots.Count);
                highestWeight = w;
                highestWeightedSpots.Add(i);
            }
            else if (w == highestWeight)
            {
                highestWeightedSpots.Add(i);
            }
        }
        if (highestWeightedSpots.Count > 1) { return highestWeightedSpots[Random.RandomRange(0, highestWeightedSpots.Count)]; }
        else { return highestWeightedSpots[0]; }
    }

    private int PickRobberSpotFromWeights(List<GameObject> listOfValidRobberSpots)
    {
        int highestWeight = -1000;
        List<int> highestWeightedSpots = new List<int>();

        for (int i = 0; i < listOfValidRobberSpots.Count; i++)
        {
            int w = board.GetWeightForRobberSpot(listOfValidRobberSpots[i],playerNumber,aiSetting);
            if (w > highestWeight)
            {
                highestWeightedSpots.RemoveRange(0, highestWeightedSpots.Count);
                highestWeight = w;
                highestWeightedSpots.Add(i);
            }
            else if (w == highestWeight)
            {
                highestWeightedSpots.Add(i);
            }
        }
        if (highestWeightedSpots.Count > 1) { return highestWeightedSpots[Random.RandomRange(0, highestWeightedSpots.Count)]; }
        else { return highestWeightedSpots[0]; }
    }

}
