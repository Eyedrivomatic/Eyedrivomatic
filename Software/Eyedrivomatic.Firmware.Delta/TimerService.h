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


// TimerService.h

#ifndef _TIMERSERVICE_h
#define _TIMERSERVICE_h

#define TIMER_REGISTRATION_QUEUE_SIZE 4

typedef void (*TimerCallback)();

class TimerServiceClass
{
protected:
	static void timer_interrupt();
	void trigger_timer(unsigned long now);
	void set_next_timer(unsigned long now);

 public:
	void addTimer(TimerCallback callback, unsigned long milliseconds);
	void removeTimer(TimerCallback);

protected:
	struct TimerRegistration
	{
		bool active = false;
		TimerCallback callback = NULL;
		unsigned long triggerTime = 0;
	};

	TimerRegistration _registeredTimers[TIMER_REGISTRATION_QUEUE_SIZE];
	volatile TimerRegistration *_nextTimer;
};

extern TimerServiceClass TimerService;

#endif

