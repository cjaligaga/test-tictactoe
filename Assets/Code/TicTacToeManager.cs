using System;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] TMP_Text gameMessageTM;
    [SerializeField] Transform gridButtonPanel;
    [SerializeField] Button resetButton;
    
    public static TicTacToeManager instance;

    readonly TicTacToeLogic logic = new ();
    readonly GridButton[] gridButtons = new GridButton[9];

    PlayerRef playerO;
    PlayerRef playerX;
    
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        logic.onTurnStateChanged += OnTurnStateChanged;
        
        resetButton.interactable = false;
        
        for (int i = 0; i < gridButtons.Length; i++)
        {
            if ( gridButtonPanel.GetChild (i).TryGetComponent (out GridButton gridButton))
            {
                gridButtons[i] = gridButton;
                gridButton.button.interactable = false;
            }
        }
    }

    void OnTurnStateChanged (TurnState turnState)
    {
        if (turnState == TurnState.End)
        {
            OnGameEnd (logic.WinningSymbol);
            return;
        }

        PlayerRef whoseTurn = turnState == TurnState.PlayerO ? playerO : playerX;
        PlayerRef notTurn = turnState == TurnState.PlayerO ? playerX : playerO;
        
        foreach (GridButton gridButton in gridButtons)
        {
            gridButton.RPC_Target_ChangeInteractability (whoseTurn, true);
            gridButton.RPC_Target_ChangeInteractability (notTurn, false);
        }

        RPC_Target_SetGameMessage (whoseTurn, "Your Turn");
        RPC_Target_SetGameMessage (notTurn, "Opponent's Turn");
    }

    void OnGameEnd (Symbol winningSymbol)
    {
        RPC_SetGameMessage (winningSymbol != Symbol.None ? $"Player {winningSymbol} won!" : $"It's a tie!");
        RPC_ToggleResetButtonInteractability (true);
        
        foreach (GridButton gridButton in gridButtons)
        {
            gridButton.RPC_ChangeInteractability (false);
        }
    }

    public void MakeMove (int index)
    {
        if (gridButtons[index].CurrentSymbol != Symbol.None) return;
        
        gridButtons[index].CurrentSymbol = logic.CurrenTurnState switch
        {
            TurnState.PlayerX => Symbol.X,
            TurnState.PlayerO => Symbol.O,
            _ => gridButtons[index].CurrentSymbol
        };
        
        logic.PlaceSymbol (index);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetGameMessage (string message)
    {
        gameMessageTM.text = message;
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Target_SetGameMessage ([RpcTarget] PlayerRef player, string message)
    {
        gameMessageTM.text = message;
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ToggleResetButtonInteractability (bool isInteractable)
    {
        resetButton.interactable = isInteractable;
    }

    public void OnResetButtonClicked()
    {
        RPC_OnResetButtonClicked();
    }

    [Rpc (RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_OnResetButtonClicked()
    {
        RestartGame();
    }

    void RestartGame()
    {
        foreach (GridButton gridButton in gridButtons)
        {
            gridButton.CurrentSymbol = Symbol.None;
        }

        StartGame();
    }

    void StartGame()
    {
        RPC_ToggleResetButtonInteractability (false);
        logic.StartLogic();
    }
    
    public void PlayerJoined (PlayerRef player)
    {
        if (Runner.ActivePlayers.Count() != 2) return;

        var playerRefs = Runner.ActivePlayers.ToArray();
        
        playerO = playerRefs[0];
        playerX = playerRefs[1];

        StartGame();
    }

    public void PlayerLeft (PlayerRef player)
    {
        gameMessageTM.text = "Someone left. Please close the game.";
        resetButton.interactable = false;
        
        foreach (GridButton gridButton in gridButtons)
        {
            gridButton.button.interactable = false;
        }
    }
}