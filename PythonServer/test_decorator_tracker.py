from module1 import base, a
from functools import wraps
from types import MethodType

#a variety of decorators
def testlogger1(func):
    @wraps(func)
    def with_logging(*args, **kwargs):
        print "Entering %s.%s" % (args[0].__class__.__name__, func.__name__)
        return func(*args, **kwargs)
    return with_logging

from functools import wraps

def state(statenum):
    def decorator (f):
        f.func_dict['state'] = statenum
        @wraps(f)   # In order to preserve docstrings, etc.
        def wrapped (self, *args, **kwargs):
            f.func_dict['state'] = statenum
            return f(self, *args, **kwargs)
        return wrapped
    return decorator

#Argument passed to decorator
def myDecorator(logIt):
    def actualDecorator(test_func):
        @wraps(test_func)
        def wrapper(*args, **kwargs):
            if logIt:
                print "Calling Function: " + test_func.__name__
            return test_func(*args, **kwargs)
        return wrapper
    return actualDecorator

#Same using class
class decoratorWithArguments(object):
    def __init__(self, arg1):
        print "Inside __init__()"
        self.arg1 = arg1

    def __call__(self, f):
        """
        If there are decorator arguments, __call__() is only called
        once, as part of the decoration process! You can only give
        it a single argument, which is the function object.
        """
        print "Inside __call__()", self.arg1
        @wraps(f)
        def wrapped_f(*args):
            f(*args)
            print "After f(*args)"
        return wrapped_f

#metaclass as a .__subclasses__ black magic
#child class tracker
class ChildTracker(type):
    """ Usage:
    class BaseClass(object):
    __metaclass__ = ChildTracker
    """
    def __new__(cls, name, bases, dict_):
        new_class = type.__new__(cls, name, bases, dict_)
        # Check if this is the tracking class
        if '__metaclass__' in dict_ and dict_['__metaclass__']==ChildTracker:
            new_class.child_classes = {}
        else:
            # Add the new class to the set
            new_class.child_classes[name] = new_class
        return new_class

#minimal method decorator
#http://stackoverflow.com/questions/12589522/minimal-decorator-for-class-method
def states(*states):
    def decorator (f):
        @wraps(f)   # In order to preserve docstrings, etc.
        def wrapped(self, *args, **kwargs):
            if self.state not in states:
                raise error
            return f(self, *args, **kwargs)
        return wrapped
    return decorator

#noice good decorator
#use by inheritance
class method_decorator(object):

    def __init__(self, func, obj=None, cls=None, method_type='function'):
        # These defaults are OK for plain functions
        # and will be changed by __get__() for methods once a method is dot-referenced.
        self.func, self.obj, self.cls, self.method_type = func, obj, cls, method_type

    def __get__(self, obj=None, cls=None):
        # It is executed when decorated func is referenced as a method: cls.func or obj.func.

        if self.obj == obj and self.cls == cls:
            return self # Use the same instance that is already processed by previous call to this __get__().

        method_type = (
            'staticmethod' if isinstance(self.func, staticmethod) else
            'classmethod' if isinstance(self.func, classmethod) else
            'instancemethod'
            # No branch for plain function - correct method_type for it is already set in __init__() defaults.
        )

        return object.__getattribute__(self, '__class__')( # Use specialized method_decorator (or descendant) instance, don't change current instance attributes - it leads to conflicts.
            self.func.__get__(obj, cls), obj, cls, method_type) # Use bound or unbound method with this underlying func.

    def __call__(self, *args, **kwargs):
        return self.func(*args, **kwargs)

    def __getattribute__(self, attr_name): # Hiding traces of decoration.
        if attr_name in ('__init__', '__get__', '__call__', '__getattribute__', 'func', 'obj', 'cls', 'method_type'): # Our known names. '__class__' is not included because is used only with explicit object.__getattribute__().
            return object.__getattribute__(self, attr_name) # Stopping recursion.
        # All other attr_names, including auto-defined by system in self, are searched in decorated self.func, e.g.: __module__, __class__, __name__, __doc__, im_*, func_*, etc.
        return getattr(self.func, attr_name) # Raises correct AttributeError if name is not found in decorated self.func.

    def __repr__(self): # Special case: __repr__ ignores __getattribute__.
        return self.func.__repr__()

#implementation
class my_decorator(method_decorator):
    def __call__(self, *args, **kwargs):
        print('Calling {method_type} {method_name} from instance {instance} of class {class_name} from module {module_name} with args {args} and kwargs {kwargs}.'.format(
            method_type=self.method_type,
            method_name=self.__name__,
            instance=self.obj,
            class_name=(self.cls.__name__ if self.cls else None),
            module_name=self.__module__,
            args=args,
            kwargs=kwargs,
        ))
        return method_decorator.__call__(self, *args, **kwargs)
