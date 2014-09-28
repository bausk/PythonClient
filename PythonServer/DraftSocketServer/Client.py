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
        #mReply = Message()
        #mReply.__dict__ = simplejson.loads(stringReply)
        #print("Received by handler: " + stringReply + "\n")
        #Cleanup for errors received from client should be somewhere here.
        #MethodIdentifier = 'None'

        a = getattr(self.workers_module_name, self.alphanumeric(string_reply[1]))
        a(alive_socket)

        # try:
        #     MethodIdentifier = Alphanumeric(mReply.Callback)
        #     if mReply.Action.upper() == Protocol.ClientAction.CMD:
        #         MethodUUID = self.InstantiateProcedure(MethodIdentifier, Socket = alive_socket)
        #         self.dInstantiatedProcedures[MethodUUID].Uuid = MethodUUID
        #     else:
        #         MethodUUID = MethodIdentifier
        # except Exception, ex:
        #     mMessage = MessageFactory.Error(ex, MethodIdentifier, self.ErrorMessages)
        #
        # if mReply.Action.upper() != Protocol.CommonAction.TERMINATE and mReply.Status.upper() != Protocol.Status.FINISH:
        #     try:
        #         WorkerProcedure = self.dInstantiatedProcedures[MethodUUID]
        #         mMessage = WorkerProcedure(mReply) #actual call
        #     except Exception, ex:
        #         mMessage = MessageFactory.Error(ex, MethodIdentifier, self.ErrorMessages)
        #
        #     mMessage.Callback = MethodUUID #here? doubtful
        #     stringReply = simplejson.dumps(mMessage.__dict__)
        #     alive_socket.send(stringReply)
        #
        # else:
        #     #Invoked after reply is received for a TERM message.
        #     #Incoming action is a demand to terminate all work.
        #     #Kill the instance bound to incoming callback, exit
        #     try:
        #         del self.dInstantiatedProcedures[MethodUUID]
        #         if mReply.Status.upper() == Protocol.Status.FINISH:
        #             #Invoked when clients says its reply is the last message.
        #             #Don't bother sending anything, kill the instance, exit
        #             if WorkerProcedure in locals():
        #                 del WorkerProcedure
        #     except Exception, ex:
        #         print(ex.message)

        #Test string: '{"Status": "_ONHOLD", "ContentType": "NONE", "Parameters": [{"Prompt": "Choose first entity"}, {}], "Callback": "E7C2B6230C8647059ACEC108F957D3F5", "Action": "GET_ENTITY", "Payload": null}'