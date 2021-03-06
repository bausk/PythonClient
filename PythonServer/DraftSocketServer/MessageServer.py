# Created: 08.03.13
# License: MIT License
#from __future__ import unicode_literals
__author__ = "Alex Bausk <bauskas@gmail.com>"

import sys
import simplejson
from json import JSONEncoder
from sys import argv
from collections import namedtuple
import time
import zmq
from zmq.eventloop import ioloop
from functools import wraps
import uuid

from Protocol import Protocol
from Message import Message, MessageFactory

def _json_object_hook(d): return namedtuple('Message', d.keys())(*d.values())

def Alphanumeric(string):
    return "".join([x if x.isalnum() else "" for x in string.upper()])

def GenerateUuid():
    return str(uuid.uuid4())

def Deserialize(str, type = Message):
    retval = type()
    retval.__dict__ = simplejson.loads(str)
    return retval

class StateSequenceError(Exception):
     def __init__(self, value):
         self.value = value
     def __str__(self):
         return self.value

def state(statenum):
    def decorator (f):
        f.func_dict['state'] = statenum
        @wraps(f)
        def wrapped (self, *args, **kwargs):
            f.state = statenum
            return f(self, *args, **kwargs)
        return wrapped
    return decorator

class Procedure(object):
    def __init__ (self, Socket = None):
        self.Objects = {}
        self.Uuid = ""
        self.Socket = Socket
        self.CurrentState = 0
        #methods = [method for method in dir(self) if hasattr(getattr(self, method), 'state')]
        self.StateDict = dict([(getattr(self,method).state, getattr(self,method)) for method in dir(self) if hasattr(getattr(self, method), 'state')])
        #self.StateDict = {methodname: value for me}
    def __call__( self, message = Message() ):
        if self.CurrentState in self.StateDict:
            reply = self.StateDict[self.CurrentState](message)
            self.CurrentState += 1
        else:
            raise StateSequenceError(0)
        return reply

    @classmethod
    def GetSubclassesDict(cls):
        return {Alphanumeric(x.__name__):x for x in cls.__subclasses__()}

class Handler(object):
    ErrorMessages = {
                     KeyError: "Command {} was not recognized by server. Command aborted.",
                     NotImplementedError: "Command {} is not implemented yet.",
                     StateSequenceError: "Command {} is missing a well-formed state.",
                     TypeError: "TypeError: {1}",
                     AttributeError: "AttributeError: {1}"
                     }

    def __init__(self):
        self.dInstantiatedProcedures = {}
        self.dRegisteredProcedures = Procedure.GetSubclassesDict()

    def HandleReceiveLoop(self, alive_socket, *args, **kwargs):
        stringReply = alive_socket.recv().decode("utf_16")
        mReply = Message()
        mReply.__dict__ = simplejson.loads(stringReply)
        print("Received by handler: " + stringReply + "\n")
        #Cleanup for errors received from client should be somewhere here.
        MethodIdentifier = 'None'

        try:
            MethodIdentifier = Alphanumeric(mReply.Callback)
            if mReply.Action.upper() == Protocol.ClientAction.CMD:
                MethodUUID = self.InstantiateProcedure(MethodIdentifier, Socket = alive_socket)
                self.dInstantiatedProcedures[MethodUUID].Uuid = MethodUUID
            else:
                MethodUUID = MethodIdentifier
        except Exception, ex:
            mMessage = MessageFactory.Error(ex, MethodIdentifier, self.ErrorMessages)

        if mReply.Action.upper() != Protocol.CommonAction.TERMINATE and mReply.Status.upper() != Protocol.Status.FINISH:
            try:
                WorkerProcedure = self.dInstantiatedProcedures[MethodUUID]
                mMessage = WorkerProcedure(mReply) #actual call
            except Exception, ex:
                mMessage = MessageFactory.Error(ex, MethodIdentifier, self.ErrorMessages)

            mMessage.Callback = MethodUUID #here? doubtful
            stringReply = simplejson.dumps(mMessage.__dict__)
            alive_socket.send(stringReply)

        else:
            #Invoked after reply is received for a TERM message.
            #Incoming action is a demand to terminate all work.
            #Kill the instance bound to incoming callback, exit
            try:
                del self.dInstantiatedProcedures[MethodUUID]
                if mReply.Status.upper() == Protocol.Status.FINISH:
                    #Invoked when clients says its reply is the last message.
                    #Don't bother sending anything, kill the instance, exit
                    if WorkerProcedure in locals():
                        del WorkerProcedure
            except Exception, ex:
                print(ex.message)

        #Test string: '{"Status": "_ONHOLD", "ContentType": "NONE", "Parameters": [{"Prompt": "Choose first entity"}, {}], "Callback": "E7C2B6230C8647059ACEC108F957D3F5", "Action": "GET_ENTITY", "Payload": null}'

    def HandleSendLoop(self, alive_socket):
        pass

    def InstantiateProcedure(self, classname, *args, **kwargs):
        cls = self.dRegisteredProcedures[classname]
        uuid = Alphanumeric(GenerateUuid())
        self.dInstantiatedProcedures[uuid] = cls(*args, **kwargs) #To return a correct instantiated procedure, we select class and generate uuid to refer to the instance
        return uuid

    @classmethod
    def NewErrorMessage(cls, error, id, ErrorMessages):
        message = Message(Action = Protocol.ServerAction.WRITE, Payload = ErrorMessages[type(error)].format(id, error.message), Status = Protocol.Status.FINISH)
        return message