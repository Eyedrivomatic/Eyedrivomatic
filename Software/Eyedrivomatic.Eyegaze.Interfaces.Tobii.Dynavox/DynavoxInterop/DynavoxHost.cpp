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


// This is the main DLL file.

#include "stdafx.h"

#include "tobiigaze_discovery.h"
#include "tobiigaze.h"

#include "DynavoxHost.h"
#include "GazeDataSource.h"


DEFAULT_NAMESPACE{

	DynavoxHost::DynavoxHost(String ^url)
		: _url(url)
	{
	}

	DynavoxHost::~DynavoxHost()
	{
		this->!DynavoxHost();
	}

	DynavoxHost::!DynavoxHost()
	{
		if (IsConnected)
		{
			tobiigaze_disconnect(_eyeTracker);
		}

		if (_worker != NULL) delete _worker;

		if (_gazeDataSource != nullptr)
		{
			delete _gazeDataSource;
			_gazeDataSource = nullptr;
		}

		if (_eyeTracker != NULL)
		{
			tobiigaze_destroy(_eyeTracker);
			_eyeTracker = NULL;
		}
		
		if (_logger != NULL) delete _logger;
	}

	struct ConnectedCallbackData
	{
		gcroot<System::Threading::Tasks::TaskCompletionSource<bool>^> ConnectedTaskSource;
		gcroot<DynavoxHost^> Host;
		Logger *Logger;
	};

	void TOBIIGAZE_CALL ConnectedCallback(tobiigaze_error_code error_code, void *userData)
	{
		auto connectedData = static_cast<ConnectedCallbackData*>(userData);

		connectedData->Logger->WriteLog(LogLevel::Information, "Connected.\n");
		if (!connectedData->Host->LogDeviceInfo())
		{
			connectedData->ConnectedTaskSource->TrySetResult(false);
			return;
		}

		if (error_code)
		{
			connectedData->Logger->WriteLog(LogLevel::Error,
				String::Format("tobiigaze_connect_async: FAILED - [{0}]",
					gcnew String(tobiigaze_get_error_message(error_code))));

			connectedData->ConnectedTaskSource->TrySetException(gcnew ApplicationException(
				String::Format("Tobii Dynavox Eyegaze Operation [tobiigaze_connect_async] failed - [{1}]",
					gcnew String(tobiigaze_get_error_message(error_code)))));
		}
		else
		{
			connectedData->Logger->WriteLog(LogLevel::Diagnostic, "tobiigaze_connect_async: Completed Successfully");
			connectedData->ConnectedTaskSource->TrySetResult(true);
		}
		delete connectedData;
	}

	System::Threading::Tasks::Task<bool> ^DynavoxHost::InitializeAsync(LogWriter  ^loggingCallback)
	{
		if (_eyeTracker != nullptr) throw gcnew ApplicationException("DynavoxHost is already initialized.");
		
		if (_logger != NULL) delete _logger;
		_logger = new Logger(loggingCallback);

		_logger->WriteLog(LogLevel::Information, String::Format("Connecting to device at [{0}]", _url));

		tobiigaze_error_code errorCode;

		IntPtr p = Marshal::StringToHGlobalAnsi(_url);
		const char* url = static_cast<char*>(p.ToPointer());
		_eyeTracker = tobiigaze_create(url, &errorCode);
		Marshal::FreeHGlobal(p);

		if (!LogAndCheckError(errorCode, "tobiigaze_create")) return System::Threading::Tasks::Task::FromResult(false);
		_logger->Register(_eyeTracker);

		_logger->WriteLog(LogLevel::Diagnostic, "Starting tobii event loop thread.");
		if (_worker != NULL) delete _worker;

		_worker = new HostWorker(_logger);
		_worker->Start(_eyeTracker);

		// Connect to the tracker.
		auto callbackData = new ConnectedCallbackData;
		callbackData->Host = this;
		callbackData->ConnectedTaskSource = gcnew System::Threading::Tasks::TaskCompletionSource<bool>();
		callbackData->Logger = _logger;

		tobiigaze_connect_async(_eyeTracker, &ConnectedCallback, callbackData);

		return callbackData->ConnectedTaskSource->Task;
	}

	bool DynavoxHost::Initialize(LogWriter  ^loggingCallback)
	{
		if (_eyeTracker != nullptr) throw gcnew ApplicationException("DynavoxHost is already initialized.");

		if (_logger != NULL) delete _logger;
		_logger = new Logger(loggingCallback);

		_logger->WriteLog(LogLevel::Information, String::Format("Connecting to device at [{0}]", _url));

		tobiigaze_error_code errorCode;

		IntPtr p = Marshal::StringToHGlobalAnsi(_url);
		const char* url = static_cast<char*>(p.ToPointer());
		_eyeTracker = tobiigaze_create(url, &errorCode);
		Marshal::FreeHGlobal(p);

		if (!LogAndCheckError(errorCode, "tobiigaze_create")) return false;
		_logger->Register(_eyeTracker);

		_logger->WriteLog(LogLevel::Diagnostic, "Starting tobii event loop thread.");
		if (_worker != NULL) delete _worker;

		_worker = new HostWorker(_logger);
		_worker->Start(_eyeTracker);

		// Connect to the tracker.
		auto callbackData = new ConnectedCallbackData;
		callbackData->Host = this;
		callbackData->ConnectedTaskSource = gcnew System::Threading::Tasks::TaskCompletionSource<bool>();
		callbackData->Logger = _logger;

		tobiigaze_connect(_eyeTracker, &errorCode);
		if (!LogAndCheckError(errorCode, "tobiigaze_connect")) return false;

		return LogDeviceInfo();
	}

	bool DynavoxHost::IsConnected::get()
	{
		return IsInitialized && tobiigaze_is_connected(_eyeTracker);
	}

	IObservable<GazeData^> ^DynavoxHost::DataStream::get()
	{
		if (!IsConnected)
		{
			_logger->WriteLog(LogLevel::Error, "Gaze data stream is not available - the Tobii Dynavox eyetracker is not connected.");
			throw gcnew ApplicationException("The eyetracker is not connected.");
		}

		if (_gazeDataSource == nullptr)
		{
			_gazeDataSource = gcnew GazeDataSource(_eyeTracker, _logger);
		}

		return _gazeDataSource;
	}

	// Queries for and prints device information.
	bool DynavoxHost::LogDeviceInfo()
	{
		tobiigaze_error_code errorCode;
		struct tobiigaze_device_info info;

		tobiigaze_get_device_info(_eyeTracker, &info, &errorCode);
		if (!LogAndCheckError(errorCode, "tobiigaze_get_device_info")) return false;

		_logger->WriteLog(LogLevel::Diagnostic, String::Format("Serial number: {0}", gcnew String(info.serial_number)));
		_logger->WriteLog(LogLevel::Diagnostic, String::Format("Model: {0}, Generation {1}", gcnew String(info.model), gcnew String(info.generation)));
		_logger->WriteLog(LogLevel::Diagnostic, String::Format("Firmware Version: {0}", gcnew String(info.firmware_version)));
		return true;
	}

	bool DynavoxHost::LogAndCheckError(tobiigaze_error_code errorCode, const char  *operation)
	{
		if (errorCode)
		{
			_logger->WriteLog(LogLevel::Error,
				String::Format("{0}: FAILED - [{1}]", 
					gcnew String(operation), 
					gcnew String(tobiigaze_get_error_message(errorCode))));
			return false;
		}

		_logger->WriteLog(LogLevel::Diagnostic, String::Format("{0}: Completed Successfully", gcnew String(operation)));
		return true;
	}

	void DynavoxHost::LogAndFailIfError(tobiigaze_error_code errorCode, const char  *operation)
	{
		if (!LogAndCheckError(errorCode, operation))
		{
			throw gcnew ApplicationException(
				String::Format("Tobii Dynavox Eyegaze Operation [{0}] failed - [{1}]",
					gcnew String(operation),
					gcnew String(tobiigaze_get_error_message(errorCode))));
		}
	}
}END_DEFAULT_NAMESPACE

