using System;
using System.Linq;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] TMP_Text gameMessageTM;
    [SerializeField] TMP_Text playerSymbolTM;
    [SerializeField] Transform gridButtonPanel;
    [SerializeField] Button resetButton;
    [SerializeField] ParticleSystem confettiParticleSystem;
    
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
        bool IsTie = winningSymbol == Symbol.None;
        RPC_SetGameMessage (!IsTie ? $"Player {winningSymbol} won!" : $"It's a tie!");
        RPC_ToggleResetButtonInteractability (true);
        
        if (!IsTie)
            RPC_Target_PlayConfettiParticleSystem (winningSymbol == Symbol.O ? playerO : playerX, true);
        
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
    void RPC_Target_SetPlayerSymbolTM ([RpcTarget] PlayerRef player, Symbol assignedSymbol)
    {
        playerSymbolTM.SetText ($"You are:\n{assignedSymbol.ToString()}");
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetGameMessage (string message)
    {
        SetGameMessage (message);
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Target_SetGameMessage ([RpcTarget] PlayerRef player, string message)
    {
        SetGameMessage (message);
    }

    void SetGameMessage (string message)
    {
        gameMessageTM.transform.DOPunchScale (Vector3.one * 0.15f, 0.25f, vibrato: 5);
        gameMessageTM.text = message;
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_ToggleResetButtonInteractability (bool isInteractable)
    {
        resetButton.transform.DOPunchScale (Vector3.one * 0.15f, 0.5f);
        resetButton.interactable = isInteractable;
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_Target_PlayConfettiParticleSystem ([RpcTarget] PlayerRef player, bool playConfetti)
    {
        if (playConfetti) confettiParticleSystem.Play();
        else confettiParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlayConfettiParticleSystem (bool playConfetti)
    {
        if (playConfetti) confettiParticleSystem.Play();
        else confettiParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
        RPC_PlayConfettiParticleSystem (false);
        
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
        
        RPC_Target_SetPlayerSymbolTM (playerO, Symbol.O);
        RPC_Target_SetPlayerSymbolTM (playerX, Symbol.X);

        StartGame();
    }

    public void PlayerLeft (PlayerRef player)
    {
        gameMessageTM.text = "Opponent left. Please close the game.";
        resetButton.interactable = false;
        
        foreach (GridButton gridButton in gridButtons)
        {
            gridButton.button.interactable = false;
        }
    }
}