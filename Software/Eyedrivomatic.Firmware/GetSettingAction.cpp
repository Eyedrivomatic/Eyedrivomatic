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

const char SettingsResponseFormatString[] PROGMEM = "SETTING: %S %S";
const char SettingsResponseFormatInt[] PROGMEM = "SETTING: %S %d";

#define LogErrorAndReturn(msg, ...) Response.SendResponse_P(msg, ##__VA_ARGS__); return;

typedef struct GetSettingsAction
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
	GetSettingsAction(SettingName_MinPosX, GetSettingActionClass::getXMin),
	GetSettingsAction(SettingName_CenterPosX, GetSettingActionClass::getXCenter),
	GetSettingsAction(SettingName_MaxPosX, GetSettingActionClass::getXMax),
	GetSettingsAction(SettingName_InvertX, GetSettingActionClass::getXInvert),

	GetSettingsAction(SettingName_MinPosY, GetSettingActionClass::getYMin),
	GetSettingsAction(SettingName_CenterPosY, GetSettingActionClass::getYCenter),
	GetSettingsAction(SettingName_MaxPosY, GetSettingActionClass::getYMax),
	GetSettingsAction(SettingName_InvertY, GetSettingActionClass::getYInvert),

	GetSettingsAction(SettingName_SwitchDefault, GetSettingActionClass::getSwitch),

	GetSettingsAction(SettingName_All, GetSettingActionClass::getAll),
};


void GetSettingActionClass::execute(const char * parameters)
{
	size_t size;
	const char * settingName = getSettingName(parameters, size);
	if (0 == size) { LogErrorAndReturn(PSTR("ERROR: SETTING NAME NOT SPECIFIED")); }

	for (int i = 0; i < sizeof(SettingsActions) / sizeof(GetSettingsAction); i++)
	{
		if (strncmp_P(settingName, SettingsActions[i].settingName, size) == 0)
		{
			LoggerService.debug_P(PSTR("Sending %s"), settingName); //No null terminator. So it sends setting parameter if it exists.
			SettingsActions[i].getFunc(parameters + size);
			return;
		}

	}
	LogErrorAndReturn(PSTR("ERROR: INVALID SETTING NAME IN %s"), settingName);
}


void GetSettingActionClass::getXMin(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_MinPosX, Settings.MinPos_X);
}

void GetSettingActionClass::getXCenter(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_CenterPosX, Settings.CenterPos_X);
}

void GetSettingActionClass::getXMax(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_MaxPosX, Settings.MaxPos_X);
}

void GetSettingActionClass::getXInvert(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatString, SettingName_InvertX, Settings.Invert_X ? OnString : OffString);
}

void GetSettingActionClass::getYMin(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_MinPosY, Settings.MinPos_Y);
}

void GetSettingActionClass::getYCenter(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_CenterPosY, Settings.CenterPos_Y);
}

void GetSettingActionClass::getYMax(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatInt, SettingName_MaxPosY, Settings.MaxPos_Y);
}

void GetSettingActionClass::getYInvert(const char * parameters)
{
	Response.SendResponse_P(SettingsResponseFormatString, SettingName_InvertY, Settings.Invert_Y ? OnString : OffString);
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
	getXMin(NULL);
	getXCenter(NULL);
	getXMax(NULL);
	getXInvert(NULL);
	getYMin(NULL);
	getYCenter(NULL);
	getYMax(NULL);
	getYInvert(NULL);

	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch1],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch1] ? OnString : OffString);
	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch2],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch2] ? OnString : OffString);
	Response.SendResponse_P(SettingsResponseFormatString, HardwareSwitchNames[HardwareSwitch::Switch3],
		Settings.DefaultSwitchStates[HardwareSwitch::Switch3] ? OnString : OffString);
}

GetSettingActionClass GetSettingAction;

