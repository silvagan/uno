﻿using Raylib_CsLo;
using System.Numerics;

namespace Application;

internal class VerticalStack
{
    public float gap;
    public Vector2 position;

    public Vector2 nextPosition(float height)
    {
        var result = position;

        position.Y += height;
        position.Y += gap;

        return result;
    }

    public Rectangle nextRectangle(float width, float height)
    {
        var pos = nextPosition(height);
        return new Rectangle(pos.X, pos.Y, width, height);
    }
}