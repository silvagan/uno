using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

public enum UnoCardColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Special
};

public enum UnoCardType
{
    Number,
    Block,
    Reverse,
    PlusTwo,
    PlusFour,
    ChangeColor
};

public class UnoCard
{
    public UnoCardColor color;
    public UnoCardType type;
    public int number = -1;
    public UnoCardColor changedColor; 

    public static bool CanCardBePlayed(UnoCard bottomCard, UnoCard topCard)
    {
        // A special card can always be placed on top of anything else
        if (topCard.color == UnoCardColor.Special)
        {
            return true;
        }

        // If both card are a number, and those numbers match. It can be placed
        if (bottomCard.type == UnoCardType.Number && topCard.type == UnoCardType.Number && bottomCard.number == topCard.number)
        {
            return true;
        }

        if (bottomCard.type == topCard.type)
        {
            return true;
        }

        UnoCardColor bottomColor = bottomCard.color;
        if (bottomCard.type == UnoCardType.ChangeColor)
        {
            bottomColor = bottomCard.changedColor;
        }

        // If the color matches, in can be placed on top.
        return bottomColor == topCard.color;
    }
}
