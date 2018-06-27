using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bot.Models
{
    enum ChatState
    {
        Start = 0,
        SendNumber,
        FullName,
        Id,
        Reshteh,
        End,
        AddNote,
        GetDatails,
        SendMessage,
        Search,
        AddMore,
        DeleteList,
        AddUser,
        UserMessage,
        Remove
    }
}
