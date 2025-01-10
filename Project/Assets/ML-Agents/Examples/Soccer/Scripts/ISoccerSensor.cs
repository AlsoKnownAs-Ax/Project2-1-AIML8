using Unity.MLAgents.Sensors;

public interface ISoccerSensor
{
    void CollectObservations(VectorSensor sensor);
    void OnEpisodeBegin();
    bool IsActive();
}
