__author__ = 'bausk'
import win32com.client
import math
from contracts import contract

# win32com.client.Dispatch("RobotOM")

@contract
def demo(a, b, c):
    """
    :rtype : float
    :type c: int, >0
    :type b: int, >0
    :type a: int, >0
    """

    d = math.sqrt(a + b + c)
    return d

demo(1, 2, 3)
demo(1, 2, 3)