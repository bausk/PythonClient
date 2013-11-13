from MessageServer import MessageFactory

class AutoCAD(object):
 
 
     def Parse(cls, reply, func):
         args = cls.MethodsDict[func.func_name](cls, reply)
 
         return args
 
 
     def GetUserString(cls, reply):
         result = tuple(a for a in reply)
         return result
 
     MethodsDict = {
                    MessageFactory.GetUserString.func_name: GetUserString
                    }



aa = AutoCAD()
