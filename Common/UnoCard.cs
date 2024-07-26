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
}
