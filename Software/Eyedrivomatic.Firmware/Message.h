// Message.h

#ifndef _MESSAGE_h
#define _MESSAGE_h

#if defined(ARDUINO) && ARDUINO >= 100
	#include "arduino.h"
#else
	#include "WProgram.h"
#endif

#define READ_BUFFER_SIZE 32
#define WRITE_BUFFER_SIZE 128

#define CHECKSEP '#'


class MessageClass
{
 protected:

public:
	bool readNext();
	bool validateChecksum();
	void logMessage();

	bool send(const char * data);
	void sendAck();
	void sendNak();

	size_t getBuffer(const char ** pBuffer) const;

#ifdef _DEBUG
	bool useChecksum = false;
	bool useAckResponses = false;
#else
	bool useChecksum = true;
	bool useAckResponses = true;
#endif

private:
	bool readMessageIntoBuffer();
	uint8_t calculateChecksum(const char* data, size_t size);
	bool sendInternal(const char * data);
	char timedPeek(unsigned long timeout);
	
	char _readBuffer[READ_BUFFER_SIZE];
	size_t _messageSize;
	const char * _lasterror;
};

extern MessageClass Message;

#endif

