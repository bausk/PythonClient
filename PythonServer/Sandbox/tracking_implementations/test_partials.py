#from Protocol import AutoCADProtocol as Protocol
#from Message import Message, MessageFactory
#from functools import partial
from __future__ import absolute_import
import sys
from AutoCAD import Payload

class Struct(object):
    def __init__(self, **entries):
        self.__dict__.update(entries)

def GetEntity(reply):
    returnItem = Struct()
    returnItem.ObjectId = reply
    def swapidwith(id):
        return MessageFactory.New(action = Protocol.AutocadAction.DBObject.SwapIdWith, status = Protocol.Status.ONHOLD, payload = [returnItem.ObjectId, id])
    returnItem.SwapIdWith = swapidwith
    return returnItem

a = GetEntity(1)

b = GetEntity(3)

msg = a.SwapIdWith(40)

a.ObjectId = 2

msg2 = b.SwapIdWith(50)

msg3 = a.SwapIdWith(45)
#woahdude