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
private:
	SOCKET mListenSocket;
	SOCKET mClientSocket;

	bool InitSocket();
	void ShutdownSocket();
};

#endif
