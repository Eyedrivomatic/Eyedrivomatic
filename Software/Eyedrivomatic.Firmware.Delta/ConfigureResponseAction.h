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

