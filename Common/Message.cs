using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum MessageType
    {
        ID,

        Connect,
        Disconnect,

        UpdateReadiness,

        UpdateMatchData,

        StartGame,
        PlaceCard,
        EndGame
    }
    public class ReceivedMessage
    {
        public MessageType type;
        public byte[] payload;
    };
}
