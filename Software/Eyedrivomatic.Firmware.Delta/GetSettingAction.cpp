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


// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "GetSettingAction.h"
#include "LoggerService.h"
#include "Settings.h"
#include "Response.h"

const char SettingsResponseFormatString[] PROGMEM = "SETTING: %s %s";
const char SettingsResponseFormatInt[] PROGMEM = "SETTING: %s %d";
const char SettingsResponseFormatDouble1[] PROGMEM = "SETTING: %s %3.1f";
const char SettingsResponseFormatPos[] PROGMEM = "SETTING: %s %3.1f,%3.1f";


#define LogErrorAndReturn(msg, ...) Response.SendResponse_P(msg, ##__VA_ARGS__); return;

struct GetSettingsAction
{
public:
	GetSettingsAction(const char * name, void(*f)(const char*))
	{
		settingName = name;
		getFunc = f;
	}
	const char * settingName;
	void(*getFunc)(const char*);
};

const char SettingName_All[] PROGMEM = "ALL";

const GetSettingsAction SettingsActions[] PROGMEM =
{
	GetSettingsAction(SettingName_Center, GetSettingActionClass::getCenter),
	GetSettingsAction(SettingName_MaxSpeed, GetSettingActionClass::getMaxSpeed),
	GetSettingsAction(SettingName_Orientation, GetSettingActionClass::getOrientation),

	GetSettingsAction(SettingName_SwitchDefault, GetSettingActionClass::getSwitch),

	GetSettingsAction(SettingName_All, GetSettingActionClass::getAll),
};


void GetSettingActionClass::execute(const char * parameters)
{
	size_t size;
	const char * settingName = getSettingName(parameters, size);
	if (0 == size) { LogErrorAndReturn(PSTR("ERROR: SETTING NAME NOT SPECIFIED")); }

	for (unsigned int i = 0; i < sizeof(SettingsActions) / sizeof(GetSettingsAction); i++)
	{
		if (strncmp_P(settingName, SettingsActions[i].settingName, size) == 0u)
		{
			LoggerService.debug_P(PSTR("Sending %s"), settingName); //No null terminator. So it sends setting parameter if it exists.
			SettingsActions[i].getFunc(parameters + size);
			return;
		}

	}
	LogErrorAndReturn(PSTR("ERROR: INVALID SETTING NAME IN %s"), settingName);
}


void GetSettingActionClass::getCenter(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatPos, SettingName_Center, Settings.CenterPos_X, Settings.CenterPos_Y);
}

void GetSettingActionClass::getMaxSpeed(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatDouble1, SettingName_MaxSpeed, Settings.Max_Speed);
}

void GetSettingActionClass::getOrientation(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_Orientation, Settings.Orientation);
}

void GetSettingActionClass::getSwitch(const char * parameters)
{
	HardwareSwitch hardwareSwitch;
	if (!getHardwareSwitch(&parameters, hardwareSwitch)) { LogErrorAndReturn(PSTR("ERROR: INVALID SWITCH VALUE")); }

	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[hardwareSwitch],
		Settings.DefaultSwitchStates[hardwareSwitch] ? OnString : OffString);
}

void GetSettingActionClass::getAll(const char * parameters)
{
	getCenter(NULL);
	getMaxSpeed(NULL);
	getOrientation(NULL);

	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch1],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch1] ? OnString : OffString);
	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch2],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch2] ? OnString : OffString);
	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch3],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch3] ? OnString : OffString);
	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch4],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch4] ? OnString : OffString);
}

GetSettingActionClass GetSettingAction;

