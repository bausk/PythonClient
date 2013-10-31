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

def _json_object_hook(d): return namedtuple('Message', d.keys())(*d.values())

def Alphanumeric(string):
    return "".join([x if x.isalnum() else "" for x in string.upper()])

def state(statenum):
    def decorator (f):
        f.func_dict['state'] = statenum
        @wraps(f)   # In order to preserve docstrings, etc.
        def wrapped (self, *args, **kwargs):
            f.state = statenum
            return f(self, *args, **kwargs)
        return wrapped
    return decorator

class Protocol(object):
    FINISH = "__FINISH__"
    CMD = "COMMAND"
    SENDMSG = "SENDMESSAGE"
    REQUEST_USER_INPUT = "REQUEST_INPUT"
    Types = {
                type(""):"STRING",
                type([]):"LIST",
                type({}):"DICTIONARY",
                object:"OBJECT",
                type(None):"NONE"
                }


class Message(object):
    def __init__( self, MessageType = "", Content = None, Callback = ""):
        self.MessageType = MessageType
        self.ContentType = Protocol.Types[type(Content)]
        self.Content = Content
        #self.SerializedContent = simplejson.dumps(Content)
        self.Callback = Callback


class Procedure(object):
    def __init__ (self):
        self.Objects = {}
        self.Uuid = ""
        self.CurrentState = 0
        #methods = [method for method in dir(self) if hasattr(getattr(self, method), 'state')]
        self.StateDict = dict([(getattr(self,method).state, getattr(self,method)) for method in dir(self) if hasattr(getattr(self, method), 'state')])
        #self.StateDict = {methodname: value for me}
    def __call__( self, message = Message() ):
        reply = self.StateDict[self.CurrentState](message)
        self.CurrentState += 1
        return reply

    @classmethod
    def GetSubclassesDict(cls):
        return {Alphanumeric(x.__name__):x for x in cls.__subclasses__()}
    

class MessageEncoder(JSONEncoder):
    def default(self, o):
        return {
                "MessageType": o.MessageType,
                "ContentType": o.ContentType,
                "Content": o.Content,
                "Callback": o.Callback
                }

class Handler(object):
    def __init__( self , reg):
        self.RegisteredMethods = reg
            
    def handler(self, alive_socket, *args, **kwargs):
        message_string = alive_socket.recv().decode("utf_16")
        message = Message()
        message = simplejson.loads(message_string, object_hook=_json_object_hook)
        print("Received by handler: " + message_string + "\n")
        #Cleanup for errors received from client should be somewhere here.
        #Something like this: self.RegisteredMethods[message.Content].__init__()
        WorkingMethod = self.RegisteredMethods[message.Callback]
        reply = WorkingMethod(message)
        reply_string = simplejson.dumps(reply.__dict__)
        alive_socket.send(reply_string)
        #reply = {'MessageType':"Set Event", 'ContentType':"None", 'Content':"ObjectCreated", 'SerializedContent':'"ObjectCreated"'}

class Handler2(object):
    ErrorMessages = {
                     KeyError: "Command {} was not recognized by server",
                     NotImplementedError: "Command {} is not implemented yet",
                     }
    def __init__(self):
        self.dInstantiatedProcedures = {}
        self.dRegisteredProcedures = Procedure.GetSubclassesDict()
    def Handler(self, alive_socket, *args, **kwargs):
        #message_string = alive_socket.recv().decode("utf_16")
        stringReply = u'{"MessageType":"COMMAND","ContentType":"NONE","Callback":"REPENT","Content":""}'
        mReply = Message()
        #mReply = simplejson.loads(stringReply, object_hook=_json_object_hook)
        mReply.__dict__ = simplejson.loads(stringReply)
        print("Received by handler: " + stringReply + "\n")
        #Cleanup for errors received from client should be somewhere here.
        #Something like this: self.RegisteredMethods[message.Content].__init__()
        MethodIdentifier = Alphanumeric(mReply.Callback)
        try:
            if mReply.MessageType.upper() == Protocol.CMD:
                    MethodUUID = self.InstantiateProcedure(MethodIdentifier)
            else:
                MethodUUID = MethodIdentifier
            WorkerProcedure = self.dInstantiatedProcedures[MethodUUID]
            mMessage = WorkerProcedure(mReply)
        except Exception, ex:
            mMessage = self.ComposeErrorMessage(ex, MethodIdentifier)
        stringReply = simplejson.dumps(mMessage.__dict__)
        alive_socket.send(stringReply)
        #reply = {'MessageType':"Set Event", 'ContentType':"None", 'Content':"ObjectCreated", 'SerializedContent':'"ObjectCreated"'}

    def InstantiateProcedure(self, classname, *args, **kwargs):
        cls = self.dRegisteredProcedures[classname]
        uuid = Alphanumeric(self.GenerateUuid())
        self.dInstantiatedProcedures[uuid] = cls(*args, **kwargs)
        return uuid

    @staticmethod
    def GenerateUuid():
        return str(uuid.uuid4())

    @classmethod
    def ComposeErrorMessage(cls, error, id):
        message = Message(Protocol.SENDMSG, cls.ErrorMessages[type(error)].format(id), Protocol.FINISH)
        return message
