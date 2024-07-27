using Raylib_CsLo;
using System;
using System.Diagnostics;
using System.Numerics;
using Rectangle = Raylib_CsLo.Rectangle;


namespace Application;

internal class MatchScreen
{
    public UnoMatch match;
    public List<UnoCard> deck;
    public Font font;
    public Shader outlineShader;
    Texture blockTexture;
    Texture reverseTexture;

    // Outline shader variable locations
    int outlineSizeLoc;
    int outlineColorLoc;
    int textureSizeLoc;

    public MatchScreen()
    {
        match = new UnoMatch();
        deck = GenerateDeck();
        font = Raylib.LoadFontEx("assets/Cabin-BoldItalic.ttf", 256, 95);
        outlineShader = Raylib.LoadShader(null, "assets/outline.glsl");
        blockTexture = Raylib.LoadTexture("assets/block.png");
        reverseTexture = Raylib.LoadTexture("assets/reverse.png");

        Raylib.SetTextureWrap(font.texture, TextureWrap.TEXTURE_WRAP_CLAMP);

        outlineSizeLoc = Raylib.GetShaderLocation(outlineShader, "outlineSize");
        outlineColorLoc = Raylib.GetShaderLocation(outlineShader, "outlineColor");
        textureSizeLoc = Raylib.GetShaderLocation(outlineShader, "textureSize");
    }

    public List<UnoCard> GenerateDeck()
    {
        var deck = new List<UnoCard>();

        var colors = new UnoCardColor[]
        {
            UnoCardColor.Blue,
            UnoCardColor.Green,
            UnoCardColor.Red,
            UnoCardColor.Yellow
        };

        foreach (var color in colors)
        {
            for (int i = 0; i < 10; i++)
            {
                deck.Add(new UnoCard
                {
                    color = color,
                    type = UnoCardType.Number,
                    number = i
                });
            }

            deck.Add(new UnoCard
            {
                color = color,
                type = UnoCardType.Block
            });

            deck.Add(new UnoCard
            {
                color = color,
                type = UnoCardType.Reverse
            });

            deck.Add(new UnoCard
            {
                color = color,
                type = UnoCardType.PlusTwo
            });
        }

        for (int i = 0; i < 2; i++)
        {
            deck.Add(new UnoCard
            {
                color = UnoCardColor.Special,
                type = UnoCardType.PlusFour
            });
            deck.Add(new UnoCard
            {
                color = UnoCardColor.Special,
                type = UnoCardType.ChangeColor
            });
        }

        return deck;
    }

    static Color ColorFromUnoColor(UnoCardColor color)
    {
        switch (color)
        {
            case UnoCardColor.Red:
                return Raylib.RED;
            case UnoCardColor.Blue:
                return Raylib.BLUE;
            case UnoCardColor.Green:
                return Raylib.GREEN;
            case UnoCardColor.Yellow:
                return Raylib.YELLOW;
            case UnoCardColor.Special:
                return Raylib.BLACK;
            default:
                throw new Exception("Invalid color");
        }
    }

