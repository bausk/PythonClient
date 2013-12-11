from Protocol import Protocol
import simplejson

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

    def Serialize(self):
        return simplejson.dumps(self.__dict__)

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
    def Write(cls, *str):
        """Forms a message writing str to client's standard output
        """
        msg = Message(Action = Protocol.ServerAction.WRITE, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = str)
        return msg

    @classmethod
    def StartTransaction(cls):
        return Message(Action = Protocol.ServerAction.TRANSACTION_START, Status = Protocol.Status.ONHOLD)
      
    @classmethod
    def CommitTransaction(cls):
        return Message(Action = Protocol.ServerAction.TRANSACTION_COMMIT, Status = Protocol.Status.ONHOLD)
    
    @classmethod
    def AbortTransaction(cls):
        return Message(Action = Protocol.ServerAction.TRANSACTION_ABORT, Status = Protocol.Status.ONHOLD)

    @classmethod
    def Batch(cls, *messages):
        return Message(Action = Protocol.CommonAction.BATCH, Status = Protocol.Status.OK, Payload = [msg.Serialize() for msg in messages])