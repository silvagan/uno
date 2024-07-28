using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

public class UnoMatch
{
    public bool started = false;
    public List<UnoPlayer> players = new List<UnoPlayer>();
    public UnoCard? topCard = null;
    public bool isDirectionClockwise;
    public uint currentPlayer;
}
