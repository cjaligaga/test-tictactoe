using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Button))]
public class GridButton : NetworkBehaviour 
{
    [SerializeField] TMP_Text symbolTM;
    
    [Networked, OnChangedRender (nameof(OnSymbolChanged))] 
    public Symbol CurrentSymbol { get; set; }
    
    [NonSerialized] public Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_Target_ChangeInteractability ([RpcTarget] PlayerRef player, bool interactability)
    {
        ChangeInteractability(interactability);
    }

    [Rpc (RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ChangeInteractability (bool interactability)
    {
        ChangeInteractability(interactability);
    }

    void ChangeInteractability (bool interactability)
    {
        button.interactable = interactability;
    }

    public void OnSymbolChanged ()
    {
        symbolTM.text = CurrentSymbol switch
        {
            Symbol.None => string.Empty,
            Symbol.X => "X",
            Symbol.O => "O",
            _ => string.Empty
        };
    }
    
    public void OnClick()
    {
        RPC_Clicked();
    }

    [Rpc (sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    void RPC_Clicked (RpcInfo info = default)
    {
        TicTacToeManager.instance.MakeMove (transform.GetSiblingIndex());
    }
}
