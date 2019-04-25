using System;
using UnityEngine;

public class Drawing
{
    public static Texture2D lineTex;

    public static void DrawLine(Rect rect)
    {
        DrawLine(rect, GUI.contentColor, 1f);
    }

    public static void DrawLine(Rect rect, float width)
    {
        DrawLine(rect, GUI.contentColor, width);
    }

    public static void DrawLine(Rect rect, Color color)
    {
        DrawLine(rect, color, 1f);
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB)
    {
        DrawLine(pointA, pointB, GUI.contentColor, 1f);
    }

    public static void DrawLine(Rect rect, Color color, float width)
    {
        DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width);
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB, float width)
    {
        DrawLine(pointA, pointB, GUI.contentColor, width);
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color)
    {
        DrawLine(pointA, pointB, color, 1f);
    }

    public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        Matrix4x4 matrix = GUI.matrix;
        if (lineTex == 0)
        {
            lineTex = new Texture2D(1, 1);
        }
        Color color2 = GUI.color;
        GUI.color = color;
        float angle = Vector3.Angle((Vector3) (pointB - pointA), (Vector3) Vector2.right);
        if (pointA.y > pointB.y)
        {
            angle = -angle;
        }
        Vector2 vector = pointB - pointA;
        GUIUtility.ScaleAroundPivot(new Vector2(vector.magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1f, 1f), lineTex);
        GUI.matrix = matrix;
        GUI.color = color2;
    }
}

