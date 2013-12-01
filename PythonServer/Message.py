from Protocol import Protocol

class Struct(object):
    def __init__(self, **entries): 
        self.__dict__.update(entries)

class Message(object):
    def __init__( self, Action = None, Payload = None, Callback = None, Status = None, Parameters = None):
        self.Action = Action
        self.Parameters = Parameters
        self.SetPayloadList(Payload)
        self.Callback = Callback
        self.Status = Status

    def Finalize(self):
        self.Status = Protocol.Status.FINISH

    def Terminate(self):
        self.Status = Protocol.Status.TERMINATE

    def SetPayloadList(self, value):
        """ Payload of a ServerMessage is always a list of dicts
        """
        self.Payload = []
        if isinstance(value, list) or isinstance(value, tuple):
            for item in value:
                if isinstance(item, str):
                    self.Payload.append({Protocol.Keywords.DEFAULT: item})
                elif isinstance(item, dict):
                    self.Payload.append(item)
                else:
                    self.Payload.append({Protocol.Keywords.OBJECT: item})
        elif isinstance(value, dict):
            self.Payload.append(value)
        else:
            self.Payload.append({Protocol.Keywords.DEFAULT: value})

    def GetDataAsRaw(self):
        if type(self.Payload) is list:
            return (a for a in self.Payload)
        else:
            return self.Payload

    def GetDataAsDicts(self):
        if type(self.Payload) is list:
            return (a[Protocol.Keywords.DEFAULT] for a in self.Payload)
        else:
            return self.Payload

    def GetDataAsStructs(self):
        #WORK HERE
        if type(self.Payload) is list:
            return (Struct(**a[Protocol.Keywords.DEFAULT]) for a in self.Payload)
        else:
            return self.Payload



class MessageFactory(object):
    @classmethod
    def Error(cls, error, id, ErrorMessages):
        message = Message(Action = Protocol.ServerAction.WRITE, Payload = ErrorMessages[type(error)].format(id, error.message), Status = Protocol.Status.FINISH)
        return message

    @classmethod
    def Termination(cls):
        message = Message(Action = Protocol.CommonAction.TERMINATE, Status = Protocol.Status.FINISH)
        return message

    @classmethod
    def GetUserStrings(cls, *prompts):
        """Forms a message for single user input request. str Prompt is a command line message prompt.
        """
        msg = Message(Action = Protocol.ServerAction.REQUEST_USER_INPUT, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = [a for a in prompts])
        return msg

    @classmethod
    def Write(cls, *str):
        """Forms a message writing str to client's standard output
        """
        msg = Message(Action = Protocol.ServerAction.WRITE, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = str)
        return msg

    @classmethod
    def GetObjectID(cls, *prompt): #DEPRECATED
        """Wrapper for GetEntity client command.
        Request entity ID's with arguments as prompts.
        """
        msg = Message(Action = Protocol.ServerAction.GET_ENTITY_ID, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = prompt)
        return msg

    @classmethod
    def GetEntity(cls, prompt):
        """Request entity ID's with arguments as prompts.
        """
        msg = Message(Action = Protocol.ServerAction.GET_ENTITY_ID, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = prompt)
        return msg
