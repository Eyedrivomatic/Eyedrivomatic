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
const char SettingName_InvertX[] PROGMEM = "INVERT_X";
const char SettingName_CenterPosY[] PROGMEM = "CENTER_Y";
const char SettingName_MinPosY[] PROGMEM = "MIN_Y";
const char SettingName_MaxPosY[] PROGMEM = "MAX_Y";
const char SettingName_InvertY[] PROGMEM = "INVERT_Y";
const char SettingName_SwitchDefault[] PROGMEM = "SWITCH";

struct SettingsClass
{
public:
	char CheckValue[9];
	uint8_t Version;

	uint8_t CenterPos_X;
	uint8_t MinPos_X;
	uint8_t MaxPos_X;

	uint8_t CenterPos_Y;
	uint8_t MinPos_Y;
	uint8_t MaxPos_Y;

	bool DefaultSwitchStates[3]; 

	bool    Invert_X;
	bool    Invert_Y;

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

