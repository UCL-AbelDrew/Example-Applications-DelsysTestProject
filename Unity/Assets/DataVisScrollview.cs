using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataVisScrollview : MonoBehaviour
{
    [Header("Sensor Friendly Name UI Component")]
    public TextMeshProUGUI sensorNameText;


    [Header("Graph Time Window")]
    [Tooltip("Frames per second (set to your data update rate).")]
    public int frameRate = 60;
    [Tooltip("How many seconds of data to display.")]
    public float secondsToDisplay = 10f;

    public enum StatisticType
    {
        Mean,
        Median,
        Mode
    }

    [Header("Statistic Calculation")]
    [Tooltip("Select which statistic to plot.")]
    public StatisticType statisticType = StatisticType.Mean;

    [Header("Graph & Viewport References")]
    public RectTransform graphContent; // The content area of the graph
    public RectTransform viewport;     // The visible area (should have a Mask or RectMask2D)
    public UILineRenderer uiLineRenderer; // Reference to your UILineRenderer component

    [Header("Graph Settings")]
    [Tooltip("The space between frame updates on the graph, to increase scroll speed increase spacing.")]
    [Range(2f, 10f)]
    public float xSpacing = 40f;       // Space between points
    private float yMin = 0f;            // Minimum value for Y axis
    private float yMax = 1f;            // Maximum value for Y axis
                                        // Remove or make this private, as it will be set dynamically
                                        // public float graphHeight = 200f;

    [Header("Line Renderer Layout")]
    [Tooltip("Vertical padding to subtract from the parent's height.")]
    public float lineRendererVerticalPadding = 20f;

    public GridLayoutGroup layoutGroup; // Reference to the GridLayoutGroup, if there's no layout group the line renderer will resize 
                                        // to the immediate parent's width and height minus the padding.

    //Set the sensor name in the UI Text component.
    public void SetSensorName(string newName)
    {
        if (sensorNameText != null)
        {
            sensorNameText.text = newName;
        }
    }

    private void UpdateUILineRendererRect()
    {
        if (uiLineRenderer == null || uiLineRenderer.rectTransform == null)
            return;

        RectTransform parent = uiLineRenderer.rectTransform.parent as RectTransform;

        if (parent == null)
            return;

        if (layoutGroup != null)
        {
            // If there's a layout group, set the size to the layout group's cell size
            uiLineRenderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layoutGroup.cellSize.x);
            uiLineRenderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutGroup.cellSize.y - lineRendererVerticalPadding);

            //Offset so the top of the rect is at the center of the parent cell when using a layout group
            float yOffsetLayout = (layoutGroup.cellSize.y * 0.5f) - ((layoutGroup.cellSize.y - lineRendererVerticalPadding) * 0.5f);
            uiLineRenderer.rectTransform.anchoredPosition = new Vector2(0, -yOffsetLayout);

            return;
        }
        
        // Set width to parent's width
        float parentWidth = parent.rect.width;
        // Set height to parent's height minus padding
        float parentHeight = parent.rect.height;
        float newHeight = Mathf.Max(0, parentHeight - lineRendererVerticalPadding);

        uiLineRenderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth);
        uiLineRenderer.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

        // Offset so the top of the rect is at the center of the parent
        float yOffset = (parentHeight * 0.5f) - (newHeight * 0.5f);
        uiLineRenderer.rectTransform.anchoredPosition = new Vector2(0, -yOffset);
    }

    [Header("Input Data Normalization")]
    public float inputMin = 0f;        // Minimum possible value of input data
    public float inputMax = 1000f;        // Maximum possible value of input data

    [Header("UI Controls")]
    public Slider scrollSpeedSlider;   // Reference to the UI slider

    private List<float> plotValues = new List<float>();
    private float GraphHeight
    {
        get
        {
            if (uiLineRenderer != null && uiLineRenderer.rectTransform != null)
                return uiLineRenderer.rectTransform.rect.height;
            return 200f;
        }
    }

    void Start()
    {
        //Get a layout group in the parent hierarchy if it exists
        layoutGroup = GetComponentInParent<GridLayoutGroup>();
        UpdateUILineRendererRect();
    }

    /// <summary>
    /// Called when the slider value changes.
    /// </summary>
    /// <param name="value">New xSpacing value from the slider.</param>
    public void OnScrollSpeedChanged(float value)
    {
        xSpacing = value;
        RedrawGraph();
    }

    /// <summary>
    /// Redraws the graph with the current xSpacing and plotValues.
    /// </summary>
    private void RedrawGraph()
    {
        if (graphContent == null || uiLineRenderer == null)
            return;

        // Update content width
        float requiredWidth = (plotValues.Count) * xSpacing;
        if (graphContent.sizeDelta.x < requiredWidth)
            graphContent.sizeDelta = new Vector2(requiredWidth, graphContent.sizeDelta.y);

        // Update UILineRenderer points
        var points = new List<Vector2>();
        for (int i = 0; i < plotValues.Count; i++)
        {
            float x = i * xSpacing;
            float y = NormalizeToGraph(plotValues[i]);
            points.Add(new Vector2(x, y));
        }
        uiLineRenderer.Points = points;
        uiLineRenderer.SetAllDirty();

        CenterContentRightEdgeInViewport();
    }

    /// <summary>
    /// Call this method to add a new data point to the graph.
    /// </summary>
    public void ReceiveData(List<double> data)
    {
        if (data == null || data.Count == 0)
            return;

        double value = 0;
        switch (statisticType)
        {
            case StatisticType.Mean:
                value = data.Average();
                break;
            case StatisticType.Median:
                value = GetMedian(data);
                break;
            case StatisticType.Mode:
                value = GetMode(data);
                break;
        }

        AddPlotPoint((float)value);
        CenterContentRightEdgeInViewport();
    }

    private void AddPlotPoint(float value)
    {
        if (graphContent == null || uiLineRenderer == null)
            return;

        // Clamp value to inputMin/inputMax
        value = Mathf.Clamp(value, inputMin, inputMax);

        // Add the new value to the plot values, before normalising.
        plotValues.Add(value);

        // Limit to last N points (N = frameRate * secondsToDisplay)
        int maxPoints = Mathf.RoundToInt(frameRate * secondsToDisplay);
        if (plotValues.Count > maxPoints)
            plotValues.RemoveRange(0, plotValues.Count - maxPoints);
  


        // Expand content width if needed
        float requiredWidth = (plotValues.Count) * xSpacing;
        if (graphContent.sizeDelta.x < requiredWidth)
            graphContent.sizeDelta = new Vector2(requiredWidth, graphContent.sizeDelta.y);
         
        
        // Update UILineRenderer points
        var points = new List<Vector2>();
        
        for (int i = 0; i < plotValues.Count; i++)
        {
            float x = i * xSpacing;
            float y = NormalizeToGraph(plotValues[i]);
            points.Add(new Vector2(x, y));
        }
        




        uiLineRenderer.Points = points;
        // Mark the UILineRenderer as dirty to redraw
        uiLineRenderer.SetAllDirty();
    }

    // Normalizes a value from inputMin/inputMax to the graph's yMin/yMax and height
    private float NormalizeToGraph(float value)
    {
        if (Mathf.Approximately(inputMax, inputMin))
            return 0f; // Avoid division by zero

        float t = (value - inputMin) / (inputMax - inputMin);
        float y = Mathf.Lerp(yMin, GraphHeight, t);
        return y;
    }

    /// <summary>
    /// Shifts the graphContent so that its right edge is always centered in the viewport.
    /// </summary>
    private void CenterContentRightEdgeInViewport()
    {
        if (viewport == null || graphContent == null || plotValues.Count == 0)
            return;

        float contentWidth = graphContent.sizeDelta.x;
        float viewportWidth = viewport.rect.width;

        Vector2 anchoredPos = graphContent.anchoredPosition;

        if (contentWidth <= viewportWidth)
        {
            // Center content if it's smaller than the viewport
            anchoredPos.x = (viewportWidth - contentWidth) * 0.5f;
        }
        else
        {
            // Center the right edge of the content in the viewport
            anchoredPos.x = viewportWidth * 0.5f - contentWidth;
        }

        graphContent.anchoredPosition = anchoredPos;
    }

    private double GetMedian(List<double> data)
    {
        var sorted = data.OrderBy(n => n).ToList();
        int count = sorted.Count;
        if (count % 2 == 1)
            return sorted[count / 2];
        else
            return (sorted[(count / 2) - 1] + sorted[count / 2]) / 2.0;
    }

    private double GetMode(List<double> data)
    {
        return data.GroupBy(n => n)
                   .OrderByDescending(g => g.Count())
                   .ThenBy(g => g.Key)
                   .First().Key;
    }
}
