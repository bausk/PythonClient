from module1 import base, a
from functools import wraps
from types import MethodType

#Lott's class instance factory
class Foobar_Collection( dict ):
    def __init__( self, *arg, **kw ):
        super( Foobar_Collection, self ).__init__( *arg, **kw )
    def Instantiate( self, *arg, **kw ):
        fb= Foobar( *arg, **kw )
        self[fb.name]= fb
        return fb




class TestCommand(TestProcedure):
    def init():
        pass

    @state(0)
    def state0():
        pass

    @state(1)
    def state1():
        pass