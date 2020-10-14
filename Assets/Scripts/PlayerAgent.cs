using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    public BoardManager boardManager;
    public Player player;
    
    /// <summary>
    /// The agent receives the action which will be an int
    /// from 0-9 depending on the available slots.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        boardManager.SelectSlot(Mathf.FloorToInt(vectorAction[0]), player);
    }

    /// <summary>
    /// Choose a random slot over the available ones
    /// </summary>
    public override void Heuristic(float[] actionsOut)
    {
        var availableSlots = (int[]) boardManager.GetAvailableSlots();
        int randomSlot = availableSlots[Random.Range(0, availableSlots.Length)];
        actionsOut[0] = randomSlot;
    }
    
    /// <summary>
    /// Add 10 observations to the agent.
    /// 9 for the state of each slot and 1 for which player he is.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var slot in boardManager._slots)
        {
            sensor.AddObservation((int)slot.currentState);
        }
        sensor.AddObservation((int)player);
    }
    
    /// <summary>
    /// Mask the observations with the filled slots, so
    /// it will not take them into account on action
    /// </summary>
    public override void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        actionMasker.SetMask(0, boardManager.FilledSlots());
    }
}