    public void DrawCard(UnoCard card, Rectangle rect)
    {
        var center = Utils.RectCenter(rect);
        var shortEdge = Math.Min(rect.width, rect.height);
        var roundness = 0.15f;
        Raylib.DrawRectangleRounded(rect, roundness, 4, Raylib.WHITE);

        var cardColor = ColorFromUnoColor(card.color);

        Raylib.DrawRectangleRounded(rect, (float)roundness, 4, Raylib.WHITE);
        Raylib.DrawRectangleRounded(Utils.ShrinkRect(rect, shortEdge*0.05f), (float)roundness, 4, cardColor);

        var centerEllipseWidth = rect.width * 0.38f;
        var centerEllipseHeight = rect.height * 0.44f;
        var centerEllipseAngle = 25f;

        RlGl.rlPushMatrix();
        {
            
            RlGl.rlTranslatef(center.X, center.Y, 0);
            RlGl.rlRotatef(centerEllipseAngle, 0, 0, 1);
            Raylib.DrawEllipse(0, 0, centerEllipseWidth, centerEllipseHeight, Raylib.WHITE);
        }
        RlGl.rlPopMatrix();

        var fontSize = shortEdge*1.1f;
        var outlineSize = 0.06f * fontSize / 256;

        if (card.type == UnoCardType.Number)
        {
            var text = $"{card.number}";

            Utils.DrawTextCentered(font, text, center + new Vector2(5, 5), fontSize, 0, Raylib.BLACK);
            DrawTextOutlined(font, text, center, fontSize, cardColor, Raylib.BLACK, outlineSize);

            var offsetToCorner = new Vector2(rect.width * 0.36f, rect.height * 0.39f);

            DrawTextOutlined(font, text, center - offsetToCorner, fontSize * 0.2f, Raylib.WHITE, Raylib.BLACK, outlineSize * 0.6f);

            RlGl.rlPushMatrix();
            {
                RlGl.rlTranslatef(center.X + offsetToCorner.X, center.Y + offsetToCorner.Y, 0);
                RlGl.rlRotatef(180, 0, 0, 1);
                DrawTextOutlined(font, text, new Vector2(0, 0), fontSize * 0.2f, Raylib.WHITE, Raylib.BLACK, outlineSize * 0.6f);
            }
            RlGl.rlPopMatrix();
        } else if (card.type == UnoCardType.PlusTwo || card.type == UnoCardType.PlusFour)
        {
            var text = card.type == UnoCardType.PlusTwo ? "+2" : "+4";

            var offsetToCorner = new Vector2(rect.width * 0.34f, rect.height * 0.39f);

            DrawTextOutlined(font, text, center - offsetToCorner, fontSize * 0.2f, Raylib.WHITE, Raylib.BLACK, outlineSize * 0.6f);

            RlGl.rlPushMatrix();
            {
                RlGl.rlTranslatef(center.X + offsetToCorner.X, center.Y + offsetToCorner.Y, 0);
                RlGl.rlRotatef(180, 0, 0, 1);
                DrawTextOutlined(font, text, new Vector2(0, 0), fontSize * 0.2f, Raylib.WHITE, Raylib.BLACK, outlineSize * 0.6f);
            }
            RlGl.rlPopMatrix();
        }

        if (card.type == UnoCardType.ChangeColor)
        {
            DrawChangeColorIcon(center, centerEllipseWidth, centerEllipseHeight, centerEllipseAngle);

            var offsetToCorner = new Vector2(rect.width * 0.35f, rect.height * 0.38f);
            var cornerScale = 0.15f;

            DrawChangeColorIcon(center - offsetToCorner, centerEllipseWidth * cornerScale, centerEllipseHeight * cornerScale, centerEllipseAngle);

            RlGl.rlPushMatrix();
            {
                RlGl.rlTranslatef(center.X + offsetToCorner.X, center.Y + offsetToCorner.Y, 0);
                RlGl.rlRotatef(180, 0, 0, 1);

                DrawChangeColorIcon(Vector2.Zero, centerEllipseWidth * cornerScale, centerEllipseHeight * cornerScale, centerEllipseAngle);
            }
            RlGl.rlPopMatrix();
        }

        if (card.type == UnoCardType.Block || card.type == UnoCardType.Reverse)
        {
            var texture = card.type == UnoCardType.Reverse ? reverseTexture : blockTexture;

            DrawTextureCentered(texture, center, shortEdge * 0.65f, cardColor);

            var offsetToCorner = new Vector2(rect.width * 0.35f, rect.height * 0.38f);
            var cornerScale = 0.15f;

            DrawTextureCentered(texture, center - offsetToCorner, shortEdge * cornerScale, Raylib.WHITE);

            RlGl.rlPushMatrix();
            {
                RlGl.rlTranslatef(center.X + offsetToCorner.X, center.Y + offsetToCorner.Y, 0);
                RlGl.rlRotatef(180, 0, 0, 1);
                DrawTextureCentered(texture, Vector2.Zero, shortEdge * cornerScale, Raylib.WHITE);
            }
            RlGl.rlPopMatrix();
        }

        if (card.type == UnoCardType.PlusTwo)
        {
            var iconCardSize = new Vector2(rect.width, rect.height) * 0.35f;
            var position = Utils.GetCenteredPosition(rect, iconCardSize);

            var offset = shortEdge * 0.10f;

            DrawIconCardStack(
                [
                    new Rectangle(position.X + offset, position.Y - offset, iconCardSize.X, iconCardSize.Y),
                    new Rectangle(position.X - offset, position.Y + offset, iconCardSize.X, iconCardSize.Y)
                ],
                roundness,
                [cardColor, cardColor]
            );
        }

        if (card.type == UnoCardType.PlusFour)
        {
            var iconCardSize = new Vector2(rect.width, rect.height) * 0.29f;
            var position = Utils.GetCenteredPosition(rect, iconCardSize);

            var offset = shortEdge * 0.18f;

            DrawIconCardStack(
                [
                    new Rectangle(position.X - offset*0.2f, position.Y + offset     , iconCardSize.X, iconCardSize.Y),
                    new Rectangle(position.X + offset     , position.Y - offset*0.2f, iconCardSize.X, iconCardSize.Y),
                    new Rectangle(position.X + offset*0.2f, position.Y - offset     , iconCardSize.X, iconCardSize.Y),
                    new Rectangle(position.X - offset     , position.Y + offset*0.2f, iconCardSize.X, iconCardSize.Y)
                ],
                roundness,
                [
                    ColorFromUnoColor(UnoCardColor.Yellow),
                    ColorFromUnoColor(UnoCardColor.Green),
                    ColorFromUnoColor(UnoCardColor.Blue),
                    ColorFromUnoColor(UnoCardColor.Red),
                ]
            );
        }
    }

    static float iconCardShear = -0.35f;

    static void DrawIconCardStack(Rectangle[] rects, float roundness, Color[] colors)
    {
        Debug.Assert(rects.Length == colors.Length);

        for (int i = 0; i < rects.Length; i++)
        {
            DrawIconCardShadow(rects[i], roundness);
        }

        for (int i = 0; i < rects.Length; i++)
        {
            DrawIconCard(rects[i], roundness, colors[i]);
        }
    }

