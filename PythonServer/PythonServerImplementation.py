# Created: 08.03.13
# License: MIT License
#from __future__ import unicode_literals
__author__ = "Alex Bausk <bauskas@gmail.com>"

from sys import argv
import zmq
from zmq.eventloop import ioloop

from DraftSocketServer.MessageServer import Handler, Procedure, state
from DraftSocketServer.Message import Message
from DraftSocketServer.Autocad import Payload
from DraftSocketServer.Autocad import AcadUtility as Utility
from DraftSocketServer.Autocad.AcadUtility import AutocadMessageFactory as MessageFactory

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
        message = MessageFactory.GetUserStrings("Hello AutoCAD! Gimme one:", "Gimme two!")
        message.Parameters["AllowSpaces"] = True
        return message

    @state(1)
    def state1(self, reply):
        print(reply)
        message = MessageFactory.Write("OK")
        message.Terminate()
        return message


class Inform(Procedure):
    @state(0)
    def state0(self, reply):
        message = MessageFactory.Write("Hello AutoCAD\n", "Nice!")
        message.Terminate()
        return message

class Inform2(Procedure):
    @state(0)
    def state0(self, reply):
        message = MessageFactory.Write("Hello AutoCAD\n", "Nice!")
        message.Finalize()
        return message
    @state(1)
    def state1(self, reply):
        return MessageFactory.Termination()

class TestTransaction(Procedure):

    @state(0)
    def state0(self, reply):
        return MessageFactory.Write("\nState 0\n")

    @state(1)
    def state1(self, reply):
        return MessageFactory.StartTransaction()

    @state(2)
    def state2(self, reply):
        return MessageFactory.Write("\nState 2\n")

    @state(3)
    def state3(self, reply):
        prompt1 = Utility.GetEntityOptions(Prompt = "\nChoose first entity", Name = "obj1")
        Options = [prompt1]
        msg1 = MessageFactory.GetEntity(Options)
        msg2 = MessageFactory.CommitTransaction()
        msg3 = MessageFactory.Write("\nState 3\n")
        msg4 = MessageFactory.StartTransaction()
        return MessageFactory.Batch(msg1, msg2, msg3, msg4)

    @state(4)
    def state4(self, reply):
        return MessageFactory.Batch(
                                    MessageFactory.Write("\nState 4\n"),
                                    MessageFactory.CommitTransaction(),
                                    )

    @state(5)
    def state5(self, reply):
        return MessageFactory.Batch(
                                    MessageFactory.StartTransaction(),
                                    MessageFactory.Write("\nState 5\n"),
                                    MessageFactory.CommitTransaction(),
                                    )
    @state(6)
    def state6(self, reply):
        msg = MessageFactory.StartTransaction()
        msg.Terminate()
        return msg


class ServerSE(Procedure):
    @state(0)
    def state0(self, reply):
        prompt1 = Utility.GetEntityOptions(Prompt = "\nChoose first entity", Name = "obj1")
        prompt2 = Utility.GetEntityOptions(Prompt = "\nChoose second entity", Name = "obj2")
        Options = [prompt1, prompt2]
        message = MessageFactory.GetEntity(Options)
        return message

    @state(1)
    def state1(self, reply = Message()):
        self.entity1, self.entity2 = Payload.GetEntities(reply)
        message1 = MessageFactory.StartTransaction()
        message2 = MessageFactory.GetObjectForRead(self.entity1.ObjectId, self.entity2.ObjectId)
        prompt1 = Utility.GetKeywordOptions(
                                            Prompt = "\nSwap their identities?",
                                            Keywords = ["Yes", "No"],
                                            Default = "Yes",
                                            AllowArbitraryInput = False,
                                            Name = "result"
                                            )
        message3 = MessageFactory.GetKeywords(prompt1)
        message4 = self.entity1.SwapIdWith(self.entity2.ObjectId)
        
        batchmessage = MessageFactory.Batch(message1, message2, message3)
        return batchmessage

    @state(2)
    def state2(self, reply = Message()):
        msg, msg2 = reply.UnpackBatch()
        message1 = MessageFactory.Write("OK")
        message2 = MessageFactory.CommitTransaction()
        message2.Terminate()
        return message

def main():
    #script, filename = init()
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