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
from MessageServer import Message, Handler, Procedure, Protocol, state
#from test_decorator_tracker import *

ALIVE_URL = 'tcp://127.0.0.1:5556'

#class Message(object):
#    def __init__(self, Action, ContentType, Content):
#        self.Action = Action
#        self.ContentType = ContentType
#        self.Content = Content
#    def as_message(dct):
#        return Message(dct['Action'], dct['ContentType'], dct['Content'])

#def as_message(dct):
#    return Message(dct['Action'], dct['ContentType'], dct['Content'])

def init():
    return argv

class Handshake(Procedure):

    #def __call__( self, message = Message() ):
    #    reply = self.StateDict[self.CurrentState](message)
    #    return reply

    @state(0)
    def state0(self, reply):
        #message = Message(Action = Protocol.ServerAction.REQUEST_USER_INPUT)
        message = Handler.NewGetUserStringMessage("Hello AutoCAD")
        message.Parameters["AllowSpaces"] = False
        message.Finalize()
        return message

#    @state(1)
#    def state1(self, message):
#        reply = message #this is a stub
#        #do actual work with user input supposedly received in message
#        return reply

class Inform(Procedure):

    #def __call__( self, message = Message() ):
    #    reply = self.StateDict[self.CurrentState](message)
    #    return reply

    @state(0)
    def state0(self, reply):
        #message = Message(Action = Protocol.ServerAction.REQUEST_USER_INPUT)
        message = Handler.NewWriteMessage("Hello AutoCAD")
        message.Finalize()
        return message

#    @state(1)
#    def state1(self, message):
#        reply = message #this is a stub
#        #do actual work with user input supposedly received in message
#        return reply

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
    Interaction = Handler()

    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)
    io_loop = ioloop.IOLoop.instance()
    io_loop.add_handler(socket, Interaction.Handler, io_loop.READ)
    #Interaction.Handler(socket)
    print("Started IO loop.\n")
    io_loop.start()
    print("Work complete.\n")
    
if __name__ == '__main__':
    main()

#def