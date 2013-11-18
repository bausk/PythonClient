from Protocol import Protocol

class Message(object):
    def __init__( self, Action = None, Payload = None, Callback = None, Status = None, Parameters = None):
        self.Action = Action
        self.ContentType = Protocol.Types[type(Payload)]
        self.Parameters = Parameters
        self._Payload = None
        #self.SerializedContent = simplejson.dumps(Content)
        self.Callback = Callback
        self.Status = Status
    def Finalize(self):
        self.Status = Protocol.Status.FINISH

    @property
    def Payload(self):
        return self._Payload

    @Payload.setter
    def Payload(self, value):
        """ Payload of a ServerMessage is always a list of dicts
        """
        self._Payload = []
        if isinstance(value, list):
            self._Payload = value
        elif isinstance(value, dict):
            self._Payload.append(value)
        else:
            self._Payload.append({Protocol.Keywords.DEFAULT: value})

    def Parse(self):
        if type(self.Payload) is list:
            return (a for a in self.Payload)
        else:
            return self.Payload

class MessageFactory(object):
    @classmethod
    def Error(cls, error, id, ErrorMessages):
        message = Message(Action = Protocol.ServerAction.WRITE, Payload = ErrorMessages[type(error)].format(id, error.message), Status = Protocol.Status.FINISH)
        return message

    @classmethod
    def NewErrorMessage(cls, error, id, ErrorMessages):
        message = Message(Action = Protocol.ServerAction.WRITE, Payload = ErrorMessages[type(error)].format(id, error.message), Status = Protocol.Status.FINISH)
        return message

    @classmethod
    def GetUserString(cls, prompt = None):
        """Forms a message for single user input request. str Prompt is a command line message prompt.
        """
        msg = Message(Action = Protocol.ServerAction.REQUEST_USER_INPUT, Status = Protocol.Status.ONHOLD, Parameters = {"InputType": Protocol.PL_STRING}, Payload = prompt)
        return msg

    @classmethod
    def Write(cls, str):
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
