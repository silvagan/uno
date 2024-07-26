using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

internal class UnoMatch
{
    List<UnoPlayer> players = new List<UnoPlayer>();
    UnoCard topCard;
    bool isDirectionClockwise;
    uint currentPlayer;
}
