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

def _json_object_hook(d): return namedtuple('Message', d.keys())(*d.values())

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
    Procedures = {}
    def __init__ (self):
        self.Procedures[self.__class__.__name__] = self.__class__
    

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
