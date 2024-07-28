using Microsoft.Toolkit.HighPerformance;
using Raylib_CsLo;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Numerics;
using Rectangle = Raylib_CsLo.Rectangle;


namespace Application;

class HeldUnoCard {
    public UnoCard card;
    public Vector2 position;
    public Vector2 velocity;

    public Rectangle GetRect(Vector2 size)
    {
        return new Rectangle(position.X - size.X / 2, position.Y - size.Y / 2, size.X, size.Y);
    }
}

internal class MatchScreen
{
    public UnoClient net;

    public UnoMatch match;

    public List<UnoCard> deck;
    public Font font;
    public Shader outlineShader;
    Texture blockTexture;
    Texture reverseTexture;

    static float cardWidth = 120f;
    static Vector2 cardSize = new Vector2(cardWidth, (400f / 250f) * cardWidth);

    // Outline shader variable locations
    int outlineSizeLoc;
    int outlineColorLoc;
    int textureSizeLoc;

    List<HeldUnoCard> heldCards = new List<HeldUnoCard>();
    HeldUnoCard? grabbedCard = null;

    Vector2 deckPosition = new Vector2(20, 20);

    List<Tuple<UnoPlayer, UnoCard>> cardDrawQueue = new List<Tuple<UnoPlayer, UnoCard>>();
    float cardDrawInterval = 0.2f;
    DateTime lastCardDrawAt = DateTime.Now;

