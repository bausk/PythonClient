from ecdsa.numbertheory import modular_exp

__author__ = 'bausk'
import msgpack
import zmq

ALIVE_URL2 = 'tcp://127.0.0.1:5558'


class Handler(object):
    def __init__(self, module_name, context):
        self.workers_module_name = module_name
        self.context = context
        self.robot_socket = context.socket(zmq.REQ)
        self.robot_socket.bind(ALIVE_URL2)

    def alphanumeric(self, string):
        return "".join([x if x.isalnum() else "" for x in string.lower()])

    def receive_loop(self, alive_socket, *args, **kwargs):
        self.socket = alive_socket
        byte_reply = alive_socket.recv()
        string_reply = msgpack.unpackb(byte_reply)
        print string_reply
        a = getattr(self.workers_module_name, self.alphanumeric(string_reply[1]))
        a(alive_socket, self.robot_socket, string_reply)

    @staticmethod
    def new_message(ident, method, header, namespace, *args):
        return msgpack.packb(
            (
                {
                "message_id": ident,
                "method": method,
                "namespace": namespace
                },
            header,
            list(args))
            )