// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include "ActionService.h"
#include "ConfigureChecksumAction.h"
#include "ConfigureResponseAction.h"
#include "GetSettingAction.h"
#include "SetSettingAction.h"
#include "SendStatusAction.h"
#include "EnableLogAction.h"
#include "MoveServoAction.h"
#include "InvalidAction.h"
#include "StopAction.h"
#include "LoggerService.h"
#include "ToggleSwitchAction.h"

struct CommandMapEntry
{
	CommandMapEntry(const char * command, ActionClass & action)
		: command(command), action(action)
	{}

	const char * command;
	ActionClass & action;
};

const char ActionName_ConfigureChecksum[] PROGMEM = "#";
const char ActionName_ConfigureResponse[] PROGMEM = "!";
const char ActionName_SendStatus[] PROGMEM = "STATUS";
const char ActionName_EnableLog[] PROGMEM = "LOG";
const char ActionName_Stop[] PROGMEM = "STOP";
const char ActionName_Move[] PROGMEM = "MOVE";
const char ActionName_Switch[] PROGMEM = "SWITCH";
const char ActionName_Set[] PROGMEM = "SET";
const char ActionName_Get[] PROGMEM = "GET";

const CommandMapEntry CommandMap[] PROGMEM =
{
	CommandMapEntry(ActionName_ConfigureChecksum, ConfigureChecksumAction),
	CommandMapEntry(ActionName_ConfigureResponse, ConfigureResponseAction),
	CommandMapEntry(ActionName_SendStatus, SendStatusAction),
	CommandMapEntry(ActionName_EnableLog, EnableLogAction),
	CommandMapEntry(ActionName_Stop, StopAction),
	CommandMapEntry(ActionName_Move, MoveServoAction),
	CommandMapEntry(ActionName_Switch, ToggleSwitchAction),
	CommandMapEntry(ActionName_Set, SetSettingAction),
	CommandMapEntry(ActionName_Get, GetSettingAction),
};

const char * ActionService::GetCommand(const char * message, size_t & commandLength)
{
	if (NULL == message) return NULL;

	const char *commandStart = message;
	while (*commandStart == ' ') commandStart++;

	const char* commandEnd = strchr(message, ' ');
	if (NULL == commandEnd) commandEnd = strchr(message, CHECKSEP);
	if (NULL == commandEnd) commandEnd = message + strlen(message);

	commandLength = commandEnd - commandStart;
	if (commandLength == 0) return NULL;

	return commandStart;
}

const char * ActionService::GetParameters(const char * message)
{
	if (NULL == message) return NULL;

	size_t commandLength;
	const char * command = GetCommand(message, commandLength);
	if (NULL == command) return NULL;

	const char * params = command + commandLength;
	while (*params == ' ') params++;

	return params;
}

const char * ActionService::GetParameters(const MessageClass & message)
{
	const char *buffer;
	if (0 == message.getBuffer(&buffer))
	{
		return NULL;
	}
	return GetParameters(buffer);
}


ActionClass & ActionService::GetAction(const char * message)
{
	size_t actionCount = sizeof(CommandMap) / sizeof(CommandMapEntry);

	if (NULL == message) return InvalidAction;
	size_t commandLength;
	const char * command = GetCommand(message, commandLength);
	if (NULL == command) return InvalidAction;

	for (size_t i = 0; i < actionCount; i++)
	{
		if (strncmp_P(command, CommandMap[i].command, commandLength) == 0)
		{
			ActionClass & action = CommandMap[i].action;
			return action;
		}
	}

	return InvalidAction;
}

ActionClass & ActionService::GetAction(const MessageClass & message)
{
	const char * buffer;
	if (0 == message.getBuffer(&buffer)) return InvalidAction;
	return GetAction(buffer);
}

