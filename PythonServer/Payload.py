from Protocol import AutoCADProtocol as Protocol
from Message import Message

class Struct(object):
    def __init__(self, **entries):
        self.__dict__.update(entries)

def GetEntity(reply):
    retval = []
    for payloadItem in reply.Payload:
        returnItem = Struct()
        returnItem.Default = payloadItem[Protocol.Keywords.DEFAULT]
        returnItem.Name = payloadItem[Protocol.Local.Name]
        returnItem.ObjectId = payloadItem[Protocol.Local.ObjectID]
        returnItem.Handle = payloadItem[Protocol.Local.Handle]
        returnItem.TypeName = payloadItem[Protocol.Local.TypeName]
        returnItem.SwapIdWith = Mess
        retval.append(returnItem)
    return (retval)