using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Uno;

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
}
