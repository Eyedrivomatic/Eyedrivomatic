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


// 
// 
// 

#include "VersionInfo.h"
#include "BuildDefs.h"

#define xstr(a) str(a)
#define str(a) #a
#define VERSION xstr(VERSION_MAJOR) "." xstr(VERSION_MINOR) "." xstr(VERSION_BUILD)

const char VersionInfo::Application[] = "Eyedrivomatic";
const char VersionInfo::Version[] = VERSION;
