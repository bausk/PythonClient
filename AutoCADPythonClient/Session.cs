using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Draftsocket
{
    interface ISession
    {
        List<Dictionary<string, object>> ObjectsToDicts<T>(List<T> obj);
        ClientMessage DispatchReply(ServerMessage Reply);
        Dictionary<string, object> SavedObjects {get; set;}
    }
}
