import numpy as np
import socket, base64, cv2

class TcpServer:

    def __init__(self, ip='127.0.0.1', port=3000):
        self.serversock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serversock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.serversock.bind((ip, port))
        self.serversock.listen(3600)
        print('Waiting for connections...')
        self.clientsock, self.client_address = self.serversock.accept()
    
    def send(self, senddata):
        sendmsg = '{}\n'.format(senddata)
        self.clientsock.sendall(sendmsg.encode('utf-8'))
    
    def send_img(self, img):
        encimg = cv2.imencode('.png', img)[1]
        self.send('data:image/png;base64,' + base64.b64encode(encimg).decode('utf-8'))
    
    def receive(self):
        rcv = ''
        while True:
            rcv += self.clientsock.recv(4096).decode('utf-8')
            if len(rcv) % 4096 != 0: break
        return rcv
    
    def receive_img(self):
        rcv = self.receive()
        if rcv.split(',')[0] != 'data:image/png;base64': return None
        rcv = rcv.split(',')[1]
        rcv = base64.b64decode(rcv)
        rcv = np.frombuffer(rcv,dtype=np.uint8)
        img = cv2.imdecode(rcv, cv2.IMREAD_UNCHANGED)
        return img

    def close(self):
        self.clientsock.close()
        self.serversock.close()

class TcpClient:

    def __init__(self, ip='127.0.0.1', port=3000):
        self.client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.client.connect((ip, port))
        print("Connection is OK")
    
    def send(self, senddata):
        sendmsg = '{}\n'.format(senddata)
        self.client.send(sendmsg.encode('utf-8'))

    def send_img(self, img):
        encimg = cv2.imencode('.png', img)[1]
        self.send('data:image/png;base64,' + base64.b64encode(encimg).decode('utf-8'))
    
    def receive(self):
        rcv = ''
        while True:
            rcv += self.client.recv(4096).decode('utf-8')
            if len(rcv) % 4096 != 0: break
        return rcv
    
    def receive_img(self):
        rcv = self.receive()
        if rcv.split(',')[0] != 'data:image/png;base64': return None
        rcv = rcv.split(',')[1]
        rcv = base64.b64decode(rcv)
        rcv = np.frombuffer(rcv,dtype=np.uint8)
        img = cv2.imdecode(rcv, cv2.IMREAD_UNCHANGED)
        return img

    def close(self):
        self.client.close()