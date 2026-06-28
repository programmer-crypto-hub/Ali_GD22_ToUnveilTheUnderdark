using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public NetworkBool mouseLeftClick;
    public NetworkBool mouseRightClick;

    public NetworkBool jumpPressed;
    public NetworkBool attackPressed;
    public NetworkBool diceRollPressed;
    public NetworkBool endTurnPressed;
    public NetworkBool interactPressed;
    public NetworkBool toggleShopPressed;
    public NetworkBool toggleInventoryPressed;

    public Vector3 direction;
}