    static void DrawIconCard(Rectangle rect, float roundness, Color color)
    {
        DrawRectangleShearedRounded(rect, roundness, 4, iconCardShear, Raylib.BLACK);
        DrawRectangleShearedRounded(Utils.ShrinkRect(rect, rect.width * 0.04f), roundness, 4, iconCardShear, Raylib.WHITE);
        DrawRectangleShearedRounded(Utils.ShrinkRect(rect, rect.width * 0.15f), roundness, 4, iconCardShear, color);
    }

    static void DrawIconCardShadow(Rectangle rect, float roundness)
    {
        DrawRectangleShearedRounded(new Rectangle(rect.X + rect.width*0.05f, rect.Y + rect.height * 0.05f, rect.width, rect.height), roundness, 4, iconCardShear, Raylib.BLACK);
    }

    static void DrawRectangleShearedRounded(Rectangle rect, float roundness, int segments, float shearX, Color tint)
    {
        RlGl.rlPushMatrix();
        {
            ApplyShearX(shearX);
            RlGl.rlTranslatef(-rect.y * shearX, 0, 0);
            RlGl.rlTranslatef(-rect.height * shearX * 0.5f, 0, 0);

            Raylib.DrawRectangleRounded(rect, roundness, segments, tint);
        }
        RlGl.rlPopMatrix();
    }

    static void ApplyShearX(float amount)
    {
        var transform = Matrix4x4.Identity;
        transform.M21 = amount;

        unsafe
        {
            var matrixf16 = float16.FromMatrix(transform);
            RlGl.rlMultMatrixf(matrixf16.v);
        }
    }

    static void DrawTextureCentered(Texture texture, Vector2 center, float size, Color tint)
    {
        var scale = size / texture.width;
        Raylib.DrawTextureEx(texture, center - new Vector2(texture.width, texture.height) / 2 * scale, 0, scale, tint);
    }

    static void DrawChangeColorIcon(Vector2 center, float width, float height, float tilt)
    {
        var segmentColors = new UnoCardColor[] { UnoCardColor.Blue, UnoCardColor.Green, UnoCardColor.Yellow, UnoCardColor.Red };

        RlGl.rlPushMatrix();
        {
            RlGl.rlTranslatef(center.X, center.Y, 0);
            RlGl.rlRotatef(tilt, 0, 0, 1);
            
            Raylib.DrawEllipse(0, 0, width, height, Raylib.WHITE);

            for (int i = 0; i < 4; i++)
            {
                var points = GenEllipseFan(Math.PI / 2 * i, Math.PI / 2 * (i + 1), width * 0.9f, height * 0.9f, 8);

                unsafe
                {
                    fixed (Vector2* pointsPtr = points)
                    {
                        Raylib.DrawTriangleFan(pointsPtr, points.Length, ColorFromUnoColor(segmentColors[i]));
                    }
                }
            }
        }
        RlGl.rlPopMatrix();
    }

    static Vector2[] GenEllipseFan(double fromAngle, double toAngle, float width, float height, int segments)
    {
        var points = new List<Vector2>();
        points.Add(new Vector2(0, 0));

        var angleStep = (toAngle - fromAngle) / segments;
        for (int i = 0; i <= segments; i++)
        {
            var angle = -i * angleStep + fromAngle;
            points.Add(new Vector2((float)Math.Cos(angle) * width, (float)Math.Sin(angle) * height));
        }

        return points.ToArray();
    }

    public void DrawTextOutlined(Font font, string text, Vector2 center, float fontSize, Color tint, Color outline, float outlineSize)
    {
        var outlineVec4 = new Vector4((float)outline.r / 255, (float)outline.g / 255, (float)outline.b / 255, (float)outline.a / 255);

        Raylib.SetShaderValue(outlineShader, outlineColorLoc, outlineVec4, ShaderUniformDataType.SHADER_UNIFORM_VEC4);
        Raylib.SetShaderValue(outlineShader, textureSizeLoc, Raylib.MeasureTextEx(font, text, fontSize, 0), ShaderUniformDataType.SHADER_UNIFORM_VEC2);
        Raylib.SetShaderValue(outlineShader, outlineSizeLoc, outlineSize, ShaderUniformDataType.SHADER_UNIFORM_FLOAT);

        Raylib.BeginShaderMode(outlineShader);
            Utils.DrawTextCentered(font, text, center, fontSize, 0, Raylib.WHITE);
        Raylib.EndShaderMode();

        Utils.DrawTextCentered(font, text, center, fontSize, 0, tint);
    }

    public void Tick(float dt)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.LIGHTGRAY);

        var windowRect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        var cardWidth = 120f;
        var cardRatio = 400f / 250f;
        var cardSize = new Vector2(cardWidth, cardRatio * cardWidth);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 13; j++)
            {
                var index = i * 13 + j;
                if (index >= deck.Count) break;

                DrawCard(deck[index], new Rectangle(cardSize.X * j, cardSize.Y * i, cardSize.X, cardSize.Y));
            }
        }

        Raylib.EndDrawing();
    }
}
