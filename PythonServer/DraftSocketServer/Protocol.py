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
        NAME = "NAME"

class AutoCADProtocol(Protocol):

    class AutocadAction:
        GET_KEYWORD = "GET_KEYWORD"
        GET_ENTITY_ID = "GET_ENTITY"
        MANIPULATE_DB = "TR_MANIPULATE_DB"
        REQUEST_USER_INPUT = "REQUEST_INPUT"
        GET_DB_OBJECTS = "TR_GET_OBJECTS"

        class DBObject:
            UpgradeOpen = "UPGRADEOPEN"
            DowngradeOpen = "DOWNGRADEOPEN"
            SwapIdWith = "SWAPIDWITH"


    class Local:
        Prompt = "PROMPT"
        Default = "DEFAULT"
        RejectMessage = "REJECTMESSAGE"
        AllowedClass = "ALLOWEDCLASS"
        AllowNone = "ALLOWNONE"
        Keywords = "KEYWORDS"
        AllowArbitraryInput = "ALLOWARBINPUT"
        ForRead = "FORREAD"
        ObjectID = "KEY_OBJECTID"
        TypeName = "KEY_TYPENAME"
        Handle = "KEY_HANDLE"