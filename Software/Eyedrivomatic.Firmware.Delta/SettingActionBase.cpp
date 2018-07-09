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

#include "SettingActionBase.h"

const char * SettingActionBaseClass::getSettingName(const char * parameters, size_t & size)
{
	size = 0;
	//The first parameter is the setting name.
	if (NULL == parameters) return NULL;

	const char *nameStart = parameters;
	while (*nameStart == ' ') nameStart++;

	const char* nameEnd = strchr(nameStart, ' ');
	if (NULL == nameEnd) nameEnd = strchr(nameStart, ':');
	if (NULL == nameEnd) nameEnd = parameters + strlen(parameters);

	if (nameEnd == nameStart) return NULL;

	size = nameEnd - nameStart;
	return nameStart;
}

bool SettingActionBaseClass::getHardwareSwitch(const char ** parameters, HardwareSwitch & hardwareSwitch)
{
	int len = (NULL == parameters) ? 0 : strlen(*parameters);
	if (len == 0) return false;

	const char * start = *parameters;
	const char * end = (*parameters) + len;
		
	while (start != end && *start == ' ') start++; //Find the start of the switch number.
	if (start == end) return false;

	char * numEnd;
	long switchNum = strtol(start, &numEnd, 10)-1;
	if (start == numEnd) return false;
	
	if (switchNum < HardwareSwitch::Switch1 || switchNum > HardwareSwitch::Switch3) return false;

	hardwareSwitch = static_cast<HardwareSwitch>(switchNum);
	*parameters = numEnd;
	return true;
}



