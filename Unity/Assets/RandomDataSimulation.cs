using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDataSimulation : MonoBehaviour
{
    [Header("Random Data Settings")]
    [Tooltip("Number of random values generated each frame.")]
    public int listSize = 8;

    [Tooltip("Minimum value for random generation.")]
    public double minValue = 0.0;

    [Tooltip("Maximum value for random generation.")]
    public double maxValue = 1.0;

    [Header("Target Script Reference")]
    [Tooltip("Script that will receive the generated data.")]
    public DataVisScrollview targetScript;

    [Tooltip("Temporarily boost values for testing in inspector")]
    public bool boost;

    public double boostMinValue = 80.0;
    public double boostMaxValue = 100.0;

    private System.Random random = new System.Random();

    void FixedUpdate()
    {
        var data = new List<double>(listSize);
        for (int i = 0; i < listSize; i++)
        {
            double value = 0;
            if (boost)
            {
                value = random.NextDouble() * (boostMaxValue - boostMinValue) + boostMinValue;
            }
            else
            {
                value = random.NextDouble() * (maxValue - minValue) + minValue;
            }
            data.Add(value);
        }

        if (targetScript != null)
        {
        
            targetScript.ReceiveData(data);
        }
    }
}