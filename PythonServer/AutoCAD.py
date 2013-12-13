import MessageServer
from Message import Message, MessageFactory
from Protocol import AutoCADProtocol as Protocol

def GetEntityOptions(Prompt = None, AllowedClasses = None, RejectMessage = None, Name = None):
    dictionary = {}
    if AllowedClasses is not None:
        dictionary[Protocol.Local.AllowedClasses] = AllowedClasses
    if Prompt is not None:
        dictionary[Protocol.Local.Prompt] = Prompt
    if RejectMessage is not None:
        dictionary[Protocoll.Local.RejectMessage] = RejectMessage
    if Name is not None:
        dictionary[Protocol.Keywords.NAME] = Name
    return dictionary

def GetKeywordOptions(Prompt = None, Default = None, AllowInput = None, Keywords = None, Name = None):
    dictionary = {}
    if Prompt is not None:
        dictionary[Protocol.Local.Prompt] = Prompt
    if Default is not None:
        dictionary[Protocol.Local.Default] = Default
    if AllowInput is not None:
        dictionary[Protocol.Local.AllowArbitraryInput] = AllowInput
    if Keywords is not None and type(Keywords) is list:
        dictionary[Protocol.Local.Keywords] = Keywords
    else:
        dictionary[Protocol.Local.Keywords] = [Keywords]
    if Name is not None:
        dictionary[Protocol.Keywords.NAME] = Name
    return dictionary

class AutocadMessageFactory(MessageFactory):
    @classmethod
    def GetEntity(cls, prompt):
        """Request entity ID's with arguments as prompts.
        """
        msg = Message(Action = Protocol.AutocadAction.GET_ENTITY_ID, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = prompt)
        return msg
    @classmethod
    def GetKeywords(cls, prompt):
        """Request keyword prompts with arguments as prompts.
        """
        msg = Message(Action = Protocol.AutocadAction.GET_KEYWORD, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = prompt)
        return msg
    @classmethod
    def GetUserStrings(cls, *prompts):
        """Forms a message for single user input request. str Prompt is a command line message prompt.
        """
        msg = Message(Action = Protocol.AutocadAction.REQUEST_USER_INPUT, Status = Protocol.Status.ONHOLD, Parameters = {}, Payload = [a for a in prompts])
        return msg

    @classmethod
    def GetObjectForRead(cls, *ids):
        """
        """
        mes = Message(Action = Protocol.AutocadAction.GET_OBJECTS, Status = Protocol.Status.ONHOLD, Parameters = {Protocol.Local.ForRead: true}, Payload = [ids])