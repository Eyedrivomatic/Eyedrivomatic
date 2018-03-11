wxWidgets Calibration Sample
============================
This sample application demonstrates how to calibrate the eye tracker. It is
built using the wxWidgets GUI library, which is available on most popular
platforms.

The sample is built using the model-view-view model pattern, a variation on the
model-view-controller pattern. The idea is to separate the human-facing parts
of the application from the model-facing parts. (Interestingly enough it is the
eye tracker that takes the part of the "model" in this application.)

The classes that make up the application are:

- CalibrationSampleApp: the application entry point.
- CalibrationWindow: the frame/panel combo that does the drawing.
- CalibrationViewModel: the view model for the calibration window. Communicates
  with the eye tracker and instructs the calibration window what it should be
  displaying at any time.
- TestingViewModel: a dummy implementation of the calibration view model, which
  can be used for testing the calibration window. The choice of which view model
  to use is done by the CalibrationSampleApp.

Building on Windows
-------------------
The sample code includes a project file for Visual Studio 2012, which works with
VS 2010 as well.

1. Download and install wxWidgets version 3.0 or later from
   https://www.wxwidgets.org/downloads/. We recommend using the source code
   package, since it is easier to get started with than the binaries packages.
2. Set the WXROOT environment variable to the path where you installed/unpacked
   wxWidgets. For example, "C:\wxWidgets-3.0.0".
3. Build the wxWidgets library according to the instructions in
   %WXROOT%\docs\msw\install.txt.
4. Start Visual Studio and open the project file included with the sample. Build
   and run the application. When building for the x64 platform, make sure to 
   select x64 as the solution platform (Build/Configuration manager).

Note that Visual Studio has to be (re)started after the environment variable is
set. Otherwise the setting won't take effect.

Building on Linux (Ubuntu 12.0.4 LTS)
-------------------------------------
1. Install the wxWidgets development package like so:
   (sudo) apt-get install libwxgtk2.8-dev
2. Build the sample using the included makefile.
