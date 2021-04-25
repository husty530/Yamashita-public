#pragma once

#include <conio.h>
#include <iostream>
#include <sstream>
#include <stdio.h>
#include <winsock2.h>
#include <WS2tcpip.h>

#pragma comment( lib, "ws2_32.lib" )

class TcpSocket
{

private:

    int srcSock;
    int dstSock;
    struct sockaddr_in srcAddr;
    struct sockaddr_in dstAddr;
    char buf[256];

public:

    enum Mode { Server, Client };

    /// <summary>
    /// Establish TcpSocket Connection
    /// </summary>
    /// <param name="mode">Server or Client</param>
    /// <param name="ip">Host IP Adress</param>
    /// <param name="port">Port Number</param>
    TcpSocket(Mode mode, const char* ip = "127.0.0.1", int port = 3000);

    /// <summary>
    /// Send Message
    /// </summary>
    /// <param name="msg">Any Array, type of 'char*'</param>
    void Send(const char* msg);

    /// <summary>
    /// Receive Message
    /// </summary>
    /// <returns>Any Array, type of 'char*'</returns>
    char* Receive();

    /// <summary>
    /// Disconnect TcpSocket Connection
    /// </summary>
    void Close();

};