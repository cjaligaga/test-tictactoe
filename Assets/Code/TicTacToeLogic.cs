using System;
using UnityEngine;
using UnityEngine.Events;

public class TicTacToeLogic
{
    const uint GRID_BOX_COUNT = 9;
    
    Symbol[] symbolGrid = new Symbol[GRID_BOX_COUNT];
    
    public UnityAction<TurnState> onTurnStateChanged = null;
    
    public TurnState CurrenTurnState { get; private set; } = TurnState.None;
    public Symbol WinningSymbol { get; private set; } = Symbol.None;

    public void StartLogic()
    {
        ClearStates();
        SwitchTurn();
    }

    void SwitchTurn()
    {
        CurrenTurnState = CurrenTurnState == TurnState.PlayerO ? TurnState.PlayerX : TurnState.PlayerO;
        onTurnStateChanged.Invoke (CurrenTurnState);
    }
    
    void ClearStates()
    {
        symbolGrid = new Symbol[GRID_BOX_COUNT];

        for (var x = 0; x < symbolGrid.Length; x++)
        {
            symbolGrid[x] = Symbol.None;
        }
        
        CurrenTurnState = TurnState.None;
        WinningSymbol = Symbol.None;
    }

    public void PlaceSymbol (int index)
    {
        if (index < 0 || index >= symbolGrid.Length) return;
        
        Symbol symbolOnBox = symbolGrid [index];
        
        if (symbolOnBox != Symbol.None) return;

        symbolGrid [index] = CurrenTurnState switch
        {
            TurnState.PlayerO => Symbol.O,
            TurnState.PlayerX => Symbol.X,
            _ => symbolGrid [index]
        };
        
        AssessTurn (symbolGrid [index]);
    }

    void AssessTurn (Symbol symbolOnTurn)
    {
        for (int i = 0; i < winningPatterns.GetLength(0); i++)
        {
            if (symbolGrid[winningPatterns[i, 0]] != symbolOnTurn ||
                symbolGrid[winningPatterns[i, 1]] != symbolOnTurn ||
                symbolGrid[winningPatterns[i, 2]] != symbolOnTurn) continue;
            
            WinningSymbol = symbolOnTurn;
        }

        if (WinningSymbol != Symbol.None || Array.TrueForAll (symbolGrid, symbol => symbol != Symbol.None))
        {
            CurrenTurnState = TurnState.End;
            onTurnStateChanged.Invoke (CurrenTurnState);
            return;
        }
        
        SwitchTurn();
    }
    
    static readonly int[,] winningPatterns = {
        {0, 1, 2}, 
        {3, 4, 5}, 
        {6, 7, 8},
        {0, 3, 6}, 
        {1, 4, 7}, 
        {2, 5, 8}, 
        {0, 4, 8}, 
        {2, 4, 6}            
    };
}