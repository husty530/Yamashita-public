#include "TcpSocket.h"

TcpSocket::TcpSocket(Mode mode, const char* ip, int port)
{
    if (mode == Server)
    {
        srcSock = socket(AF_INET, SOCK_STREAM, 0);
        srcAddr.sin_family = AF_INET;
        srcAddr.sin_port = htons(port);
        srcAddr.sin_addr.s_addr = INADDR_ANY;
        bind(srcSock, (struct sockaddr*)&srcAddr, sizeof(srcAddr));
        std::cout << "Waiting for connection ..." << std::endl;
        listen(srcSock, 5);
        socklen_t len = sizeof(dstAddr);
        dstSock = accept(srcSock, (struct sockaddr*)&dstAddr, &len);
        std::cout << "Connected." << std::endl;
    }
    else
    {
        dstSock = socket(AF_INET, SOCK_STREAM, 0);
        dstAddr.sin_family = AF_INET;
        dstAddr.sin_port = htons(port);
        dstAddr.sin_addr.s_addr = inet_addr(ip);
        connect(dstSock, (struct sockaddr*)&dstAddr, sizeof(dstAddr));
        std::cout << "Connected." << std::endl;
    }
}

void TcpSocket::Send(const char* msg)
{
    send(dstSock, msg, strlen(msg), 0);
}

char* TcpSocket::Receive()
{
    memset(buf, 0, sizeof(buf));
    recv(dstSock, buf, sizeof(buf), 0);
    return buf;
}

void TcpSocket::Close()
{
    closesocket(dstSock);
    WSACleanup();
}