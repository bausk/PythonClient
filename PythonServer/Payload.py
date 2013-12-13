from Protocol import AutoCADProtocol as Protocol
from Message import Message, MessageFactory
from functools import partial

class Struct(object):
    def __init__(self, **entries):
        self.__dict__.update(entries)

def GetEntity(reply):
    retval = []
    for payloadItem in reply.Payload:
        returnItem = Struct()
        returnItem.Default = payloadItem[Protocol.Keywords.DEFAULT]
        if Protocol.Keywords.NAME in payloadItem:
            returnItem.Name = payloadItem[Protocol.Keywords.NAME]
        returnItem.ObjectId = payloadItem[Protocol.Local.ObjectID]
        returnItem.Handle = payloadItem[Protocol.Local.Handle]
        returnItem.TypeName = payloadItem[Protocol.Local.TypeName]
        returnItem.SwapIdWith = partial(MessageFactory. Message(Action = Protocol.AutocadAction.DBObject.SwapIdWith)
        retval.append(returnItem)
    return (retval)