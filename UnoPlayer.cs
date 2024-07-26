using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno;

internal class UnoPlayer
{
    string name;
    List<UnoCard> hand;

    public UnoPlayer(string name)
    {
        this.name = name;
        this.hand = new List<UnoCard>();
    }
}
