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


#pragma once

#include "IDynavoxHost.h"

DEFAULT_NAMESPACE{

	public ref class DynavoxHostFactory
	{
	public:
		static property bool IsAvailable { bool get(); }
		static property String ^DeviceUrl { String ^ get(); }
		static IDynavoxHost^ CreateHost();
	};

}END_DEFAULT_NAMESPACE
