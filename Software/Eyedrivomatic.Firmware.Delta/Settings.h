//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


// Settings.h

#ifndef _SETTINGS_h
#define _SETTINGS_h

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

const char SettingName_CenterPosX[] PROGMEM = "CENTER_X";
const char SettingName_MinPosX[] PROGMEM = "MIN_X";
const char SettingName_MaxPosX[] PROGMEM = "MAX_X";
const char SettingName_CenterPosY[] PROGMEM = "CENTER_Y";
const char SettingName_MinPosY[] PROGMEM = "MIN_Y";
const char SettingName_MaxPosY[] PROGMEM = "MAX_Y";
const char SettingName_SwitchDefault[] PROGMEM = "SWITCH";

struct SettingsClass
{
public:
	char CheckValue[9];
	uint8_t Version;

	double CenterPos_X;
	double MinPos_X;
	double MaxPos_X;

	double CenterPos_Y;
	double MinPos_Y;
	double MaxPos_Y;

	bool DefaultSwitchStates[3]; 

public:
	// attempt to read the device settings from memory.
	// if the settings are not available, then set defaults.
	void init();

	// save the device settings to memory.
	void save();

	// load default values.
	void reset();

protected:
	// upgrade settings based on the version number.
	void upgrade();
};

extern SettingsClass Settings;

#endif

