# -*- coding:utf-8 -*-

class ExitException(Exception):
    '''
    这个异常表示client正常退出，不是因为发生异常而导致的退出
    '''
    def __init__(self,message="Client exit!"):
        self.message=message

    def __repr__(self):
        return self.message
    
    def __str__(self):
        return self.__repr__()

class IllegalInputException(Exception):
    '''
    这个异常表示用户输入非法
    '''
    def __init(self,message="Illegal input!"):
        self.message=message

    def __repr__(self):
        return self.message

    def __str__(self):
        return self.__repr__()
        