class Protocol(object):

    class ClientAction:
        CMD = "COMMAND"
        ERROR = "CLIENT_ERROR"
        EVENT = "EVENT"
        CONTINUE = "CONTINUE"
        CONSOLE = "CONSOLE"

    class ServerAction:
        WRITE = "WRITE"
        SETEVENT = "SET_EVENT"
        TRANSACTION_START = "TR_START"
        TRANSACTION_COMMIT = "TR_COMMIT"
        TRANSACTION_ABORT = "TR_ABORT"

    class CommonAction:
        TERMINATE = "CA_TERMINATE"
        BATCH = "CA_BATCH"

    class Status:
        FINISH = "_FINISH"
        OK = "_OK"
        ONHOLD = "_ONHOLD"
        TERMINATE = "_TERMINATE"

    class Keywords:
        DEFAULT = "DEFAULT"
        OBJECT = "OBJECT" #Deprecated

    class PayloadTypes:
        STRING = "STRING"
        LIST = "LIST"
        DICT = "DICT"
        LISTOFSTRINGS = "LISTOFSTRINGS"


class AutoCADProtocol(Protocol):

    class AutocadAction:
        GET_KEYWORD = "GET_KEYWORD"
        GET_ENTITY_ID = "GET_ENTITY"
        GETOBJECT = "TR_GET_OBJECT"
        MANIPULATE_DB = "TR_MANIPULATE_DB"
        REQUEST_USER_INPUT = "REQUEST_INPUT"
        GET_OBJECTS = "GET_OBJECTS"

        class DBObject:
            UpgradeOpen = "UPGRADEOPEN"
            DowngradeOpen = "DOWNGRADEOPEN"
            SwapIdWith = "SWAPIDWITH"


    class Local:

        Prompt = "PROMPT"
        Default = "DEFAULT"
        RejectMessage = "REJECTMESSAGE"
        AllowedClass = "ALLOWEDCLASS"
        Name = "NAME"
        AllowNone = "ALLOWNONE"
        Keywords = "KEYWORDS"
        AllowArbitraryInput = "ALLOWARBINPUT"

        ForRead = "FORREAD"

        ObjectID = "KEY_OBJECTID"
        TypeName = "KEY_TYPENAME"
        Handle = "KEY_HANDLE"

    #types
    #Types = {
    #            type(0):"INT",
    #            type(""):"STRING",
    #            type([]):"LIST",
    #            type({}):"DICTIONARY",
    #            object:"OBJECT",
    #            type(None):"NONE"
    #            }

