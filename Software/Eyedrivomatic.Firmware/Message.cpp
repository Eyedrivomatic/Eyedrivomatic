// 
// 
// 
#include "Message.h"
#include "LoggerService.h"
#include <avr/wdt.h>

#define LF  0X0A
#define ACK 0x06 
#define NAK 0x15
#define CHECKSEP '#'


const char OK[]  PROGMEM = "OK";
const char ERR[] PROGMEM = "ERR";

const uint8_t SendRetries = 3;
const unsigned long ResponseTimeout = 1000;

bool MessageClass::readNext()
{
	// Messages are sent as ascii strings in the following format.
	// "Command[ P ][:X]LF"
	// Command = a command; 
	// [ P ] = command parameters. The format is specific to the command but may not include line-feeds.
	// [:X] = a checksum in hex - xOr the bytes of the message up to and including the space between the last parameter and the checksum.
	// [LF] = the ASCII line-feed character (0x0A) 

	_lasterror = NULL;

	int nextChar = Serial.peek();
	if (-1 == nextChar) return false; //Nothing available;

	switch (nextChar)
	{
	case LF:
		Serial.read();
		break;
	default:
		return readMessageIntoBuffer();
	}

	return false;
}

void MessageClass::logMessage()
{
	if (NULL != _lasterror)
	{
		if (_messageSize > 0) LoggerService.error_P(PSTR("Invalid message received '%s'"), _readBuffer);
		LoggerService.error_P(_lasterror);
	}
	else LoggerService.debug_P(PSTR("Received message [%s]"), _readBuffer);
}

bool MessageClass::send(const char * data)
{
	for (int i = 0; i < SendRetries; i++)
	{
		if (sendInternal(data)) return true;
	}
	return false;
}

size_t MessageClass::getBuffer(const char ** pBuffer) const
{
	*pBuffer = _readBuffer;
	return _messageSize;
}

bool MessageClass::readMessageIntoBuffer()
{
	_messageSize = Serial.readBytesUntil(LF, _readBuffer, READ_BUFFER_SIZE-1); //does not include the LF but will return when the buffer is empty. Leave space to add a null char.
	_readBuffer[_messageSize] = '\0'; //Place a null terminator at the end of the message.
	if (_messageSize == 0) return false;

	if (Serial.peek() == LF) Serial.read();
	return true;
}

uint8_t MessageClass::calculateChecksum(const char * data, size_t size)
{
	char checkChar = 0;
	for (size_t i = 0; i < size; i++)
	{
		checkChar ^= static_cast<uint8_t>(data[i]);
	}
	return checkChar;
}

bool MessageClass::validateChecksum()
{
	if (!useChecksum) return true;
	if (_messageSize < 4) return false;

	if (_readBuffer[_messageSize - 3] != CHECKSEP) { _lasterror = PSTR("Missing checksum seperator."); return false; }

	_readBuffer[_messageSize - 3] = '\0'; //makes processing the message much easier.

	uint8_t expectedChecksum = calculateChecksum(_readBuffer, _messageSize - 3); //Don't include the CHECKSEP character.
	unsigned long receivedChecksum = strtoul(_readBuffer + _messageSize - 2, NULL, 16);

	if (receivedChecksum != static_cast<unsigned long>(expectedChecksum))
	{
		_lasterror = PSTR("Checksum failed.");
		return false;
	}

	return true;
}

bool MessageClass::sendInternal(const char * data)
{
	size_t written = 0;
	size_t size = strlen(data);
	int sends = 0;

	while (written < size)
	{
		written += Serial.write(data + written, size - written);
		sends++;
	}

	if (useChecksum)
	{
		char checksum[4];
		snprintf_P(checksum, sizeof(checksum), PSTR("%1%02X"), CHECKSEP, calculateChecksum(data, size));
		written += Serial.write(checksum, 3);
	}
	
	written += Serial.write(LF);

	LoggerService.debug_P(PSTR("Sent %d bytes in %d sends."), written, sends);
	if (written == size + (useChecksum ? 4 : 1)) 
	{
		LoggerService.error_P(PSTR("Failed to write message. expecting %d characters written."), size + (useChecksum ? 4 : 1));
		return false;
	}
	
	return true;
}

void MessageClass::sendAck()
{
	if (useAckResponses && Serial.write(ACK) != sizeof(char))
	{
		LoggerService.warn_P(PSTR("Failed to send ACK response."));
	}
}

void MessageClass::sendNak()
{
	if (useAckResponses && Serial.write(NAK) != sizeof(char))
	{
		LoggerService.warn_P(PSTR("Failed to send NAK response."));
	}
}

MessageClass Message;

