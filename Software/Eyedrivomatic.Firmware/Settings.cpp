#include "Settings.h"
#include "State.h"

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
	CenterPos_X = 90;
	MinPos_X = 60;
	MaxPos_X = 120;
	CenterPos_Y = 90;
	MinPos_Y = 60;
	MaxPos_Y = 120;

	DefaultSwitchStates[HardwareSwitch::Switch1] = false;
	DefaultSwitchStates[HardwareSwitch::Switch2] = false;
	DefaultSwitchStates[HardwareSwitch::Switch3] = false;

	save();
}

void SettingsClass::save()
{
	EEPROM.put(0, this);
}

void SettingsClass::upgrade()
{
	if (Version < 3)
	{
		DefaultSwitchStates[HardwareSwitch::Switch1] = false;
		DefaultSwitchStates[HardwareSwitch::Switch2] = false;
		DefaultSwitchStates[HardwareSwitch::Switch3] = false;
	}
	Version = 3;
}

SettingsClass Settings;

