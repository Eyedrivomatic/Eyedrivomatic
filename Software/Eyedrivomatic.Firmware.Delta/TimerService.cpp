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

#include <IntervalTimer.h>

#include "TimerService.h"
#include "LoggerService.h"

#define TimeToTrigger(reg) (reg.durrationMs > reg.start) ? 
IntervalTimer Timer;

void TimerServiceClass::addTimer(TimerCallback callback, unsigned long milliseconds)
{
	unsigned long now = millis();

	Timer.end();

	for (int i = 0; i< TIMER_REGISTRATION_QUEUE_SIZE; i++)
	{ 
		TimerRegistration &reg = _registeredTimers[i];
		if (!reg.active)
		{
			reg.callback = callback;
			reg.triggerTime = now + milliseconds; //expected overflow.
			reg.active = true;
			break;
		}
	}
	set_next_timer(now);
}

void TimerServiceClass::removeTimer(TimerCallback callback)
{
	unsigned long now = millis();
	Timer.end();

	if (_nextTimer != NULL && _nextTimer->callback == callback) _nextTimer = NULL;

	for (int i = 0; i< TIMER_REGISTRATION_QUEUE_SIZE; i++)
	{
		TimerRegistration &reg = _registeredTimers[i];
		if (reg.active && reg.callback == callback)
		{
			reg.active = false;
			break;
		}
	}
	set_next_timer(now);
}

void TimerServiceClass::trigger_timer(unsigned long now)
{
	_nextTimer->active = false;
	_nextTimer->callback();
	_nextTimer = NULL;
	set_next_timer(now);
}

void TimerServiceClass::set_next_timer(unsigned long now)
{
	for (int i = 0; i< TIMER_REGISTRATION_QUEUE_SIZE; i++)
	{
		TimerRegistration &reg = _registeredTimers[i];
		if (reg.active && (_nextTimer == NULL || (_nextTimer->triggerTime - now) > (reg.triggerTime - now)))
		{
			_nextTimer = &reg;
		}
	}

	if (_nextTimer == NULL) return;

	Timer.begin(TimerServiceClass::timer_interrupt, (_nextTimer->triggerTime - now) * 1000);
}

void TimerServiceClass::timer_interrupt()
{
	Timer.end();
	TimerService.trigger_timer(millis());
}

TimerServiceClass TimerService;
