using Raylib_CsLo;
using System.Drawing;

namespace Application;

internal class MatchScreen
{
    public UnoMatch match;

    public MatchScreen()
    {
        match = new UnoMatch();
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

    

    public void Tick(float dt)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.RAYWHITE);

        Raylib.EndDrawing();
    }
}
