using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application;

public class UnoPlayer
{
    public string name;
    public List<UnoCard> hand;
    public bool isReady = false;
    public int id = 0;

    public UnoPlayer(string name)
    {
        this.name = name;
        this.hand = new List<UnoCard>();
    }

}
