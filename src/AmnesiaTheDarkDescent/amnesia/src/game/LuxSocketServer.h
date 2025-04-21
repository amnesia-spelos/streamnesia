#ifndef LUX_SOCKET_SERVER_H
#define LUX_SOCKET_SERVER_H

#include "LuxBase.h"
#include "LuxWinSocketInit.h"

class cLuxSocketServer : public iLuxUpdateable
{
public:
    cLuxSocketServer();
    ~cLuxSocketServer();

    void Update(float afTimeStep);
	void SendMessage(const tString& message);
	void SetConnectionSettings(const tString& host, int port);
	const tString& GetHost() const { return mHost; }
	int GetPort() const { return mPort; }
private:
	SOCKET mListenSocket;
	SOCKET mClientSocket;
	
	tString mHost;
	int mPort;

	bool InitSocket();
	void ShutdownSocket();
};

#endif
