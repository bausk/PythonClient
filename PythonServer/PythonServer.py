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

def handler(alive_socket, *args, **kwargs):
    message = alive_socket.recv()
    reply = message.decode("utf_16")
    message_object = simplejson.loads(reply)
    
    prompt = u"Received entity information\n"
    print(prompt.encode(sys.stdout.encoding))

    message = {'MessageType':"None", 'ContentType':"None", 'Content':"OK"}
    message_blob = simplejson.dumps(message, encoding = "UTF-8")#.encode(encoding = "ASCII")
    #message_blob = "111aaa".encode(encoding = "UTF-8")
    #message_blob = "{'MessageType':'None', 'ContentType':'None', 'Content':'OK'}".encode(encoding = "UTF-16")
    alive_socket.send(message_blob)
    message_blob = simplejson.dumps(message, encoding = "UTF-8")
    #alive_socket.send_json(message_blob)

def main():
    script, filename = init()

    print("Shadowbinder server starting...\n")
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)
    io_loop = ioloop.IOLoop.instance()
    io_loop.add_handler(socket, handler, io_loop.READ)
    io_loop.start()
    
if __name__ == '__main__':
    main()

