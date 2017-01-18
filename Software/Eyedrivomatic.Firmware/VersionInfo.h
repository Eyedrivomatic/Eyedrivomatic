// VersionInfo.h

#ifndef _VERSIONINFO_h
#define _VERSIONINFO_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

class VersionInfo
{
public:
	static const char Application[];
	static const char Version[];
};
#endif

