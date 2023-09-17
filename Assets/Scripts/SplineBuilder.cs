using System.Collections.Generic;
using System.Linq;
using FluffyUnderware.Curvy;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SplineBuilder : MonoBehaviour
{
    [FormerlySerializedAs("Image")] [SerializeField] private Image image;
    [FormerlySerializedAs("Spline")] [SerializeField] private CurvySpline spline;
    [FormerlySerializedAs("Interpolation")] [SerializeField] public CurvyInterpolation interpolation;
    [SerializeField] private float threshold = 0.5f;
    [SerializeField] private int pixelStep = 10;

    private Vector3[] splinePoints;

    [ContextMenu("Get Points")]
    private void GetPoints()
    {
        splinePoints = BuildSplinePoints(DetectEdgePixels())?.ToArray();
        if (splinePoints is null)
        {
            Debug.Log("Spline wasn't create");
            return;
        }
        if(spline)
            DestroyImmediate(spline.gameObject);
        spline = CurvySpline.Create();
        spline.transform.SetParent(transform);
        spline.transform.localPosition = Vector3.zero;
        spline.transform.localRotation = Quaternion.identity;

        spline.Interpolation = interpolation;
        spline.Add(splinePoints, Space.World);
        spline.Add(splinePoints.FirstOrDefault(), Space.World);
    }

    private List<Vector2> DetectEdgePixels()
    {
        int CompareVector2(Vector2 a, Vector2 b)
        {
            var centerTexture = new Vector2(image.sprite.texture.width / 2, image.sprite.texture.height / 2);
            Vector3 directionA = (a - centerTexture).normalized;
            Vector3 directionB = (b - centerTexture).normalized;
            var angleA = Mathf.Atan2(directionA.x, directionA.y); 
            var angleB = Mathf.Atan2(directionB.x, directionB.y);

            if (angleA < 0)
                angleA += 2 * Mathf.PI;

            if (angleB < 0)
                angleB += 2 * Mathf.PI;

            return angleA.CompareTo(angleB);
        }

        if (!image) 
            return null;
        
        var edgePoints = new List<Vector2>();

        for (var x = 0; x < image.sprite.texture.width; x += pixelStep)
        {
            for (var y = 0; y < image.sprite.texture.height; y++)
                if (TryAddPixel(x, y, edgePoints))
                    break;

            for (var y = image.sprite.texture.height - 1; y >= 0; y--)
                if (TryAddPixel(x, y, edgePoints))
                    break;
        }

        for (var y = 0; y < image.sprite.texture.height; y += pixelStep)
        {
            for (var x = 0; x < image.sprite.texture.width; x++)
                if (TryAddPixel(x, y, edgePoints))
                    break;

            for (var x = image.sprite.texture.width - 1; x >= 0; x--)
                if (TryAddPixel(x, y, edgePoints))
                    break;
        }

        edgePoints.Sort(CompareVector2);
        foreach (var edgePoint in edgePoints)
            Debug.Log(edgePoint);

        return edgePoints;
    }

    private bool TryAddPixel(int x, int y, List<Vector2> edgePoints)
    {
        if (!image)
            return false;  
        
        var pixelColor = image.sprite.texture.GetPixel(x, y);

        if (!(pixelColor.a > threshold))
            return false;
        var newPoint = new Vector2(x, y);
        if (!edgePoints.Contains(newPoint))
            edgePoints.Add(new Vector3(x, y));
        return true;
    }

    private List<Vector3> BuildSplinePoints(List<Vector2> edgePixels)
    {
        if (!image || !Camera.main)
            return null;
        
        var rectTransform = image.GetComponent<RectTransform>();
        var texture = image.sprite.texture;

        var imagePosition = RectTransformUtility.WorldToScreenPoint(Camera.main, rectTransform.position);
        var imageSize = rectTransform.rect.size;
        var textureSize = new Vector2(texture.width, texture.height);


        var results = new List<Vector3>();

        foreach (var pixel in edgePixels)
        {
            var normalizedX = pixel.x / textureSize.x;
            var normalizedY = pixel.y / textureSize.y;
            var imageX = (normalizedX - 0.5f) * imageSize.x;
            var imageY = (normalizedY - 0.5f) * imageSize.y;

            var point = new Vector2(imageX, imageY) + (Vector2)rectTransform.position + imagePosition;
            var imagePoint = new Vector3(point.x, point.y, 0);
            var worldPoint = Camera.main.ScreenToWorldPoint(imagePoint);
            worldPoint[2] = image.transform.position.z;
            
            results.Add(worldPoint);
        }

        return results;
    }
}