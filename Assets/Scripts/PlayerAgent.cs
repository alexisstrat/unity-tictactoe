using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    public BoardManager boardManager;
    public Player player;
    
    public override void OnActionReceived(float[] vectorAction)
    {
        boardManager.SelectSlot(Mathf.FloorToInt(vectorAction[0]), player);
    }

    public override void Heuristic(float[] actionsOut)
    {
        var availableSlots = (int[]) boardManager.GetAvailableSlots();
        int randomSlot = availableSlots[Random.Range(0, availableSlots.Length)];
        actionsOut[0] = randomSlot;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var slot in boardManager._slots)
        {
            sensor.AddObservation((int)slot.currentState);
        }
        sensor.AddObservation((int)player);
    }
    
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(0, boardManager.FilledSlots());
    }
}