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

const char ValueOutOfRangeError_ul[] PROGMEM = "ERROR: %S is out of range (%u to %u)";

#define LogError(msg, ...) Response.SendResponse_P(msg, ##__VA_ARGS__);

#define LogErrorAndReturn(msg, ...) LogError(msg, ##__VA_ARGS__) return;

#define ReadAndValidate_ul(parameters, minValue, maxValue, settingName, setting) \
unsigned long value = strtoul(parameters, NULL, 10); \
if (value >= minValue && value <= maxValue) setting = value;\
else LogError(ValueOutOfRangeError_ul, settingName, minValue, maxValue)\

typedef struct SetSettingsAction
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
	SetSettingsAction(SettingName_MinPosX, SetSettingActionClass::setXMin),
	SetSettingsAction(SettingName_CenterPosX, SetSettingActionClass::setXCenter),
	SetSettingsAction(SettingName_MaxPosX, SetSettingActionClass::setXMax),

	SetSettingsAction(SettingName_MinPosY, SetSettingActionClass::setYMin),
	SetSettingsAction(SettingName_CenterPosY, SetSettingActionClass::setYCenter),
	SetSettingsAction(SettingName_MaxPosY, SetSettingActionClass::setYMax),

	SetSettingsAction(SettingName_SwitchDefault, SetSettingActionClass::setSwitch),

	SetSettingsAction(SettingName_ResetDefaults, SetSettingActionClass::setDefaults),
};


void SetSettingActionClass::execute(const char * parameters)
{
	size_t size;
	const char * settingName = getSettingName(parameters, size);
	if (0 == size) { LogErrorAndReturn(PSTR("ERROR: SETTING NAME NOT SPECIFIED")); }

	for (int i = 0; i < sizeof(SettingsActions) / sizeof(SetSettingsAction); i++)
	{
		if (strncmp_P(settingName, SettingsActions[i].settingName, size) == 0)
		{
			LoggerService.debug_P(PSTR("Sending %s"), settingName); //No null terminator. So it sends setting parameter if it exists.
			SettingsActions[i].setFunc(parameters + size);
			return;
		}

	}
	LogErrorAndReturn(PSTR("ERROR: INVALID SETTING NAME IN %s"), settingName);
}


void SetSettingActionClass::setXMin(const char * parameters)
{
	ReadAndValidate_ul(parameters, 0, Settings.CenterPos_X-1, SettingName_MinPosX, Settings.MinPos_X);
	GetSettingActionClass::getXMin(NULL);
}

void SetSettingActionClass::setXCenter(const char * parameters)
{
	ReadAndValidate_ul(parameters, Settings.MinPos_X+1, Settings.MaxPos_X-1, SettingName_CenterPosX, Settings.CenterPos_X);
	GetSettingActionClass::getXCenter(NULL);
}

void SetSettingActionClass::setXMax(const char * parameters)
{
	ReadAndValidate_ul(parameters, Settings.CenterPos_X + 1, 180, SettingName_MaxPosX, Settings.MaxPos_X);
	GetSettingActionClass::getXMax(NULL);
}

void SetSettingActionClass::setYMin(const char * parameters)
{
	ReadAndValidate_ul(parameters, 0, Settings.CenterPos_Y - 1, SettingName_MinPosY, Settings.MinPos_Y);
	GetSettingActionClass::getYMin(NULL);
}

void SetSettingActionClass::setYCenter(const char * parameters)
{
	ReadAndValidate_ul(parameters, Settings.MinPos_Y + 1, Settings.MaxPos_Y - 1, SettingName_CenterPosY, Settings.CenterPos_Y);
	GetSettingActionClass::getYCenter(NULL);
}

void SetSettingActionClass::setYMax(const char * parameters)
{
	ReadAndValidate_ul(parameters, Settings.CenterPos_Y + 1, 180, SettingName_MaxPosY, Settings.MaxPos_Y);
	GetSettingActionClass::getYMax(NULL);
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
		LogErrorAndReturn(PSTR("ERROR: INVALID SWITCH STATE %s"), parameters);
	}
	GetSettingActionClass::getSwitch(cache);
}

void SetSettingActionClass::setDefaults(const char * parameters)
{
	LoggerService.debug_P(PSTR("Loading Factory Default Settings"));
	Settings.reset();

	GetSettingActionClass::getAll(parameters);
}

SetSettingActionClass SetSettingAction;

