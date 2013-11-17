import MessageServer

class Keywords:
    Prompt = "PROMPT"
    AllowedClass = "ALLOWEDCLASS"
    RejectMessage = "REJECTMESSAGE"
    Name = "NAME"

def GetSelectionOptions(Prompt = None, AllowedClasses = None, RejectMessage = None, Name = None):
    dictionary = {}
    if AllowedClasses is not None:
        dictionary[Keywords.AllowedClasses] = AllowedClasses
    if Prompt is not None:
        dictionary[Keywords.Prompt] = Prompt
    if RejectMessage is not None:
        dictionary[Keywords.RejectMessage] = RejectMessage
    if Name is not None:
        dictionary[Keywords.Name] = Name
    return dictionary

def GetUserString(reply):
    result = tuple(a for a in reply.payload)
    return result
