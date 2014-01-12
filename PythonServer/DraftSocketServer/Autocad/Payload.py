from ..Protocol import AutoCADProtocol as Protocol
from ..Message import Message, MessageFactory
from ..Types import Struct

def GetEntities(reply = Message()):
    """Returns a tuple of entities from a GetEntities reply
    Usage example: a ,= GetEntity(ReplyFromClient)
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
        def swapidwith(id):
            return MessageFactory.New(action = Protocol.AutocadAction.DBObject.SwapIdWith, status = Protocol.Status.ONHOLD, payload = [returnItem.ObjectId, id])
        returnItem.SwapIdWith = swapidwith
        retval.append(returnItem)
    return (retval)

