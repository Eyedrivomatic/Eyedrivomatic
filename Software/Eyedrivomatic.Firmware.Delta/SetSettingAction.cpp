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

#include "SetSettingAction.h"
#include "GetSettingAction.h"
#include "LoggerService.h"
#include "Settings.h"
#include "Response.h"

#define LogError(msg, ...) Response.SendResponse_P(msg, ##__VA_ARGS__);

#define LogErrorAndReturn(msg, ...) LogError(msg, ##__VA_ARGS__) return;

#define ReadAndValidate_ul(parameters, minValue, maxValue, settingName, setting, nextParam) \
{\
	unsigned long value = strtoul(parameters, &nextParam, 10); \
	if (parameters == nextParam) { LogErrorAndReturn(PSTR("ERROR: MISSING {%s} VALUE"), settingName); }\
	if (value >= minValue && value <= maxValue) setting = value; \
	else LogError(PSTR("ERROR: '%s' is out of range (%u to %u) for %s"), parameters, minValue, maxValue, settingName)\
}\

#define ReadAndValidate_lst(parameters, allowed, settingName, setting, nextParam) \
{\
	unsigned long value = strtoul(parameters, &nextParam, 10); \
	if (parameters == nextParam) { LogErrorAndReturn(PSTR("ERROR: MISSING {%s} VALUE"), settingName); }\
	for (auto item : allowed)\
	{\
		if (value == item)\
		{\
			setting = value;\
			break;\
		}\
	}\
	if (setting != value) LogError(PSTR("ERROR: '%s' is not a valid value for %s"), parameters, settingName);\
}\

#define ReadAndValidate_l(parameters, minValue, maxValue, settingName, setting, nextParam) \
{\
	long value = strtol(parameters, &nextParam, 10); \
	if (parameters == nextParam) { LogErrorAndReturn(PSTR("ERROR: MISSING {%s} VALUE"), settingName); }\
	if (value >= minValue && value <= maxValue) setting = value; \
	else LogError(PSTR("ERROR: '%li' is out of range (%i to %i) for %s"), value, minValue, maxValue, settingName)\
}\

#define ReadAndValidate_d(parameters, minValue, maxValue, settingName, setting, nextParam) {\
	double value = strtod(parameters, &nextParam); \
	if (parameters == nextParam) { LogErrorAndReturn(PSTR("ERROR: MISSING {%s} VALUE"), settingName); }\
	if (value >= minValue && value <= maxValue) setting = value; \
	else LogError(PSTR("ERROR: '%3.1f' is out of range (%3.1f to %3.1f) for %s"), value, minValue, maxValue, settingName)\
}\

#define ReadAndValidate_b(parameters, settingName, setting) \
while (*parameters == ' ') parameters++; \
if (strncmp_P(parameters, OnString, strlen_P(OffString)) == 0) setting = true; \
else if (strncmp_P(parameters, OffString, strlen_P(OffString)) == 0) setting = false; \
else LogError(PSTR("ERROR: '%s' is not a valid value for %s"), parameters, settingName);

struct SetSettingsAction
{
public:
	SetSettingsAction(const char * name, void(*f)(const char*))
	{
		settingName = name;
		setFunc = f;
	}
	const char * settingName;
	void(*setFunc)(const char*);
};

const char SettingName_ResetDefaults[] PROGMEM = "DEFAULTS";


const SetSettingsAction SettingsActions[] PROGMEM =
{
	SetSettingsAction(SettingName_Center, SetSettingActionClass::setCenter),
	SetSettingsAction(SettingName_MaxSpeed, SetSettingActionClass::setMaxSpeed),
	SetSettingsAction(SettingName_Orientation, SetSettingActionClass::setOrientation),

	SetSettingsAction(SettingName_SwitchDefault, SetSettingActionClass::setSwitch),

	SetSettingsAction(SettingName_ResetDefaults, SetSettingActionClass::setDefaults),
};


void SetSettingActionClass::execute(const char * parameters)
{
	size_t size;
	const char * settingName = getSettingName(parameters, size);
	if (0 == size) { LogErrorAndReturn(PSTR("ERROR: SETTING NAME NOT SPECIFIED")); }

	for (unsigned int i = 0; i < sizeof(SettingsActions) / sizeof(SetSettingsAction); i++)
	{
		if (strncmp_P(settingName, SettingsActions[i].settingName, size) == 0)
		{
			LoggerService.debug_P(PSTR("Sending '%s'"), settingName); //No null terminator. So it sends setting parameter if it exists.
			SettingsActions[i].setFunc(parameters + size + 1);
			return;
		}

	}
	LogErrorAndReturn(PSTR("ERROR: INVALID SETTING NAME IN '%s'"), settingName);
}


void SetSettingActionClass::setCenter(const char * parameters)
{
	double min_x, max_x, min_y, max_y;
	State.getCenterLimit(min_x, max_x, min_y, max_y);
	char * nextParam;

	ReadAndValidate_d(parameters, min_x, max_x, SettingName_CenterX, Settings.CenterPos_X, nextParam);
	if (*nextParam == ',') nextParam++;
	ReadAndValidate_d(nextParam, min_y, max_y, SettingName_CenterY, Settings.CenterPos_Y, nextParam);
	State.resetServoPositions();
	GetSettingAction.getCenter(NULL);
}

void SetSettingActionClass::setOrientation(const char * parameters)
{
	char * nextParam;
	unsigned long allowed[] = { 0, 90, 180, 270 };

	ReadAndValidate_lst(parameters, allowed, SettingName_Orientation, Settings.Orientation, nextParam);
	State.resetServoPositions();
	GetSettingAction.getOrientation(NULL);
}

void SetSettingActionClass::setMaxSpeed(const char * parameters)
{
	double max_speed;
	State.getSpeedLimit(max_speed);
	char * nextParam;
	ReadAndValidate_d(parameters, 0L, max_speed, SettingName_MaxSpeed, Settings.Max_Speed, nextParam);
	GetSettingAction.getMaxSpeed(NULL);
}

void SetSettingActionClass::setSwitch(const char * parameters)
{
	HardwareSwitch hardwareSwitch;
	const char * cache = parameters;
	if (!getHardwareSwitch(&parameters, hardwareSwitch)) { LogErrorAndReturn(PSTR("ERROR: INVALID SWITCH VALUE")); }

	while (*parameters == ' ') parameters++;
	if (strncmp_P(parameters, OnString, strlen_P(OnString)) == 0) Settings.DefaultSwitchStates[hardwareSwitch] = true;
	else if (strncmp_P(parameters, OffString, strlen_P(OffString)) == 0) Settings.DefaultSwitchStates[hardwareSwitch] = false;
	else
	{
		LogErrorAndReturn(PSTR("ERROR: INVALID SWITCH STATE '%s'"), parameters);
	}
	GetSettingAction.getSwitch(cache);
}

void SetSettingActionClass::setDefaults(const char * parameters)
{
	LoggerService.debug_P(PSTR("Loading Factory Default Settings"));
	Settings.reset();

	GetSettingAction.getAll(parameters);
	State.reset();
}

SetSettingActionClass SetSettingAction;

