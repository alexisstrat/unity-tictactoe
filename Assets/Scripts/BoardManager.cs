using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Use this for training")]
    public bool AIOnly;
    
    [Tooltip("What symbol the player has")]
    public Player humanPlayer = Player.X;
    
    public PlayersTurn playersTurn = PlayersTurn.X;

    [Header("References")]
    public PlayerAgent player1; // Player X
    public PlayerAgent player2; // Player O
    public Slot[] _slots;

    private ObjectPooler _objectPooler;
    private bool _playerChanged;

    private int _availableSlots = 0;
    [HideInInspector]
    public int slotsX = 0;
    [HideInInspector]
    public int slotsO = 0;
    
    private BoardState _boardState = BoardState.Draw;
    
    [HideInInspector]
    public bool canPlay;

    // Event to invoke when a match has finished
    public delegate void MatchFinishedDelegate(BoardState boardState);
    public event MatchFinishedDelegate OnMatchFinished;

    void Start()
    {
        _objectPooler = GetComponent<ObjectPooler>();
        
        if(AIOnly)
            _playerChanged = true;
    }
    
    void FixedUpdate()
    {
        if (AIOnly)
        {
            canPlay = true;
        }
        else if ((int) playersTurn == (int) humanPlayer)
        {
            return;
        }
        
        if (_playerChanged)
        {
            _playerChanged = false;
            if(!AIOnly)
                StartCoroutine(RequestDecision());
            else
            {
                GetCurrentPlayer().RequestDecision();
            }
        }
    }

    /// <summary>
    /// Used when a human player is playing the game to
    /// delay AI's move.
    /// </summary>
    IEnumerator RequestDecision()
    {
        yield return new WaitForSeconds(1f);
        GetCurrentPlayer().RequestDecision();
        yield return null;
    }
    
    
    private PlayerAgent GetCurrentPlayer()
    {
        if (playersTurn == PlayersTurn.X)
        {
            return player1;
        }
        return player2;
    }
    
    
    public void ChangeTurn()
    {
        playersTurn = playersTurn == PlayersTurn.O ? PlayersTurn.X : PlayersTurn.O;
        _playerChanged = true;
    }
    
    /// <summary>
    /// Agent choosing a slot
    /// </summary>
    public void SelectSlot(int slotNo, Player player)
    {
        _slots[slotNo].currentState = player == Player.X ? SlotState.X : SlotState.O;
        if (!AIOnly)
        {
            GameObject go = _objectPooler.GetPooledObject(player == Player.X ? _objectPooler.pooledXs : _objectPooler.pooledOs);
            go.transform.position = _slots[slotNo].gameObject.transform.position;
            go.SetActive(true);
            go.transform.DOScale(Vector3.one, 0.2f).From();
        }

        if (player == Player.O)
        {
            slotsO++;
        }
        else
        {
            slotsX++;
        }
        
        CheckForWinner();
    }

    /// <summary>
    /// Human player choosing a slot
    /// </summary>
    public void SelectSlotHuman(GameObject slot)
    {
        GameObject go = _objectPooler.GetPooledObject(playersTurn == PlayersTurn.X ? _objectPooler.pooledXs : _objectPooler.pooledOs);
        go.transform.position = slot.transform.position;
        go.SetActive(true);
        go.transform.DOScale(Vector3.one, 0.2f).From();

        CheckForWinner();
    }

    private void CheckForWinner()
    {
        _availableSlots--;
        
        GetBoardState();

        if (_availableSlots > 0 && _boardState == BoardState.Draw)
        {
            ChangeTurn();
            return;
        }
        
        // Reward 1, and -1 for winner and loser respectively 
        if (_boardState == BoardState.X)
        {
            player1.SetReward(1f);
            player2.SetReward(-1f);
        }
        else if (_boardState == BoardState.O)
        {
            player1.SetReward(-1f);
            player2.SetReward(1f);
        } 
        // Reward the 2nd player in case it is a draw and penalize the 1st player
        else if (_boardState == BoardState.Draw)
        {
            if (slotsO > slotsX)
            {
                player1.SetReward(.25f);
                player2.SetReward(-.25f);
            }
            else
            {
                player1.SetReward(-.25f);
                player2.SetReward(.25f);
            }
        }
        
        player1.EndEpisode();
        player2.EndEpisode();

        if (!AIOnly)
        {
            MatchFinished();
            return;
        }
        
        ResetBoard();
    }
    
    /// <summary>
    ///    ._._._.
    ///    |0|1|2|
    ///    |3|4|5|
    ///    |6|7|8|
    ///    `-`-`-`
    /// First we add the horizontal ((0,1,2),(3,4,5) etc)
    /// and vertical ((0,3,6),(1,4,7) etc) rows and check if we have
    /// a winner. If not we add the diagonal rows and perform the same addition check.
    /// </summary>
    private void GetBoardState()
    {
        // Check first for horizontal or vertical winner
        for (int i = 0; i < 3; i++)
        {
            int horizontalValue = (int) _slots[i * 3].currentState + (int) _slots[i * 3 + 1].currentState +
                                  (int) _slots[i * 3 + 2].currentState;
            int verticalValue = (int) _slots[i].currentState + (int) _slots[i + 3].currentState +
                                (int) _slots[i + 6].currentState;

            if (GetWinner(horizontalValue, verticalValue))
            {
                return;
            }
        }

        // Check for diagonal winner
        int diagonalTopLeftValue =
            (int) _slots[0].currentState + (int) _slots[4].currentState + (int) _slots[8].currentState;
        int diagonalTopRightValue =
            (int) _slots[2].currentState + (int) _slots[4].currentState + (int) _slots[6].currentState;


        GetWinner(diagonalTopLeftValue, diagonalTopRightValue);
    }
    
    /// <summary>
    /// Since the SlotState for X=1 and O=-1 we check if the addition of the rows
    /// are either 3 or -3 and determine who won.
    /// </summary>
    private bool GetWinner(int row1, int ro2)
    {
        if (row1 == 3 || ro2 == 3)
        {
            _boardState = BoardState.X;
            return true;
        }

        if (row1 == -3 || ro2 == -3)
        {
            _boardState = BoardState.O;
            return true;
        }

        _boardState = BoardState.Draw;
        return false;
    }

    /// <summary>
    /// Agent requesting the available slots on the board
    /// </summary>
    /// <returns>The available slots one can position X or O</returns>
    public IEnumerable<int> GetAvailableSlots()
    {
        List<int> availableFields = new List<int>();

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].currentState == SlotState.Free)
                availableFields.Add(i);
        }

        return availableFields.ToArray();
    }
    
    /// <summary>
    /// Used by Agent to add to the mask to filter the next action
    /// </summary>
    public IEnumerable<int> FilledSlots()
    {
        List<int> filledSlots = new List<int>();

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].currentState != SlotState.Free)
                filledSlots.Add(i);
        }

        return filledSlots.ToArray();
    }

    /// <summary>
    /// Resets the board...woah
    /// </summary>
    public void ResetBoard()
    {
        foreach (var slot in _slots)
        {
            slot.ResetSlot();
        }

        _objectPooler.ResetPool();
        _availableSlots = 9;
        _boardState = BoardState.Draw;
        slotsO = 0;
        slotsX = 0;
        ChangeTurn();
    }

    public void MatchFinished()
    {
        OnMatchFinished?.Invoke(_boardState);
    }
}

public enum Player
{
    X = 0,
    O = 1
}

public enum PlayersTurn
{
    X = 0,
    O = 1
}

public enum BoardState
{
    Draw,
    X,
    O
}
