#ifndef LUX_WINSOCKET_INIT_H
#define LUX_WINSOCKET_INIT_H

#define WIN32_LEAN_AND_MEAN
#define NOMINMAX

#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")

#ifdef SendMessage
#undef SendMessage
#endif

#endif
