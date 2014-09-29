from ecdsa.numbertheory import modular_exp

__author__ = 'bausk'
import msgpack


class Handler(object):

    def __init__(self, module_name):
        self.workers_module_name = module_name

    def alphanumeric(self, string):
        return "".join([x if x.isalnum() else "" for x in string.lower()])

    def receive_loop(self, alive_socket, *args, **kwargs):

        self.socket = alive_socket
        byte_reply = alive_socket.recv()
        string_reply = msgpack.unpackb(byte_reply)

        a = getattr(self.workers_module_name, self.alphanumeric(string_reply[1]))
        a(alive_socket)
