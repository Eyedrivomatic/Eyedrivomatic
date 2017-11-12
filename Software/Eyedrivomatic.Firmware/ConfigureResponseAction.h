// ConfigureChecksumAction.h

#ifndef _CONFIGURERESPONSEACTION_h
#define _CONFIGURERESPONSEACTION_h

#include "Action.h"

class ConfigureResponseActionClass : public ActionClass
{
 public:
	 virtual bool expectChecksum() override { return false; }
	 virtual bool shouldRespond() override { return false; }

	 virtual void execute(const char * parameters) override;
};

extern ConfigureResponseActionClass ConfigureResponseAction;

#endif

