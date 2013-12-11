using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

namespace Draftsocket
{
    public partial class AutoCAD : ISession
    {
        public ClientMessage GetKeyword(ServerMessage reply)
        {
                List<Dictionary<string, object>> Prompts = reply.Payload;
                var Result = new List<PromptResult>();
                foreach (Dictionary<string, object> PromptDict in Prompts)
                {
                    PromptKeywordOptions pko;

                    object value;
                    if (PromptDict.TryGetValue(Protocol.Local.Prompt, out value))
                        pko = new PromptKeywordOptions((string)PromptDict[Protocol.Local.Prompt]);
                    else
                        return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);

                    if (PromptDict.TryGetValue(Protocol.Local.AllowNone, out value))
                        pko.AllowNone = true;

                    if (PromptDict.TryGetValue(Protocol.Local.AllowArbitraryInput, out value))
                        if ((bool)value == true)
                            pko.AllowArbitraryInput = true;
                        else
                            pko.AllowArbitraryInput = false;
                    if (PromptDict.TryGetValue(Protocol.Local.Keywords, out value))
                        foreach (string Keyword in (Newtonsoft.Json.Linq.JArray)value)
                            pko.Keywords.Add(Keyword);
                    else
                        return new ClientMessage(Protocol.ClientAction.ERROR, Protocol.Status.FINISH);

                    if (PromptDict.TryGetValue(Protocol.Local.Default, out value))
                        pko.Keywords.Default = (string)value;

                    PromptResult pkr =
                      ed.GetKeywords(pko);

                    //Add the result to payload (SetPayload called later to form the message).
                    Result.Add(pkr);

                    //Memoization: if a name field was set in the payload item of reply message,
                    //keep the result (per) in SavedObjects
                    if (PromptDict.TryGetValue(Protocol.Local.Name, out value))
                        this.SavedObjects.Add((string)value, pkr);
                }

                ClientMessage message = new ClientMessage(Protocol.ClientAction.CONTINUE);
                List<Dictionary<string, object>> DictResult = this.ObjectsToDicts(Result);
                message.SetPayload(DictResult);
                return message;
        }
    }
}
