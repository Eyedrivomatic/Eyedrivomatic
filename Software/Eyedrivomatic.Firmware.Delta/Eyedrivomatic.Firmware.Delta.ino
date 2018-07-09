/*
 Name:		Firmware.ino
 Created:	8/25/2016 8:32:44 PM
 Author:	Cody Barnes
*/
#define _DEBUG

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <MsTimer2.h>
#include <avr/wdt.h>

#include "Message.h"
#include "MessageProcessor.h"
#include "LoggerService.h"
#include "Response.h"
#include "Settings.h"
#include "State.h"
#include "ActionService.h"
#include "HostConnectionService.h"

void sendStartupInfo();

// the setup function runs once when you press reset or power the board
void setup() 
{
	State.init();
	Settings.init();
	State.reset();

	wdt_enable(WDTO_1S);

	LoggerService.shouldQueueLogs(false);
	LoggerService.sendQueuedLogs();
}

// the loop function runs over and over again until power down or reset
// nothing done here should be time critical.
void loop() 
{
	wdt_reset();

	HostConnectionService.MonitorConnection();
	Response.SendQueuedResponses();

	if (Message.readNext())
	{
		MessageProcessor.processMessage(Message);
	}

	LoggerService.sendQueuedLogs();
}