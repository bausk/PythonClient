from Protocol import AutoCADProtocol as Protocol
from Message import Message, MessageFactory
from functools import partial
import types


class Struct(object):
    def __init__(self, **entries):
        self.__dict__.update(entries)

    @classmethod
    def PartialPayload(func, *args, **keywords):
        def ConstructMessage(payload):
            """single payload argument is expected
            See documentation for Payload.py
            """
            newkeywords = keywords.copy()
            newkeywords['payload'] = payload
            return func(*(args), **newkeywords)
        ConstructMessage.func = func
        ConstructMessage.args = args
        ConstructMessage.keywords = keywords
        return ConstructMessage

    @classmethod
    def PartialPayloadAndKeys(func, *args, **keywords):
        def ConstructMessage(payload, **fkeywords):
            """single payload argument is expected
            See documentation for DraftSocket Python API
            """
            newkeywords = keywords.copy()
            newkeywords.update(fkeywords)
            newkeywords['payload'] = payload
            return func(*(args), **newkeywords)
        ConstructMessage.func = func
        ConstructMessage.args = args
        ConstructMessage.keywords = keywords
        return ConstructMessage


def GetEntities(reply):
    """Returns a tuple of entities from a GetEntities reply
    Usage example: a ,= GetEntity(Message()) - Message() should be a reply from the client
    """
    retval = []
    for payloadItem in reply.Payload:
        returnItem = Struct()
        returnItem.Default = payloadItem[Protocol.Keywords.DEFAULT]
        if Protocol.Keywords.NAME in payloadItem:
            returnItem.Name = payloadItem[Protocol.Keywords.NAME]
        returnItem.ObjectId = payloadItem[Protocol.Local.ObjectID]
        returnItem.Handle = payloadItem[Protocol.Local.Handle]
        returnItem.TypeName = payloadItem[Protocol.Local.TypeName]
        returnItem.SwapIdWith = Struct.PartialPayload(MessageFactory.New, Action = Protocol.AutocadAction.DBObject.SwapIdWith, Status = Protocol.Status.ONHOLD)
        retval.append(returnItem)
    return (retval)

