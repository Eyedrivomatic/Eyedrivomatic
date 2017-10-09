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
