class base(object):
    def p(self):
        print "Hello Base"

def a():
    print base.__subclasses__()