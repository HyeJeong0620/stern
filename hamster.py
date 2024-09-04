import socket
from roboid import *
import time
import threading

hamster = Hamster()

hamster.leds(1, 1)

local_address = ('0.0.0.0', 11001)  # 클라이언트의 로컬 IP와 포트 설정
server_address = ('127.0.0.1', 11000)  # 서버 IP와 포트 설정

# Create a UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock.bind(local_address)

def black():
    while True:
        print(hamster.right_floor())
        print(hamster.left_floor())
        if hamster.right_floor() < 20 or hamster.left_floor() < 20:
            sock.sendto("black".encode(), server_address)
            hamster.wheels(-100, -100)
            hamster.leds(4, 4)
            time.sleep(0.1)
            hamster.wheels(0, 0)
            hamster.leds(0, 0)
        time.sleep(0.1)  # Avoid busy-waiting

def udp_client():
    try:
        while True:
            # Receive data from server
            data, addr = sock.recvfrom(4096)
            print(data.decode())
            # Check floor sensor values
            if data.decode() == "stop":
                hamster.wheels(0, 0)
                hamster.leds(1, 1)
            elif data.decode() == "go":
                hamster.leds(2, 2)
                hamster.wheels(100, 100)
            elif data.decode() == "back":
                hamster.wheels(-100, -100)
                hamster.leds(4, 4)
            elif data.decode() == "left":
                hamster.wheels(-50, 50)
                hamster.leds(2, 4)
            elif data.decode() == "right":
                hamster.wheels(50, -50)
                hamster.leds(4, 2)
    except KeyboardInterrupt:
        print('Closing socket')
    finally:
        sock.close()

if __name__ == '__main__':
    thread1 = threading.Thread(target=black, args=())
    thread1.start()
    udp_client()
