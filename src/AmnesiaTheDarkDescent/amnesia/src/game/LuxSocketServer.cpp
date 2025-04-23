#include "LuxSocketServer.h"
#include "LuxMap.h"
#include "LuxMapHandler.h"
#include "LuxPlayer.h"

cLuxSocketServer::cLuxSocketServer()
    : iLuxUpdateable("LuxSocketServer")
{
	mHost = "127.0.0.1";
	mPort = 5150;
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
	service.sin_addr.s_addr = inet_addr(mHost.c_str());
	service.sin_port = htons(mPort);

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

    Log("Socket listening on %s:%d\n", mHost.c_str(), mPort);
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

void cLuxSocketServer::Update(float afTimeStep)
{
	// Accept client if not already connected
    if (mClientSocket == INVALID_SOCKET)
    {
        sockaddr_in clientAddr;
        int addrLen = sizeof(clientAddr);
        SOCKET clientSocket = accept(mListenSocket, (SOCKADDR*)&clientAddr, &addrLen);

        if (clientSocket != INVALID_SOCKET)
        {
            Log("Client connected!\n");
            mClientSocket = clientSocket;

            SendMessage("Hello, from Amnesia: The Dark Descent!");
        }
    }

    // Handle incoming data
    if (mClientSocket != INVALID_SOCKET)
    {
        char buffer[8192];
        int bytesReceived = recv(mClientSocket, buffer, sizeof(buffer) - 1, 0);

        if (bytesReceived > 0)
        {
            buffer[bytesReceived] = '\0';

            // Strip trailing \r or \n
            for (int i = 0; buffer[i]; ++i)
            {
                if (buffer[i] == '\r' || buffer[i] == '\n')
                {
                    buffer[i] = '\0';
                    break;
                }
            }

            Log("Client says: %s\n", buffer);

            if (strcmp(buffer, "ping") == 0)
            {
				SendMessage("RESPONSE:ping:pong");
            }
            else if (strcmp(buffer, "getpos") == 0)
            {
                if (gpBase->mpMapHandler && gpBase->mpMapHandler->GetCurrentMap())
                {
                    cVector3f vPos = gpBase->mpPlayer->GetCharacterBody()->GetFeetPosition();
                    char response[128];
                    sprintf(response, "RESPONSE:getpos:%.2f, %.2f, %.2f", vPos.x, vPos.y, vPos.z);
                    SendMessage(response);
                }
                else
                {
                    SendMessage("RESPONSE:getpos:no map loaded");
                }
            }
			else if (strcmp(buffer, "getmap") == 0)
			{
				if (gpBase->mpMapHandler && gpBase->mpMapHandler->GetCurrentMap())
				{
					tString mapFile = gpBase->mpMapHandler->GetCurrentMap()->GetFileName();
					SendMessage("RESPONSE:getmap:" + mapFile);
				}
				else
				{
					SendMessage("RESPONSE:getmap:no map loaded");
				}
			}
            else if (strncmp(buffer, "exec:", 5) == 0)
            {
                const char* script = buffer + 5;
                if (gpBase->mpMapHandler && gpBase->mpMapHandler->GetCurrentMap())
                {
                    gpBase->mpMapHandler->GetCurrentMap()->RunScript(script);
					SendMessage("RESPONSE:exec:script executed");
                }
                else
                {
					SendMessage("RESPONSE:exec:no map loaded");
                }
            }
            else
            {
                SendMessage("WARNING:Unknown command");
            }
        }
        else if (bytesReceived == 0 || (bytesReceived == SOCKET_ERROR && WSAGetLastError() != WSAEWOULDBLOCK))
        {
            Log("Client disconnected.\n");
            closesocket(mClientSocket);
            mClientSocket = INVALID_SOCKET;
        }
    }
}

void cLuxSocketServer::SendMessage(const tString& message)
{
    if (mClientSocket != INVALID_SOCKET)
    {
        tString safeMessage = message;

        // Add newline if not already present
        if (safeMessage.empty() || safeMessage[safeMessage.length() - 1] != '\n')
            safeMessage += "\n";

        send(mClientSocket, safeMessage.c_str(), (int)safeMessage.length(), 0);
    }
}

void cLuxSocketServer::SetConnectionSettings(const tString& host, int port)
{
    mHost = host;
    mPort = port;

	Log("LuxSocketServer config changed: re-init\n");
	ShutdownSocket();
    InitSocket();
}

cLuxSocketServer::~cLuxSocketServer()
{
	ShutdownSocket();
    Log("cLuxSocketServer destroyed!\n");
}
