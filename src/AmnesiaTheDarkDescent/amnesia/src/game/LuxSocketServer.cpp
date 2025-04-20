#include "LuxSocketServer.h"

cLuxSocketServer::cLuxSocketServer()
    : iLuxUpdateable("LuxSocketServer")
{
	mListenSocket = INVALID_SOCKET;
    mClientSocket = INVALID_SOCKET;

	InitSocket();
    Log("cLuxSocketServer created!\n");
}

bool cLuxSocketServer::InitSocket()
{
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0)
    {
        Log("WSAStartup failed\n");
        return false;
    }

    mListenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (mListenSocket == INVALID_SOCKET)
    {
        Log("Socket creation failed\n");
        WSACleanup();
        return false;
    }

    u_long nonBlocking = 1;
    ioctlsocket(mListenSocket, FIONBIO, &nonBlocking);

    sockaddr_in service;
    service.sin_family = AF_INET;
    service.sin_addr.s_addr = inet_addr("127.0.0.1");
    service.sin_port = htons(5150);

    if (bind(mListenSocket, (SOCKADDR*)&service, sizeof(service)) == SOCKET_ERROR)
    {
        Log("Bind failed\n");
        closesocket(mListenSocket);
        WSACleanup();
        return false;
    }

    if (listen(mListenSocket, SOMAXCONN) == SOCKET_ERROR)
    {
        Log("Listen failed\n");
        closesocket(mListenSocket);
        WSACleanup();
        return false;
    }

    Log("Socket listening on 127.0.0.1:5150\n");
    return true;
}

void cLuxSocketServer::ShutdownSocket()
{
    if (mClientSocket != INVALID_SOCKET)
    {
        closesocket(mClientSocket);
        mClientSocket = INVALID_SOCKET;
    }

    if (mListenSocket != INVALID_SOCKET)
    {
        closesocket(mListenSocket);
        mListenSocket = INVALID_SOCKET;
    }

    WSACleanup();
}

cLuxSocketServer::~cLuxSocketServer()
{
	ShutdownSocket();
    Log("cLuxSocketServer destroyed!\n");
}

void cLuxSocketServer::Update(float afTimeStep)
{
}
