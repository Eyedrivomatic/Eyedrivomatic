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


#include "Settings.h"
#include "State.h"
#include "LoggerService.h"

#include <EEPROM.h>

const char EEPROMCheckValue[] PROGMEM = "EYEDRIVE";
#define EEPROM_SETTINGS_VERSION 3

void SettingsClass::init()
{
	EEPROM.get(0, *this);
	if (strcmp_P(CheckValue, EEPROMCheckValue) != 0)
	{
		reset();
	}

	if (Version < EEPROM_SETTINGS_VERSION)
	{
		upgrade();
	}
}

void SettingsClass::reset()
{
	strcpy_P(CheckValue, EEPROMCheckValue);
	Version = EEPROM_SETTINGS_VERSION;
	CenterPos_X = 0;
	MinPos_X = HARDWARE_MIN_X;
	MaxPos_X = HARDWARE_MAX_X;
	Invert_X = true;
	CenterPos_Y = 0;
	MinPos_Y = HARDWARE_MIN_Y;
	MaxPos_Y = HARDWARE_MAX_Y;
	Invert_Y = false;

	DefaultSwitchStates[HardwareSwitch::Switch1] = false;
	DefaultSwitchStates[HardwareSwitch::Switch2] = false;
	DefaultSwitchStates[HardwareSwitch::Switch3] = false;

	save();
}

void SettingsClass::save()
{
	EEPROM.put(0, *this);
	LoggerService.info_P(PSTR("Settings Saved."));
}

void SettingsClass::upgrade()
{
	if (Version < 3)
	{
		DefaultSwitchStates[HardwareSwitch::Switch1] = false;
		DefaultSwitchStates[HardwareSwitch::Switch2] = false;
		DefaultSwitchStates[HardwareSwitch::Switch3] = false;
	}

	if (Version < 4)
	{
		Invert_X = true;
		Invert_Y = false;
	}

	Version = 4;

	save();
}

SettingsClass Settings;

