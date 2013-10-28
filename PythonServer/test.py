from module1 import base, a

class subclass(base):
    def pp(self):
        print "Hello child"

print base.__subclasses__()

a()