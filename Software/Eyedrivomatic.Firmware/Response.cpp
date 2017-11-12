// 
// 
// 

#if defined(ARDUINO) && ARDUINO >= 100
#include "arduino.h"
#else
#include "WProgram.h"
#endif


#include "Response.h"
#include "Message.h"


void ResponseClass::SendResponse(const char *message)
{
	Message.send(message);
}

void ResponseClass::SendResponse_f(const char *message, ...)
{
	va_list vl;
	va_start(vl, message);

	SendResponse_v(message, vl);
}

void ResponseClass::SendResponse_v(const char *message, va_list vl)
{
	char buffer[WRITE_BUFFER_SIZE];
	vsnprintf(buffer, sizeof(buffer), message, vl);
	SendResponse(buffer);
}

void ResponseClass::SendResponse_P(const char *message, ...)
{
	va_list vl;
	va_start(vl, message);

	char buffer[WRITE_BUFFER_SIZE];
	vsnprintf_P(buffer, sizeof(buffer), message, vl);
	SendResponse(buffer);
}

void ResponseClass::QueueResponse_v(const char* message, va_list vl)
{
	vsnprintf(_queuedResponse[_queuePtrEnd].buffer, WRITE_BUFFER_SIZE, message, vl);
	_queuePtrEnd = (_queuePtrEnd + 1) % RESPONSE_QUEUE_SIZE;
	if (_queuePtrEnd == _queuePtrStart) _queuePtrStart = (_queuePtrStart + 1) % RESPONSE_QUEUE_SIZE;
}

void ResponseClass::QueueResponse_P(const char* message, ...)
{
	va_list vl;
	va_start(vl, message);

	vsnprintf_P(_queuedResponse[_queuePtrEnd].buffer, WRITE_BUFFER_SIZE, message, vl);
	_queuePtrEnd = (_queuePtrEnd + 1) % RESPONSE_QUEUE_SIZE;
	if (_queuePtrEnd == _queuePtrStart) _queuePtrStart = (_queuePtrStart + 1) % RESPONSE_QUEUE_SIZE;
}

void ResponseClass::QueueResponse(const char* message, ...)
{
	va_list vl;
    va_start(vl, message);
	QueueResponse_v(message, vl);
}

void ResponseClass::SendQueuedResponses()
{
	while (_queuePtrStart != _queuePtrEnd)
	{
		SendResponse(_queuedResponse[_queuePtrStart].buffer);
		_queuePtrStart = (_queuePtrStart + 1) % RESPONSE_QUEUE_SIZE;
	}
}

ResponseClass Response;

