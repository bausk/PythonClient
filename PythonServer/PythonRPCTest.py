# Created: 08.03.13
# License: MIT License
# from __future__ import unicode_literals

import sys
from sys import argv
import zmq
from zmq.eventloop import ioloop
from DraftSocketServer import Client
import msgpack

ALIVE_URL = 'tcp://127.0.0.1:5557'


def init():
    return argv


def linecreated(alive_socket, robot_socket, reply):
    assert (isinstance(alive_socket, zmq.sugar.socket.Socket))
    assert (isinstance(robot_socket, zmq.sugar.socket.Socket))

    # Received event from alive_socket
    # activate second socket, send data
    # like RPC call or like explicit command?
    # get reply
    # send confirmation to REP socket at alive_socket
    params_list = reply[2]

    message = Client.Handler.new_message(
        "1001",
        "METHOD",
        "CreateLine",
        "",
        *params_list
    )
    robot_socket.send(message)
    robot_reply = robot_socket.recv()
    alive_socket.send(message)



def rpc(alive_socket, robot_socket, reply):
    assert (isinstance(alive_socket, zmq.sugar.socket.Socket))
    message = msgpack.packb(
        (
            {"message_id": "1001",
             "method": "GET",
             "namespace": "DocumentManager"},
            "MdiActiveDocument",
            []
        ),
    )
    alive_socket.send(message)
    byte_reply = alive_socket.recv()
    string_reply = msgpack.unpackb(byte_reply)
    print string_reply
    message = msgpack.packb(
        (
            {"message_id": "1001",
             "method": "GET",
             "namespace": string_reply[1]},
            "Editor",
            []
        )
    )

    alive_socket.send(message)
    byte_reply = alive_socket.recv()
    string_reply = msgpack.unpackb(byte_reply)
    print string_reply
    message = msgpack.packb(
        (
            {"message_id": "1001",
             "method": "INVOKE",
             "namespace": string_reply[1]},
            "WriteMessage",
            ["\n Our test command works! Hello from CPython!"]
        )
    )
    alive_socket.send(message)
    byte_reply = alive_socket.recv()
    string_reply = msgpack.unpackb(byte_reply)
    print string_reply

    message = msgpack.packb(
        (
            {"message_id": "1001",
             "method": "END"
             },
            "",
            []
        )
    )
    alive_socket.send(message)

def main():
    # script, filename = init()
    print("Draftsocket RPC server starting...\n")
    thismodule = sys.modules[__name__]
    context = zmq.Context()
    Interaction = Client.Handler(thismodule, context)

    socket = context.socket(zmq.REP)
    socket.bind(ALIVE_URL)

    io_loop = ioloop.IOLoop.instance()
    io_loop.add_handler(socket, Interaction.receive_loop, io_loop.READ)

    print("Started IO loop.\n")
    io_loop.start()
    print("Work complete.\n")


if __name__ == '__main__':
    main()