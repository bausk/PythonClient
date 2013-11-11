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
from MessageServer import Message, MessageFactory, Handler, Procedure, Protocol, state, AutoCAD
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

    @state(0)
    def state0(self, reply):

        message = MessageFactory.GetUserString(prompt = "Hello AutoCAD!")
        message.Parameters["AllowSpaces"] = False
        message.Finalize()
        return message

class Inform(Procedure):
    @state(0)
    def state0(self, reply):
        message = MessageFactory.Write("Hello AutoCAD")
   
        message.Finalize()
        return message

class ServerSE(Procedure):

    @state(0)
    def state0(self, reply):
        prompt1 = AutoCAD.GetPromptEntityOptions(Prompt = "Choose first entity")
        prompt2 = AutoCAD.GetPromptEntityOptions(Prompt = "Choose second entity")
        PromptEntityOptions = [prompt1, prompt2]
        message = MessageFactory.GetEntity(PromptEntityOptions)
        return message

    @state(1)
    def state1(self, reply):
        self.entity1, self.entity2 = MessageFactory.ParseReply(MessageFactory.GetObjectID)
        message = MessageFactory.GetObjectID("Hello AutoCAD")
   
        return message

    @state(2)
    def state2(self, reply):
        message = MessageFactory.GetObjectID("Hello AutoCAD")
   
        message.Finalize()
        return message


def main():
    script, filename = init()
    print("Shadowbinder server starting...\n")
    Interaction = Handler()

    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)
    io_loop = ioloop.IOLoop.instance()
    io_loop.add_handler(socket, Interaction.HandleReceiveLoop, io_loop.READ)

    print("Started IO loop.\n")
    io_loop.start()
    print("Work complete.\n")
    
if __name__ == '__main__':
    main()