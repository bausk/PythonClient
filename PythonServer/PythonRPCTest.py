# Created: 08.03.13
# License: MIT License
# from __future__ import unicode_literals
__author__ = "Alex Bausk <bauskas@gmail.com>"

from DraftSocketServer.Message import Message, MessageFactory
from DraftSocketServer import Protocol
import sys
from sys import argv
import zmq
from DraftSocketServer.MessageServer import Handler
from DraftSocketServer import Client
import msgpack

ALIVE_URL = 'tcp://127.0.0.1:5557'


def init():
    return argv


def rpc(alive_socket):
    assert (isinstance(alive_socket, zmq.sugar.socket.Socket))
    message = msgpack.packb(
        (
            {"message_id": "1001",
             "method": "GET",
             "namespace": "DocumentManager"},
            "MdiActiveDocument",
            []
        ),
        #use_bin_type=True
    )
    # msgpack.pack()
    #message = msgpack.packb(message, use_bin_type=True)
    alive_socket.send(message)

    byte_reply = alive_socket.recv()
    string_reply = msgpack.unpackb(byte_reply)
    

    #messagestr = MessageFactory.Write("Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument")


def main():
    # script, filename = init()
    print("Draftsocket server starting...\n")
    thismodule = sys.modules[__name__]
    Interaction = Client.Handler(thismodule)

    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)
    io_loop = zmq.eventloop.ioloop.IOLoop.instance()
    io_loop.add_handler(socket, Interaction.receive_loop, io_loop.READ)

    print("Started IO loop.\n")
    io_loop.start()
    print("Work complete.\n")


if __name__ == '__main__':
    main()