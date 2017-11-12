// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif

#include <MsTimer2.h>

#include "TimerService.h"
#include "LoggerService.h"

#define TimeToTrigger(reg) (reg.durrationMs > reg.start) ? 

bool TimerServiceClass::addTimer(TimerCallback callback, unsigned long milliseconds)
{
	unsigned long now = millis();

	MsTimer2::stop();

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
	MsTimer2::stop();

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

	MsTimer2::set(_nextTimer->triggerTime - now, TimerServiceClass::timer_interrupt);
	MsTimer2::start();

}

void TimerServiceClass::timer_interrupt()
{
	MsTimer2::stop();
	TimerService.trigger_timer(millis());
}

TimerServiceClass TimerService;

