// ConfigureChecksumAction.h

#ifndef _CONFIGURECHECKSUMACTION_h
#define _CONFIGURECHECKSUMACTION_h

#include "Action.h"

class ConfigureChecksumActionClass : public ActionClass
{
 public:
	 virtual bool expectChecksum() override { return false; }
	 virtual bool shouldRespond() override { return false; }

	 virtual void execute(const char * parameters) override;
};

extern ConfigureChecksumActionClass ConfigureChecksumAction;

#endif

