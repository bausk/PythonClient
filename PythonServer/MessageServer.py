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

def GenerateUuid():
    return str(uuid.uuid4())

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

class Protocol(object):

    class CAction:
        CMD = "COMMAND"
        ERROR = "CLIENT_ERROR"
        EVENT = "EVENT"
        CONTINUE = "CONTINUE"
        CONSOLE = "CONSOLE"

    class ServerAction:
        SETEVENT = "SET_EVENT"
        WRITE = "WRITE_MESSAGE"
        REQUEST_USER_INPUT = "REQUEST_INPUT"
        REQUEST_SEVERAL_USER_INPUTS = "REQUEST_SEVERAL_INPUTS"
        MANIPULATE = "MANIPULATE_DB"
        TERMINATE = "TERMINATE_SESSION"
        GET_ENTITY = "GET_ENTITY"

    class Status:
        FINISH = "_FINISH"
        OK = "_OK"
        ONHOLD = "_ONHOLD"
        

    #Payload types
    PL_STRING = 1
    PL_ENTITIES = 2

    #types
    Types = {
                type(0):"INT",
                type(""):"STRING",
                type([]):"LIST",
                type({}):"DICTIONARY",
                object:"OBJECT",
                type(None):"NONE"
                }



class Message(object):
    def __init__( self, Action = None, Payload = None, Callback = None, Status = None, Parameters = None):
        self.Action = Action
        self.ContentType = Protocol.Types[type(Payload)]
        self.Parameters = Parameters
        self.Payload = Payload
        #self.SerializedContent = simplejson.dumps(Content)
        self.Callback = Callback
        self.Status = Status
    def Finalize(self):
        self.Status = Protocol.Status.FINISH


class Procedure(object):
    def __init__ (self):
        self.Objects = {}
        self.Uuid = ""
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
            reply = Handler.NewErrorMessage(StateSequenceError, message.Callback)
        return reply

    @classmethod
    def GetSubclassesDict(cls):
        return {Alphanumeric(x.__name__):x for x in cls.__subclasses__()}


    

class MessageEncoder(JSONEncoder):
    def default(self, o):
        return {
                "Action": o.Action,
                "ContentType": o.ContentType,
                "Payload": o.Payload,
                "Callback": o.Callback
                }

class Handler(object):
    ErrorMessages = {
                     KeyError: "Command {} was not recognized by server. Command aborted.",
                     NotImplementedError: "Command {} is not implemented yet.",
                     StateSequenceError: "Command {} is missing a well-formed state.",
                     }
    def __init__(self):
        self.dInstantiatedProcedures = {}
        self.dRegisteredProcedures = Procedure.GetSubclassesDict()
    def Handler(self, alive_socket, *args, **kwargs):
        stringReply = alive_socket.recv().decode("utf_16")
        #stringReply = u'{"Action":"COMMAND","ContentType":"NONE","Callback":"REPENT","Content":""}'
        mReply = Message()
        #mReply = simplejson.loads(stringReply, object_hook=_json_object_hook)
        mReply.__dict__ = simplejson.loads(stringReply)
        print("Received by handler: " + stringReply + "\n")
        #Cleanup for errors received from client should be somewhere here.
        #Something like this: self.RegisteredMethods[message.Content].__init__()
        MethodIdentifier = Alphanumeric(mReply.Callback)
        try:
            if mReply.Action.upper() == Protocol.CAction.CMD:
                    MethodUUID = self.InstantiateProcedure(MethodIdentifier)
                    self.dInstantiatedProcedures[MethodUUID].Uuid = MethodUUID
            else:
                MethodUUID = MethodIdentifier
            WorkerProcedure = self.dInstantiatedProcedures[MethodUUID]
            mMessage = WorkerProcedure(mReply)
            mMessage.Callback = MethodUUID
        except Exception, ex:
            mMessage = self.NewErrorMessage(ex, MethodIdentifier)
        stringReply = simplejson.dumps(mMessage.__dict__)
        alive_socket.send(stringReply)
        #reply = {'Action':"Set Event", 'ContentType':"None", 'Content':"ObjectCreated", 'SerializedContent':'"ObjectCreated"'}

    def InstantiateProcedure(self, classname, *args, **kwargs):
        cls = self.dRegisteredProcedures[classname]
        uuid = Alphanumeric(GenerateUuid())
        self.dInstantiatedProcedures[uuid] = cls(*args, **kwargs)
        return uuid


    @classmethod
    def NewErrorMessage(cls, error, id):
        message = Message(Action = Protocol.ServerAction.WRITE, Payload = cls.ErrorMessages[type(error)].format(id), Status = Protocol.Status.FINISH)
        return message

    @classmethod
    def NewGetUserStringMessage(cls, request_string):
        """Forms a message for single user input request
        """
        msg = Message(Action = Protocol.ServerAction.REQUEST_USER_INPUT, Status = Protocol.Status.ONHOLD, Parameters = {"InputType": Protocol.PL_STRING}, Payload = request_string)
        return msg

    @classmethod
    def NewWriteMessage(cls, str):
        """Forms a messah=ge writing str to client's standard output
        """
        msg = Message(Action = Protocol.ServerAction.WRITE, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = str)
        return msg