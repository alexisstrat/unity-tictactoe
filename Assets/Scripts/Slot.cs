using UnityEngine;

public class Slot : MonoBehaviour
{
    
    public BoardManager boardManager;
    public SlotState currentState = SlotState.Free;

    /// <summary>
    /// Handles player's input
    /// </summary>
    private void OnMouseDown()
    {
        if(!boardManager.canPlay) return;
        
        if (currentState != SlotState.Free || boardManager.AIOnly) return;

        if ((int) boardManager.playersTurn != (int) boardManager.humanPlayer) return;
        
        currentState = boardManager.playersTurn == PlayersTurn.X ? SlotState.X : SlotState.O;
        if (boardManager.playersTurn == PlayersTurn.O)
        {
            boardManager.slotsO++;
        }
        else
        {
            boardManager.slotsX++;
        }

        boardManager.SelectSlotHuman(gameObject);
    }

    /// <summary>
    /// Called whenever a match is finished and
    /// changes the state of the slot
    /// </summary>
    public void ResetSlot()
    {
        currentState = SlotState.Free;
    }
}

public enum SlotState
{
    Free = 0,
    X = 1,
    O = -1
}