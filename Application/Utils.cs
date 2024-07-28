using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Application;

internal static class Utils
{
    public static Vector2 GetCenteredPosition(Rectangle container, Vector2 size)
    {
        return new Vector2(
            container.x + (container.width - size.X) / 2,
            container.y + (container.height - size.Y) / 2
        );
    }

    public static Rectangle GetCenteredRect(Rectangle container, Vector2 size)
    {
        var pos = GetCenteredPosition(container, size);
        return new Rectangle(pos.X, pos.Y, size.X, size.Y);
    }

    public static Rectangle ShrinkRect(Rectangle rect, float amount)
    {
        return new Rectangle(rect.X + amount, rect.Y + amount, rect.width - 2 * amount, rect.height - 2 * amount);
    }

    public static Vector2 RectCenter(Rectangle rect)
    {
        return new Vector2(rect.x + rect.width / 2, rect.y + rect.height / 2);
    }

    public static void CopyString(string from, sbyte[] to)
    {
        for (int i = 0; i < from.Length; i++)
        {
            to[i] = (sbyte)from[i];
        }
        to[from.Length] = 0;
    }

    public static string FromBytesToString(sbyte[] bytes)
    {
        var chars = new char[bytes.Length];
        var strLength = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            chars[i] = (char)bytes[i];
            if (bytes[i] == 0) break;
            strLength++;
        }

        return new string(chars, 0, strLength);
    }

    public static double ToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    public static double ToDegrees(double angle)
    {
        return angle / 180 * Math.PI;
    }

    public static void DrawTextCentered(Font font, string text, Vector2 position, float fontSize, float spacing, Color tint)
    {
        var textSize = Raylib.MeasureTextEx(font, text, fontSize, spacing);
        Raylib.DrawTextEx(font, text, position - textSize / 2, fontSize, spacing, tint);
    }

    public static void DrawTextCenteredOutlined(Font font, string text, Vector2 position, float fontSize, float spacing, Color tint, float outline, Color outlineColor)
    {
        var textSize = Raylib.MeasureTextEx(font, text, fontSize, spacing);
        var center = position - textSize / 2;

        Raylib.DrawTextEx(font, text, center + new Vector2(+outline, +outline), fontSize, spacing, outlineColor);
        Raylib.DrawTextEx(font, text, center + new Vector2(+outline, -outline), fontSize, spacing, outlineColor);
        Raylib.DrawTextEx(font, text, center + new Vector2(-outline, +outline), fontSize, spacing, outlineColor);
        Raylib.DrawTextEx(font, text, center + new Vector2(-outline, -outline), fontSize, spacing, outlineColor);

        Raylib.DrawTextEx(font, text, center, fontSize, spacing, tint);
    }
}
