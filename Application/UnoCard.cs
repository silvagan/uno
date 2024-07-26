using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

enum UnoCardColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Special
};

enum UnoCardType
{
    Number,
    Block,
    Reverse,
    PlusTwo,
    PlusFour,
    ChangeColor
};

internal class UnoCard
{
    public UnoCardColor color;
    public UnoCardType type;
    public int number = -1;
}
