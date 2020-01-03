# -*- coding:utf-8 -*-
import shelve

s=shelve.open("user1.db")

try:
    print s['vivid']['chargerBulletCount']
    print s['vivid']['bulletCount']
    print s['vivid']['playerHealth']
    print s['vivid']['weaponInfo']
finally:
    s.close()