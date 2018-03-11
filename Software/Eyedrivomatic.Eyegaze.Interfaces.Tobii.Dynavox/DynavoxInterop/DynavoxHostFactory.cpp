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


#include "stdafx.h"
#include "DynavoxHostFactory.h"
#include "DynavoxHost.h"

#include "tobiigaze_discovery.h"
#include <string.h>

DEFAULT_NAMESPACE{

	String ^ DynavoxHostFactory::DeviceUrl::get()
	{
		tobiigaze_error_code errorCode;
		const int urlSize = 256;
		char url[urlSize];
		memset(&url, 0, urlSize);

		tobiigaze_get_connected_eye_tracker(url, urlSize, &errorCode);
		return errorCode == TOBIIGAZE_ERROR_SUCCESS ? gcnew String(url): nullptr;
	}

	bool DynavoxHostFactory::IsAvailable::get()
	{
		return DeviceUrl != nullptr;
	}

	IDynavoxHost ^ DynavoxHostFactory::CreateHost()
	{
		if (!DynavoxHostFactory::IsAvailable) return nullptr;
		return gcnew DynavoxHost(DeviceUrl);
	}

}END_DEFAULT_NAMESPACE
