class ChildTracker(type):
  def __new__(cls, name, bases, dict_):
    new_class = type.__new__(cls, name, bases, dict_)
    # Check if this is the tracking class
    if '__metaclass__' in dict_ and dict_['__metaclass__']==ChildTracker:
      new_class.child_classes = {}
    else:
      # Add the new class to the set
      bases[0].child_classes[name] = new_class
      #cls
    return new_class


class BaseClass(object):
    __metaclass__ = ChildTracker
    bbb = "fathah"
    def __init__(self):
        self.buehuehue = "huehuehue"
    def aaa(self):
        self.buehuehue += "... hehe"


class Child1(BaseClass):
    kkk = "child"
    bbb = "child"
    def __init__(self):
        self.uhuehue = "uhuuhu"

class Child2(BaseClass):
    pass



print BaseClass.child_classes.keys()
ke = Child2()
ek = Child1()
print ke
print ek