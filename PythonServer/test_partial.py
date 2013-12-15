from Protocol import AutoCADProtocol as Protocol
from Message import Message, MessageFactory
from functools import partial


class Struct(object):
    def __init__(self, **entries):
        self.__dict__.update(entries)

def partial1(func, *args, **keywords):
    def partialpayload(payload, **fkeywords):
        """single payload argument is expected
        See documentation for Payload.py
        """
        newkeywords = keywords.copy()
        newkeywords.update(fkeywords)
        newkeywords['payload'] = payload
        return func(*(args), **newkeywords)
    partialpayload.func = func
    partialpayload.args = args
    partialpayload.keywords = keywords
    return partialpayload

def GetEntity(reply):
    returnItem = Struct()
    returnItem.ObjectId = 41
    returnItem.SwapIdWith = partial1(MessageFactory.New, action = Protocol.AutocadAction.DBObject.SwapIdWith, status = Protocol.Status.ONHOLD)
    return returnItem

a = GetEntity(Message())

msg = a.SwapIdWith(42)
