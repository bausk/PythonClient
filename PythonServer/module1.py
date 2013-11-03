class StateSequenceError(Exception):
         def __init__(self):
             self.value = "huhuhue"
         def __str__(self):
             return self.value

class Handler(object):
    ErrorMessages = {
                     KeyError: "Command {} was not recognized by server",
                     NotImplementedError: "Command {} is not implemented yet",
                     StateSequenceError: "huehuhuehuehue {} hue"                     
                     }

    @classmethod
    def NewErrorMessage(cls, error, id):
        message = Message(Action = Protocol.ServerAction.SENDMSG, Payload = cls.ErrorMessages[type(error)].format(id), Status = Protocol.Status.FINISH)
        return message

a = Handler.NewErrorMessage(StateSequenceError(), "11")