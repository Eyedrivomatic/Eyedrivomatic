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
	bool addTimer(TimerCallback callback, unsigned long milliseconds);
	void removeTimer(TimerCallback);

protected:
	typedef struct TimerRegistration
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

