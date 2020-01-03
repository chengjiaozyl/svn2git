# -*- coding:utf-8 -*-

from util import *
import select
import shelve
import socket
import sys
import platform
import time
import datetime
import random
import threading
from my_exception import ExitException, IllegalInputException

class MyServer(object):
    def __init__(self,port=5000):
        self.sys=platform.system()
         #Windows下不支持select追踪发sys.stdin 
         #conn_list用于保存所有socket
        if self.sys == 'Windows':
            self.conn_list=[]
        else:
            self.conn_list=[sys.stdin]
        #login_client_list用于保存登陆用户
        self.login_client_list=[]
        #Advisable to keep it as an exponent of 2
        self.RECEIVE_SIZE=1024
        #用户缓存区
        self.app_buffer=''
        
        self.port=port
        
        self.SYS_FAKE_SOCK=0
        # sock -> username mapping, vital for messaging service
        self.clientsock_username={0:'system'}
        #
        self.users=shelve.open('user1.db',writeback=True)
        #清除所有用户的在线状态, 否则会出现服务器异常退出后, 有些用户仍然为状态在线
        for username in self.users.keys():
            self.users[username]['login_timestamp']=-1
        
        self.server_socket=socket.socket(socket.AF_INET,socket.SOCK_STREAM)

        self.server_socket.setsockopt(socket.SOL_SOCKET,socket.SO_REUSEADDR,1)

        self.server_socket.bind(("127.0.0.1",self.port))
        self.server_socket.listen(10)
        #server_socket提供新连接事件
        self.conn_list.append(self.server_socket)
        print("Server started on port : "+str(self.port))

    def loop(self):
        while True:
            try:
                #Get the list sockets which are ready to be read through select 
                read_sockets, write_sockets, error_sockets = select.select(self.conn_list, [], [])

                for sock in read_sockets:
                    #新连接
                    if sock==self.server_socket:
                        new_sock, new_addr=self.server_socket.accept()
                        # 先加入conn_list, 之后根据验证情况决定是否加入到client_list中
                        self.conn_list.append(new_sock)
                        print ("Client sock (%s:%s) connected" %new_addr)
                    elif self.sys != 'Windows' and sock==sys.stdin:
                        in_content = sys.stdin.readline()
                        if in_content.strip() in ('exit', 'quit'):
                            self.server_exit()
                        else:  # send it to client as test message
                            self.broadcast_content(self.SYS_FAKE_SOCK, self.login_client_list,
                                                   in_content)
                    #从客户端传递来的数据，处理数据
                    else:
                        data=sock.recv(self.RECEIVE_SIZE)
                        if(not data):
                            raise ExitException("没有数据，表示这个socket应该断开连接")
                        else:
                            self.app_buffer+=data
                            l, new_buffer=decode(self.app_buffer)
                            self.app_buffer=new_buffer
                            l=convert_key(l)
                            for d in l:
                                code=d['code']
                                #print(d)
                                if code=='exit' or code =='quit':
                                    raise ExitException("客户端请求关闭连接")
                                elif code in ("init","i"):
                                    response=encode({'code':'msg','content':"""menu Welcome to MyGameServer!\n
                                                                              =========command list========
                                                                              1. register username password
                                                                              2. login username password
                                                                              3. exit
                                                                              """})
                                    sock.send(response)
                                elif code in ("register","r"):
                                    self.do_register(sock,d)
                                elif code in ("login","l"):
                                    #登录后，在地图内创建一个敌人
                                    self.do_login(sock,d)
                                    #self.enermy_location_create(sock)
                                elif code in ("game_data_storage","gds"):
                                    self.do_game_data_storage(sock,d)
                                elif code in ("game_data_request","gdr"):
                                    self.do_game_data_request(sock)
                                elif code in ("game_enermy_data","ged"):
                                    self.do_game_enermy(sock,d)
            except ExitException as e:
                self.tackle_client_exit(sock,e.message)
                continue
            except IllegalInputException as e:
                response = encode({'code':'msg','content':e.message})
                sock.send(response)
            except KeyboardInterrupt:
                print "server_exit"
                self.server_exit()
            except BaseException as e:
                print ("base exception")
                break

    # 登陆函数
    def do_login(self, sock, data_dict):
        print ("登陆函数运行中")
        if not data_dict.__contains__('username') or not data_dict.__contains__('password'):
            raise IllegalInputException()

        login_username = data_dict['username']
        login_password = data_dict['password']
        print (login_username)
        print (login_password)
        if not self.users.__contains__(login_username):
            response = encode_content("failed! Username not found!")
            sock.send(response)
        elif self.users[login_username]['password'] != login_password:
            response = encode_content("failed! Wrong password for this username!")
            sock.send(response)
        elif self.users[login_username]['login_timestamp'] != -1:
            response = encode_content("failed! This account is already online")
            sock.send(response)
        else:  # successful
            response = encode({'code': 'login_successful', 'bulletCount': self.users[login_username]['bulletCount'],
                               'chargerBulletCount':  self.users[login_username]['chargerBulletCount'],
                               'playerHealth': self.users[login_username]['playerHealth'],
                               'weaponInfo':  self.users[login_username]['weaponInfo']
                               })
            print ("login_successful")
            sock.send(response)
            self.users[login_username]['login_timestamp'] = int(
                time.time())  # update login_timestamp
            self.login_client_list.append(sock)
            self.clientsock_username[sock] = login_username
        print ("完成登录")

    #注册函数
    def do_register(self, sock, data_dict):
        if not data_dict.__contains__('username') or not data_dict.__contains__('password'):
            raise IllegalInputException()
        reg_username = data_dict['username']
        reg_password = data_dict['password']
        if self.users.__contains__(reg_username):
            response = encode_content("fail Username used, please change your username!")
            sock.send(response)
        else:  # okay
            self.users[reg_username] = {}  # 这里得先创建一个dict, 才能赋值
            self.users[reg_username]['password'] = reg_password
            self.users[reg_username]['reg_timestamp'] = int(time.time())
            # self.users[reg_username]['total_time'] = 0  # init total_time
            self.users[reg_username]['login_timestamp'] = int(time.time())  # the most recent login_time
            self.users[reg_username]['score'] = 0
            self.users[reg_username]['health'] = 100
            # self.users[reg_username]['room'] = '-1'
            response = encode_content(
                "register_successful Succeeded! Your username is {0}, password is {1}".format(
                    reg_username, reg_password), "register_successful")
            sock.send(response)
            self.login_client_list.append(sock)
            self.clientsock_username[sock] = reg_username  # sock --> username
            self.users.sync()

    #游戏数据存储函数
    def do_game_data_storage(self, sock, data_dict):
        print ("进入游戏数据存储函数")
        if sock not in self.login_client_list:
            response = encode_content("You have not login yet!")
            sock.send(response)
        else:
            username = self.clientsock_username[sock]
            self.users[username]['bulletCount'] = int(data_dict['bulletCount'])
            self.users[username]['chargerBulletCount'] = int(data_dict['chargerBulletCount'])
            self.users[username]['playerHealth'] = float(data_dict['playerHealth'])
            self.users[username]['weaponInfo'] = str(data_dict['weaponInfo'])
            print (username)
            print (self.users[username]['bulletCount'])
            print (self.users[username]['chargerBulletCount'])

    #游戏中敌人生成创建处理函数
    def do_game_enermy(self, sock, data_dict):
        enermyHealth=data_dict['health']
        if enermyHealth <= 0:
            self.enermy_location_create(sock)

    #游戏数据请求返回函数
    def do_game_data_request(self, sock):
        print ("进入请求数据返回函数")
        username = self.clientsock_username[sock]
        bulletCount = self.users[username]['bulletCount']
        chargerBulletCount = self.users[username]['chargerBulletCount']
        playerHealth = self.users[username]['playerHealth']
        weaponInfo = self.users[username]['weaponInfo']
        sock.send(encode({'code':'game_player_data', 'bulletCount':str(bulletCount), 'chargerBulletCount':str(chargerBulletCount),
                          'playerHealth':str(playerHealth),
                          'weaponInfo':str(weaponInfo),
                          'username':username,
                          'wayPoint1X': str(-250),
                          'wayPoint1Z': str(30),
                          'wayPoint2X': str(-260),
                          'wayPoint2Z': str(30),
                          'wayPoint3X': str(-250),
                          'wayPoint3Z': str(40),
                          }))

    #广播函数
    def broadcast_content(self, sender_sock, recv_list, message_content, code='msg'):
        """
        谁给谁们说了什么, 类型是什么
        """
        # Do not send the message to server's socket and the client who has send us the message
        for sock in recv_list:
            print('recv_list: ' + str(recv_list))
            print('broadcasted content: ' + str(message_content) + ' to ' + str(self.clientsock_username[sock]))
            if sock != self.server_socket and sock != sender_sock and sock != sys.stdin and sock!=self.SYS_FAKE_SOCK:
                try:
                    response = encode(
                        {'code': code, 'content': message_content, 'sender': self.clientsock_username[sender_sock]})
                    sock.send(response)
                except Exception as e:
                    self.tackle_client_exit(sock, e.message)

    #正常客户端退出
    def tackle_client_exit(self, sock, msg="a sock exit..."):
        """an uniform client socket exit handling function"""
        print (msg)
        if self.clientsock_username.__contains__(sock):
            username = self.clientsock_username[sock]  # 计算本次在线时长(s), 并加到total_time中
            # self.users[username]['total_time'] = self.users[username]['total_time'] + (
            #     int(time.time()) - self.users[username]['login_timestamp'])
            self.users[username]['login_timestamp'] = -1  # 下线就该把login时间设置成-1
            self.users.sync()
            # self.rooms.sync()
            self.login_client_list.remove(sock)
            self.clientsock_username.pop(sock)
        self.conn_list.remove(sock)
        sock.close()

    #服务器退出
    def server_exit(self):
        global sock
        print('server admin order server to close...')
        for sock in self.conn_list:
            self.tackle_client_exit(sock)
        self.users.close()
        # self.rooms.close()
        sys.exit(0)

    #创建机器人生成点和巡逻路标位置
    def enermy_location_create(self,sock):
        #创建三个路标，敌人巡逻点
        position1_X=random.uniform(250,300)
        position1_Z=random.uniform(30,60)
        position2_X=position1_X+15
        position2_Z=position1_Z
        position3_X=position1_X
        position3_Z=position1_Z+15
        sock.send(encode({
            'code':'game_enermy_data',
            'wayPoint1X': str(position1_X),
            'wayPoint1Z': str(position1_Z),
            'wayPoint2X': str(position2_X),
            'wayPoint2Z': str(position2_Z),
            'wayPoint3X': str(position3_X),
            'wayPoint3Z': str(position3_Z),
        }))

if __name__ == "__main__":
    server = MyServer(5000)
    server.loop()              