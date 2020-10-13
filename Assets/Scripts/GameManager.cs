using DG.Tweening;
using TMPro;
using Unity.Barracuda;
using Unity.MLAgents.Policies;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager boardManager;

    public GameObject mainUI;
    public GameObject playButton;
    public TMP_Text difficultyText;
    public TMP_Text winner;

    [Header("Trained Models")] 
    public NNModel normal;
    public NNModel hard;
    
    private int _difficulty = 0;
    
    void Start()
    {
        mainUI.SetActive(true);
        boardManager.OnMatchFinished += ShowWinner;
        playButton.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StartGame()
    {
        SetDifficulty();
        mainUI.SetActive(false);
        boardManager.ResetBoard();
        boardManager.canPlay = true;
    }

    public void ChangeDifficulty()
    {
        _difficulty = _difficulty == 0 ? _difficulty = 1 : _difficulty = 0;
        difficultyText.text = _difficulty == 0 ? "Normal" : "Hard";
    }
    
    private void SetDifficulty()
    {
        switch (_difficulty)
        {
            case 0:
                boardManager.player1.GetComponent<BehaviorParameters>().Model = normal;
                boardManager.player2.GetComponent<BehaviorParameters>().Model = normal;
                break;
            default:
                boardManager.player1.GetComponent<BehaviorParameters>().Model = hard;
                boardManager.player2.GetComponent<BehaviorParameters>().Model = hard;
                break;
        }
    }

    private void ShowWinner(BoardState boardState)
    {
        boardManager.canPlay = false;
        mainUI.SetActive(true);
        Player player = boardManager.humanPlayer;

        if ((int) boardState == (int) player + 1)
        {
            winner.text = "You Won";
            return;
        }

        if (boardState == BoardState.Draw)
        {
            winner.text = "Draw";
        }
        else
        {
            winner.text = boardState + " Won";
        }

    }
}
