#include "stdafx.h"
#include "Options.h"
#include "CmdArgs.h"
#include "Error.h"
#include "Streams.h"
#include "test/Test.h"
#include "Processor.h"
#if defined( _DEBUG ) && defined( WIN32 )
#include <windows.h>
#endif



using namespace sqtpp;
using namespace std;

/**
** @brief sqtpp main.
** @return success code:
** - 2 fatal error occured.
** - 1 a recoverable error was encountered.
** - 0 Success
*/
int wmain(int argc, const wchar_t* const argv[])
{
#	ifdef _DEBUG
#	ifdef _WIN32
	WinDebugOutBuffer logBuffer;
	wclog.rdbuf( &logBuffer );
#	endif
	sqtpp::test::Test::run();
#	endif

	Options  options;
	Processor cpp( options );

	try {
		CmdArgs  cmdargs( argc, argv );
		cmdargs.parse( options );

		const StringArray& files = cmdargs.getFiles();

		if ( files.size() == 0 ) {
			throw error::D8003();
		}
		StringArray::const_iterator it = files.begin();
		while ( it != files.end() ) {
			const wstring& fileName = *it;
			cpp.processFile( fileName );
			++it;
		}
	} catch ( const error::Error& error ) {
		wcerr << error;
	} catch ( const std::exception& ex ) {
		wcerr << ex.what();
	}

#	ifdef _DEBUG
#	ifdef WIN32
	if ( ::IsDebuggerPresent() ) {
		wchar_t c;
		wcin >> c;
	}
#	else
	wchar_t c;
	wcin >> c;
#	endif
#	endif

	if ( cpp.getMaxMessageSeverity() >= error::Error::SEV_FATAL ) {
		return 2;
	}

	if ( cpp.getMaxMessageSeverity() >= error::Error::SEV_ERROR ) {
		return 1;
	}

	return 0;
}
