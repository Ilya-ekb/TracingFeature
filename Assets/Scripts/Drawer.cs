using System;
using UnityEngine;
using UnityEngine.UI;

public class Drawer : MonoBehaviour
{
    public Image image; // The texture to draw on
    public int brushSize = 10; // Size of the brush

    private bool isDrawing = false;
    private Vector2 lastPosition;

    private Texture2D texture;

    private void Start()
    {
    }

    private void Update()
    {
        // Check if the left mouse button is being held down
        if (Input.GetMouseButton(0))
        {
            if (!isDrawing)
            {
                isDrawing = true;
                lastPosition = Input.mousePosition;
            }

            // Calculate the change in position
            Vector2 currentPosition = Input.mousePosition;
            Vector2 deltaPosition = currentPosition - lastPosition;

            // Draw on the texture
            DrawOnTexture(currentPosition, deltaPosition);

            lastPosition = currentPosition;
        }
        else
        {
            isDrawing = false;
        }
    }

    private void DrawOnTexture(Vector2 position, Vector2 delta)
    {
        // Get the UV coordinates of the position on the texture
        Vector2 uv = new Vector2(position.x / Screen.width, position.y / Screen.height);

        // Convert the UV coordinates to pixel coordinates
        int x = (int)(uv.x * texture.width);
        int y = (int)(uv.y * texture.height);

        // Draw a brush-sized circle around the pixel coordinates
        for (int i = -brushSize; i <= brushSize; i++)
        {
            for (int j = -brushSize; j <= brushSize; j++)
            {
                int brushX = x + i;
                int brushY = y + j;

                if (brushX >= 0 && brushX < texture.width && brushY >= 0 && brushY < texture.height)
                {
                    texture.SetPixel(brushX, brushY, Color.black);
                }
            }
        }

        // Apply the changes to the texture
        texture.Apply();
    }
}