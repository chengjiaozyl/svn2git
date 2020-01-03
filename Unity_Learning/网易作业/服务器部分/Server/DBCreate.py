# -*- coding:utf-8 -*-
import shelve

s=shelve.open("user1.db")

try:
    s["vivid"]={'password':"8888", 'bulletCount': 60, 'chargerBulletCount':5000, 'playerHealth': 100, 'weaponInfo':"武器: 1: Tps_Weapon_fal"}
finally:
    s.close()