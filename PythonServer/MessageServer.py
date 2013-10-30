# Created: 08.03.13
# License: MIT License
#from __future__ import unicode_literals
__author__ = "Alex Bausk <bauskas@gmail.com>"

import sys
import simplejson
from json import JSONEncoder
from sys import argv
#import collections
from collections import namedtuple
import time
import zmq
from zmq.eventloop import ioloop
import uuid

def _json_object_hook(d): return namedtuple('Message', d.keys())(*d.values())



def GenerateUuid():
    return str(uuid.uuid4())

class Message(object):
    Types = {
                type(""):"STRING",
                type([]):"LIST",
                type({}):"DICTIONARY",
                object:"OBJECT",
                type(None):"NONE"
                }
    def __init__( self, MessageType = "", Content = None, Callback = ""):
        self.MessageType = MessageType
        self.ContentType = self.Types[type(Content)]
        self.Content = Content
        #self.SerializedContent = simplejson.dumps(Content)
        self.Callback = Callback



class Procedure(object):
    #Procedures = {}
    def __init__ (self):
        self.Objects = {}
        self.Uuid = ""
        #self.Procedures[self.__class__.__name__] = self.__class__
    @classmethod
    def GetSubclassesDict(cls):
        return {Alphanumeric(x.__name__):x for x in cls.__subclasses__()}
    @staticmethod
    def Alphanumeric(string):
        return "".join([x if x.isalnum() else "" for x in string.upper()])
    

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
    def __init__(self):
        self.dInteractions = {}
        self.dRegisteredProcedures = Procedure.GetSubclassesDict()
    def handler(self, alive_socket, *args, **kwargs):
        #message_string = alive_socket.recv().decode("utf_16")
        message_string = u'{"MessageType":"COMMAND","ContentType":"NONE","Callback":"REPENT","Content":""}'
        message = Message()
        message = simplejson.loads(message_string, object_hook=_json_object_hook)
        print("Received by handler: " + message_string + "\n")
        #Cleanup for errors received from client should be somewhere here.
        #Something like this: self.RegisteredMethods[message.Content].__init__()
        MethodIdentifier = Procedure.Alphanumeric(message.Callback)
        if message.MessageType.upper() == "COMMAND":
            MethodUUID = Procedure.Alphanumeric(GenerateUuid())
            self.dInteractions[MethodUUID] = ProcedureFactory.Instantiate(self.dRegisteredProcedures[MethodIdentifier]) #??
        else:
            MethodUUID = MethodIdentifier
        
        WorkingMethod = self.RegisteredMethods[message.Callback]
        reply = WorkingMethod(message)
        reply_string = simplejson.dumps(reply.__dict__)
        alive_socket.send(reply_string)
        #reply = {'MessageType':"Set Event", 'ContentType':"None", 'Content':"ObjectCreated", 'SerializedContent':'"ObjectCreated"'}