# Created: 08.03.13
# License: MIT License
#from __future__ import unicode_literals
__author__ = "Alex Bausk <bauskas@gmail.com>"

import sys
import simplejson
from sys import argv
import collections
import time
import zmq
from zmq.eventloop import ioloop
from MessageServer import Message, Handler, Handler2, Procedure

ALIVE_URL = 'tcp://127.0.0.1:5556'

#class Message(object):
#    def __init__(self, MessageType, ContentType, Content):
#        self.MessageType = MessageType
#        self.ContentType = ContentType
#        self.Content = Content
#    def as_message(dct):
#        return Message(dct['MessageType'], dct['ContentType'], dct['Content'])

#def as_message(dct):
#    return Message(dct['MessageType'], dct['ContentType'], dct['Content'])

def init():
    return argv

class Command_1(Procedure):
    def __init__( self ):
        self.CurrentState = 0
        self.StateDict = {
                          0: self.state0,
                          1: self.state1
                          }
        Procedure.__init__(self)
    def __call__( self, message = Message() ):
        reply = self.StateDict[self.CurrentState](message)
        return reply
    def state0(self, message):
        reply = Message(MessageType = "REQUEST_INPUT", Content = "STRING", Callback = "REPENT")
        #make reply, in which ask client for user input
        self.CurrentState += 1
        return reply
    def state1(self, message):
        reply = message #this is a stub
        #do actual work with user input supposedly received in message
        return reply

class OnCommandEnded(Procedure):
    def __init__( self ):
        self.rememberThis = {'a':0}
        Procedure.__init__(self)
    def __call__( self, arg1, arg2 ):
        # do something
        self.rememberThis['a'] = arg1
        return someValue
    def method1(self):
        pass

def main():
    script, filename = init()
    print("Shadowbinder server starting...\n")
    ExState = Handler2()

    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)
    io_loop = ioloop.IOLoop.instance()
    io_loop.add_handler(socket, ExState.handler, io_loop.READ)
    ExState.handler(socket)
    print("Started IO loop.\n")
    io_loop.start()
    print("Work complete.\n")
    
if __name__ == '__main__':
    main()

#def