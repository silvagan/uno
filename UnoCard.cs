using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno;

enum UnoCardColor
{
    Red,
    Blue,
    Yello,
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
    UnoCardColor color;
    UnoCardType type;
    int number;
}
