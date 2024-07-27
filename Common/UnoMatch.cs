using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

public class UnoMatch
{
    public List<UnoPlayer> players = new List<UnoPlayer>();
    public UnoCard topCard;
    public bool isDirectionClockwise;
    public uint currentPlayer;
}