    public MatchScreen(UnoClient net)
    {
        this.net = net;

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

    static float cardRoundness = 0.15f;

    public void DrawCard(UnoCard card, Rectangle rect)
    {
        var center = Utils.RectCenter(rect);
        var shortEdge = Math.Min(rect.width, rect.height);
        Raylib.DrawRectangleRounded(rect, cardRoundness, 4, Raylib.WHITE);

        var cardColor = ColorFromUnoColor(card.color);

        Raylib.DrawRectangleRounded(rect, cardRoundness, 4, Raylib.WHITE);
        Raylib.DrawRectangleRounded(Utils.ShrinkRect(rect, shortEdge*0.05f), cardRoundness, 4, cardColor);

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
                cardRoundness,
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
                cardRoundness,
                [
                    ColorFromUnoColor(UnoCardColor.Yellow),
                    ColorFromUnoColor(UnoCardColor.Green),
                    ColorFromUnoColor(UnoCardColor.Blue),
                    ColorFromUnoColor(UnoCardColor.Red),
                ]
            );
        }
    }

    public void DrawCardBackSide(Rectangle rect)
    {
        var center = Utils.RectCenter(rect);
        var shortEdge = Math.Min(rect.width, rect.height);
        Raylib.DrawRectangleRounded(rect, cardRoundness, 4, Raylib.WHITE);

        Raylib.DrawRectangleRounded(rect, cardRoundness, 4, Raylib.WHITE);
        Raylib.DrawRectangleRounded(Utils.ShrinkRect(rect, shortEdge * 0.05f), cardRoundness, 4, ColorFromUnoColor(UnoCardColor.Special));

        var centerEllipseWidth = rect.width * 0.38f;
        var centerEllipseHeight = rect.height * 0.44f;
        var centerEllipseAngle = 25f;

        RlGl.rlPushMatrix();
        {
            RlGl.rlTranslatef(center.X, center.Y, 0);
            RlGl.rlRotatef(centerEllipseAngle, 0, 0, 1);
            Raylib.DrawEllipse(0, 0, centerEllipseWidth, centerEllipseHeight, ColorFromUnoColor(UnoCardColor.Red));
        }
        RlGl.rlPopMatrix();

        RlGl.rlPushMatrix();
        {
            RlGl.rlTranslatef(center.X, center.Y, 0);
            RlGl.rlRotatef(-centerEllipseAngle, 0, 0, 1);
            Utils.DrawTextCenteredOutlined(font, "UNO", Vector2.Zero, shortEdge * 0.5f, 0, ColorFromUnoColor(UnoCardColor.Yellow), shortEdge * 0.015f, ColorFromUnoColor(UnoCardColor.Special));
        }
        RlGl.rlPopMatrix();
    }

    public void DrawCardShadow(Rectangle rect)
    {
        var roundness = 0.15f;
        var offset = 10f;
        Raylib.DrawRectangleRounded(new Rectangle(rect.X + offset, rect.Y + offset, rect.width, rect.height), roundness, 4, Raylib.ColorAlpha(Raylib.BLACK, 0.8f));
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

    public Vector2 ClosestPoinOnSegment(Vector2 from, Vector2 segmentA, Vector2 segmentB)
    {
        var ab = segmentB - segmentA;
        var ap = from - segmentA;

        var proj = Vector2.Dot(ap, ab);
        var d = proj / ab.LengthSquared();

        return segmentA + ab * Math.Clamp(d, 0, 1);
    }


    public bool isMyPlayer(UnoPlayer player)
    {
        return player.id == net.ClientId;
    }

    public void PlayCard(UnoPlayer player, UnoCard card)
    {
        if (match.topCard == null) return;

        var playerIndex = match.players.IndexOf(player);
        if (match.currentPlayer != playerIndex) return;

        if (!UnoCard.CanCardBePlayed(match.topCard, card)) return;

        match.topCard = card;


        if (isMyPlayer(player))
        {
            player.hand.Remove(card);
            foreach (var heldCard in heldCards)
            {
                if (heldCard.card == card)
                {
                    heldCards.Remove(heldCard);
                    break;
                }
            } 
        } else
        {
            player.hand.RemoveAt(0);
        }

        if (match.isDirectionClockwise)
        {
            match.currentPlayer = (match.currentPlayer + 1) % (uint)match.players.Count;
        }
        else
        {
            match.currentPlayer = (match.currentPlayer - 1) % (uint)match.players.Count;
        }
    }

    public void DrawCardFromDeck(UnoPlayer player, UnoCard card)
    {
        player.hand.Add(card);

        if (isMyPlayer(player))
        {
            heldCards.Add(new HeldUnoCard
            {
                card = card,
                position = deckPosition
            });
        }
    }

    public void OnStart(string playerName)
    {
        //net.ClientId = 0;
        //match.players.Add(new UnoPlayer("Petras"));
        //match.players.Add(new UnoPlayer("Jonas"));
        //match.players[1].id = 2;

        //match.players.Add(myUnoPlayer);

        //match.players.Add(new UnoPlayer("Jonas"));
        //match.players.Add(new UnoPlayer("Ona"));

        // TODO: Temporary
        //match.currentPlayer = 1;
        //match.players[0].isReady = true;
        //match.players[1].isReady = true;
        //match.players[3].isReady = true;
    }

    public void OnMatchStarted()
    {
        var rng = new Random();

        foreach (var player in match.players)
        {
            for (int i = 0; i < 7; i++)
            {
                cardDrawQueue.Add(Tuple.Create(player, deck[rng.Next(0, deck.Count)]));
            }
        }

        do
        {
            match.topCard = deck[rng.Next(0, deck.Count)];
        } while (match.topCard.type != UnoCardType.Number);
    }

    public void DrawPlayerInfo(Vector2 pos, UnoPlayer player)
    {
        var panelRect = new Rectangle(pos.X, pos.Y, 160, 80);
        Raylib.DrawRectangleRec(panelRect, Raylib.LIGHTGRAY);
        Raylib.DrawRectangleLinesEx(panelRect, 2, Raylib.BLACK);

        RayGui.GuiLabel(new Rectangle(panelRect.X + 10, panelRect.Y + 10, panelRect.width - 20, 30), player.name);
        RayGui.GuiLabel(new Rectangle(panelRect.X + 10, panelRect.Y + 10 + 30, panelRect.width - 20, 30), $"{player.hand.Count} cards");

        if (match.started)
        {
            var playerIndex = match.players.IndexOf(player);
            if (playerIndex == match.currentPlayer)
            {
                Raylib.DrawCircleV(new Vector2(panelRect.X + panelRect.width - 20, panelRect.Y + 20), 10, Raylib.RED);
            }
        }

        if (!match.started && player.isReady)
        {
            Raylib.DrawCircleV(new Vector2(panelRect.X + panelRect.width - 20, panelRect.Y + 20), 10, Raylib.GREEN);
        }
    }

    public void DrawRowOfHiddenCards(Vector2 from, Vector2 to, Vector2 cardSize, int count, float angle)
    {
        // Raylib.DrawLineEx(from, to, 10, Raylib.RED);

        var step = (to - from) / count;
        for (int i = 0; i < count; i++)
        {
            var pos = from + step * (i + 0.5f);

            RlGl.rlPushMatrix();
            RlGl.rlTranslatef(pos.X, pos.Y, 0);
            RlGl.rlRotatef(angle, 0, 0, 1);
            DrawCardBackSide(new Rectangle(-cardSize.X / 2, -cardSize.Y / 2, cardSize.X, cardSize.Y));
            RlGl.rlPopMatrix();
        }

    }

    public UnoPlayer? GetMyUnoPlayer()
    {
        foreach (var player in match.players)
        {
            if (isMyPlayer(player))
            {
                return player;
            }
        }
        return null;
    }

    public void Tick(float dt)
    {
        var matchUpdate = net.GetMatchUpdate();
        if (matchUpdate != null)
        {
            for (int i = 0; i < matchUpdate.players.Count; i++)
            {
                UnoPlayer player;
                if (i >= match.players.Count)
                {
                    player = new UnoPlayer("");
                    match.players.Add(player);
                } else
                {
                    player = match.players[i];
                }

                player.id = matchUpdate.players[i].id;
                player.name = matchUpdate.players[i].name;
                player.isReady = matchUpdate.players[i].isReady;
            }

            while (match.players.Count > matchUpdate.players.Count)
            {
                match.players.RemoveAt(match.players.Count-1);
            }
        }

        var windowRect = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        var mouse = Raylib.GetMousePosition();

        var centerCardRect = Utils.GetCenteredRect(windowRect, cardSize);

        if (cardDrawQueue.Count > 0)
        {
            var now = DateTime.Now;
            var nextCardDrawAt = lastCardDrawAt + TimeSpan.FromSeconds(cardDrawInterval);
            if (now > nextCardDrawAt)
            {
                var entry = cardDrawQueue[0];
                cardDrawQueue.RemoveAt(0);
                DrawCardFromDeck(entry.Item1, entry.Item2);
                lastCardDrawAt = now;
            }
        }

        // Raise card under mouse to top
        for (int i = heldCards.Count - 1; i >= 0; i--)
        {
            var heldCard = heldCards[i]; 
            if (Raylib.CheckCollisionPointRec(mouse, heldCard.GetRect(cardSize)))
            {
                if (i == heldCards.Count - 1) break;
                heldCards.Remove(heldCard);
                heldCards.Add(heldCard);

                break;
            }
        }

        { // Card grabbing logic
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                for (int i = heldCards.Count - 1; i >= 0; i--)
                {
                    var heldCard = heldCards[i];
                    if (Raylib.CheckCollisionPointRec(mouse, heldCard.GetRect(cardSize)))
                    {
                        grabbedCard = heldCard;
                        break;
                    }
                }
            }
            else if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (grabbedCard != null)
                {
                    if (Vector2.Distance(Utils.RectCenter(centerCardRect), grabbedCard.position) < cardSize.Y)
                    {
                        PlayCard(GetMyUnoPlayer(), grabbedCard.card);
                    }
                }

                grabbedCard = null;
            }

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT) && grabbedCard != null)
            {
                grabbedCard.velocity += Raylib.GetMouseDelta()/dt;
            }
        }

        { // Push card to bottom of screen

            var offsetFromBottom = cardSize.Y * 0.1f;
            if (windowRect.height - mouse.Y < cardSize.Y * 2)
            {
                offsetFromBottom = cardSize.Y * 0.6f;
            }

            var heldSegmentSize = (float)Math.Min(windowRect.width * 0.8, cardSize.X * heldCards.Count);
            var segmentStart = (windowRect.width - heldSegmentSize) / 2;
            var heldSegmentStart = new Vector2(segmentStart, windowRect.height - offsetFromBottom);
            var heldSegmentEnd = new Vector2(segmentStart + heldSegmentSize, windowRect.height - offsetFromBottom);

            foreach (var heldCard in heldCards)
            {
                if (heldCard == grabbedCard) continue;
                var closest = ClosestPoinOnSegment(heldCard.position, heldSegmentStart, heldSegmentEnd);

                heldCard.velocity += (closest - heldCard.position) * 5;
                foreach (var otherCard in heldCards)
                {
                    if (otherCard == heldCard) continue;

                    var toOtherCard = (otherCard.position - heldCard.position);

                    if (toOtherCard.Length() < 10)
                    {
                        var rng = new Random();
                        heldCard.position += new Vector2(rng.NextSingle(), rng.NextSingle()) * 10;
                    }
                    else if (toOtherCard.Length() < cardSize.X*1.1)
                    {
                        heldCard.velocity.X -= (Vector2.Normalize(toOtherCard).X * 100000 / toOtherCard.LengthSquared());
                    }
                }

                if ((closest - heldCard.position).Length() < 1)
                {
                    heldCard.position = Vector2.Clamp(heldCard.position, heldSegmentStart, heldSegmentEnd);
                }
            }
        }

        { // Apply physics to cards
            foreach (var heldCard in heldCards)
            {
                heldCard.position += heldCard.velocity * dt;

                heldCard.velocity = Vector2.Zero;
            }
        }

        {
            var isEveryoneReady = match.players.All(p => p.isReady);
            if (match.players.Count >= 2 && isEveryoneReady && !match.started)
            {
                match.started = true;
                OnMatchStarted();
            }
        }

        var myUnoPlayer = GetMyUnoPlayer();

        var myPlayerIndex = -1;
        if (myUnoPlayer != null)
        {
            myPlayerIndex = match.players.IndexOf(myUnoPlayer);
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.RAYWHITE);

        if (!match.started && myUnoPlayer != null)
        {
            var stack = new VerticalStack
            {
                gap = 20,
                position = Utils.GetCenteredPosition(windowRect, new Vector2(200, 300))
            };

            if (myUnoPlayer.isReady)
            {
                RayGui.GuiLabel(stack.nextRectangle(100, 50), "Match has not started (you are ready)");
            } else
            {
                RayGui.GuiLabel(stack.nextRectangle(100, 50), "Match has not started");
            }

            if (RayGui.GuiButton(stack.nextRectangle(150, 50), myUnoPlayer.isReady ? "Unready" : "Ready"))
            {
                myUnoPlayer.isReady = !myUnoPlayer.isReady;
                net.UpdateReadiness(myUnoPlayer.isReady);
            }
        }

        if (match.topCard != null)
        {
            DrawCard(match.topCard, centerCardRect);
        }

        foreach (var heldCard in heldCards)
        {
            DrawCardShadow(heldCard.GetRect(cardSize));
        }

        var deckStackSize = 5;
        for (int i = 0; i < deckStackSize; i++)
        {
            DrawCardBackSide(new Rectangle(deckPosition.X + 4 * deckStackSize - i * 4, deckPosition.Y + 4 * deckStackSize - i * 4, cardSize.X, cardSize.Y));
        }

        foreach (var heldCard in heldCards)
        {
            DrawCard(heldCard.card, heldCard.GetRect(cardSize));
        }

        if (myUnoPlayer != null)
        {
            DrawPlayerInfo(new Vector2(10, windowRect.height - 90), myUnoPlayer);
        }

        var enemyCardScale = 0.7f;
        var enemyCardSize = cardSize * enemyCardScale;

        for (int offset = 1; offset < match.players.Count; offset++)
        {
            int i = (myPlayerIndex + offset) % match.players.Count;
            var player = match.players[i];

            if (i == (myPlayerIndex + 1) % match.players.Count)
            {
                DrawRowOfHiddenCards(new Vector2(windowRect.width, windowRect.height * 0.2f), new Vector2(windowRect.width, windowRect.height * 0.6f), enemyCardSize, player.hand.Count, -90);
                DrawPlayerInfo(new Vector2(windowRect.width - 170, windowRect.height - 90 - 100), player);
            }
            else if (i == (myPlayerIndex - 1) % match.players.Count)
            {
                DrawRowOfHiddenCards(new Vector2(0, windowRect.height * 0.48f), new Vector2(0, windowRect.height * 0.8f), enemyCardSize, player.hand.Count, 90);
                DrawPlayerInfo(new Vector2(10, 250), player);
            }
            else if (i == (myPlayerIndex + 2) % match.players.Count)
            {
                DrawRowOfHiddenCards(new Vector2(windowRect.width*0.3f, 0), new Vector2(windowRect.width * 0.8f, 0), enemyCardSize, player.hand.Count, 180);
                DrawPlayerInfo(new Vector2(windowRect.width - 170, 10), player);
            }
        }

        Raylib.EndDrawing();
    }
}
