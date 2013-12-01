class Protocol(object):

    class ClientAction:
        CMD = "COMMAND"
        ERROR = "CLIENT_ERROR"
        EVENT = "EVENT"
        CONTINUE = "CONTINUE"
        CONSOLE = "CONSOLE"

    class ServerAction:
        SETEVENT = "SET_EVENT"
        WRITE = "WRITE_MESSAGE"
        REQUEST_USER_INPUT = "REQUEST_INPUT"
        REQUEST_SEVERAL_USER_INPUTS = "REQUEST_SEVERAL_INPUTS"
        MANIPULATE = "MANIPULATE_DB"
        TERMINATE = "TERMINATE_SESSION"
        GET_ENTITY_ID = "GET_ENTITY"
        TRANSACTION_START = "TRANSACTION_START"
        TRANSACTION_COMMIT = "TRANSACTION_COMMIT"
        TRANSACTION_ABORT = "TRANSACTION_ABORT"

    class CommonAction:
        TERMINATE = "TERMINATE_SESSION"

    class Status:
        FINISH = "_FINISH"
        OK = "_OK"
        ONHOLD = "_ONHOLD"
        #SERVER_ERROR = "_SERVERERROR" - Deprecated

    class Keywords:
        DEFAULT = "DEFAULT"
        OBJECT = "OBJECT"

    class PayloadTypes:
        STRING = "STRING"
        LIST = "LIST"
        DICT = "DICT"
        LISTOFSTRINGS = "LISTOFSTRINGS"

    #types
    #Types = {
    #            type(0):"INT",
    #            type(""):"STRING",
    #            type([]):"LIST",
    #            type({}):"DICTIONARY",
    #            object:"OBJECT",
    #            type(None):"NONE"
    #            }

