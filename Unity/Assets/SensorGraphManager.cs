using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DelsysAPI.Pipelines;
using DelsysAPI.Components.TrignoRf;

public class SensorGraphManager : MonoBehaviour
{
    [Header("References")]
    public GameObject graphPrefab; // Assign your graph prefab in the Inspector
    public GridLayoutGroup layoutGroup; // Assign your layout group in the Inspector
    private DataVisScrollview[] graphs; // Array to hold all graph instances

    /// <summary>
    /// Call this after sensors are selected to create graphs.
    /// </summary>
    public void CreateGraphsForSensors(List<SensorTrignoRf> components) 
    {
        // Clear old graphs
        foreach (Transform child in layoutGroup.transform)
            Destroy(child.gameObject);
            graphs = null; // Reset the graphs array

        // Create new graphs for each sensor and its channels
        foreach (var comp in components)
        {
            // Instantiate a new graph for each sensor channel
            for (int i = 0; i < comp.TrignoChannels.Count; i++)
            {
                // Create a new graph object
                GameObject graphObj = Instantiate(graphPrefab, layoutGroup.transform);
                DataVisScrollview graph = graphObj.GetComponent<DataVisScrollview>();
                if (graph != null)
                {
                    // Set the sensor name and channel number
                    graph.SetSensorName($"{comp.FriendlyName} - Channel {i + 1}");
                }
            }
           
        }
        // Get all DataVisScrollview components in the layout group to update later
        graphs = layoutGroup.GetComponentsInChildren<DataVisScrollview>();
    }
    /// <summary>
    /// Updates each graph with the corresponding frame data.
    /// </summary>
    /// <param name="data">A list of channel data for the current frame.</param>
    public void UpdateGraphsWithFrameData(List<List<double>> data)
    {    

        int count = Mathf.Min(graphs.Length, data.Count);
        for (int i = 0; i < count; i++)
        {
            graphs[i].ReceiveData(data[i]);
        }
    }

}
