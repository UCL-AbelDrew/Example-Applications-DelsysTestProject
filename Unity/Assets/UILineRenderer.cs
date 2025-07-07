using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI Line Renderer for Unity UI (Canvas).
/// Draws a polyline using UI vertex primitives.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UILineRenderer : MaskableGraphic
{
    [Tooltip("List of points (in local space) to draw the line through.")]
    public List<Vector2> Points = new List<Vector2>();

    [Tooltip("Thickness of the line in pixels.")]
    public float LineThickness = 1f;

    [Tooltip("Color of the line.")]
    public Color LineColor = Color.white;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (Points == null || Points.Count < 2)
            return;

        color = LineColor;

        for (int i = 0; i < Points.Count - 1; i++)
        {
            Vector2 start = Points[i];
            Vector2 end = Points[i + 1];

            Vector2 direction = (end - start).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (LineThickness / 2f);

            // Four corners of the quad
            Vector2 v0 = start + normal;
            Vector2 v1 = start - normal;
            Vector2 v2 = end - normal;
            Vector2 v3 = end + normal;

            int idx = vh.currentVertCount;

            vh.AddVert(v0, color, Vector2.zero);
            vh.AddVert(v1, color, Vector2.zero);
            vh.AddVert(v2, color, Vector2.zero);
            vh.AddVert(v3, color, Vector2.zero);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
    }

    /// <summary>
    /// Call this to update the line after changing Points.
    /// </summary>
    public void SetAllDirty()
    {
        base.SetAllDirty();
    }
}
