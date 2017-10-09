// VersionInfo.h

#ifndef _VERSIONINFO_h
#define _VERSIONINFO_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#define VERSION_MAJOR 2
#define VERSION_MINOR 0
#define VERSION_BUILD 1

class VersionInfo
{
public:
	static const char Application[];
	static const char Version[];
};
#endif

