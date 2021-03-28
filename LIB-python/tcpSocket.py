import socket

class TcpServer:

    def __init__(self, ip='127.0.0.1', port=3000):
        self.serversock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serversock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.serversock.bind((ip, port))
        self.serversock.listen(60)
        print('Waiting for connections...')
        self.clientsock, self.client_address = self.serversock.accept()
    
    def send(self, senddata):
        sendmsg = '{}\n'.format(senddata)
        self.clientsock.sendall(sendmsg.encode('utf-8'))
        print("send : " + sendmsg)
    
    def receive(self):
        rcv = self.clientsock.recv(1024).decode('utf-8')
        print("receive : " + rcv)
        return rcv
    
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
        print("send : " + sendmsg)
    
    def receive(self):
        rcv = self.client.recv(1024).decode('utf-8')
        print("receive : " + rcv)
        return rcv
    
    def close(self):
        self.client.close()