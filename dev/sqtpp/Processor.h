/**
** @file
** @author Ralf Seidel
** @brief Declaration of the #sqtpp::Processor.
**
** ? 2004-2006 by Heinrich und Seidel GbR Wuppertal.
*/
#ifndef SQTPP_PROCESSOR_H
#define SQTPP_PROCESSOR_H
#if _MSC_VER > 10
#pragma once
#endif
#include "Token.h"
#include "Context.h"
#include "Error.h"

namespace sqtpp {

class File;
class FileStack;
class Logger;
class Options;
class Output;
class LocationStack;
class Scanner;
class Macro;
class MacroSet;
class MacroArguments;
class MacroArgumentValues;
class TokenExpression;
class TokenExpressions;
class ITokenStream;
class TokenStreamStack;
}

namespace sqtpp {

/**
** @brief The token processor.
*/
class Processor
{
private:
	/// Set of file objects.
	typedef std::set<File>   FileSet;

	class MacroExpansion;

	/**
	** @brief Message logger.
	*/
	Logger&      m_logger;


	/**
	** @brief The pre processor options.
	*/
	Options&       m_options;

	/**
	** @brief The stream supplieing the processor with tokens.
	**
	** Intially this is the lexcial scanner.
	*/
	Scanner*      m_pScanner;

	ITokenStream* m_pTokenStream;

	/**
	** @brief Flags indication if the options have been applied.
	*/
	bool          m_bOptionsApplied;

	/**
	** @brief The stack of included files.
	*/ 
	FileStack&     m_fileStack;

	/**
	** @brief Files that should be included only once.
	*/ 
	StringSet&     m_includeOnceFiles;

	/**
	** @brief All currently defined macros.
	*/
	MacroSet&          m_macros;

	/**
	** Stack of conditional directives the scanner has found.
	*/
	LocationStack&     m_conditionalStack;

	/// The current token.
	TokenExpression&   m_tokenExpression;

	/**
	** Stack of token streams we are processing.
	*/
	TokenStreamStack&  m_tokenStreamStack;

	/// The previous line dumped out.
	std::wstring       m_prevLine;

	/// A buffer for the current line.
	std::wstringstream m_outputBuffer;

	/// The output stream.
	Output*            m_pOutput;

	/// Flag that is set if the output is defined externaly 
	/// i.e. not the default output managed by processor itself.
	bool               m_bExternalOutput;

	/// Number of lines read so far.
	size_t             m_nProcessedLines;

	/**
	** @brief The index of the token in the token input stream.
	** This variable is defined debugging to be able to set conditional breakpoints.
	*/
	size_t             m_nProcessedTokenId;

	/// Maximal severity of messesages emitted during processing.
	mutable error::Error::Severity m_eMaxMsgSeverity;

	/// Number off errors encountered.
	mutable int        m_nErrorCount;

	/// Number off warnings encountered.
	mutable int        m_nWarningCount;

	/// The ouput line number.
	size_t             m_nOutputLineNumber;

	/// The number of empty lines eliminated / skipped in the ouput.
	/// This variable is used to suppress the output of the #line 
	/// information if more than one empty line is skipped.
	size_t             m_nSkippedLineCount;

	// Timestamp for the tests that overrides the default to use
	// the current time.
	const tm*          m_pTestTimestamp;

private:
	// Copy c'tor (not implemented);
	Processor( const Processor& that );

	// Assignment operator (not implemented).
	const Processor& operator=( const Processor& that );

public:
	// Constructor.
	Processor( Options& options );
	// Destructor.
	~Processor();

	// Set the output.
	void setOutput( Output* pOutput );

	// Override the default ouput stream (wcout).
	void setOutStream( std::wostream& output );

	// Get the maximal severity of messages emitted during processing the input.
	error::Error::Severity getMaxMessageSeverity() const throw() { return m_eMaxMsgSeverity; }

	// Get the current timestamp 
	void getTimestamp( tm& timestamp ) const;

	// For testing only: override the default timestamp to make test deterministic.
	void setTimestamp( const tm* pTimestamp ) { this->m_pTestTimestamp = pTimestamp; }

	// Get the file which is currently processed.
	const File& getCurrentFile() const;

	// Get the pre processing options.
	const MacroSet& getMacros() const throw() { return m_macros; }

	// Get the pre processing options.
	const Options& getOptions() const throw() { return m_options; }

	// Get the pre processing options.
	void processStream( std::wistream& input );

	// Get the pre processing options.
	void processFile( const std::wstring& fileName );

	// Get the current scan/processing context.
	Context getContext() const throw();

	// Emit a message, warning or error.
	void emitMessage( error::Error& error ) const;

	// Process the input stream to collect the macro argumentsw
	bool collectMacroArgumentValues( const Macro& macro, ITokenStream& tokenStream, MacroArgumentValues& argumentValues ) const;

	// Close the out file if openend by this instance.
	void close();
private:
	// Get the root file of the preprocessor.
	const File& getRootFile() const;

	// Get the current file we are processing.
	File& getCurrentFile();

	// Check if the given identifier is the name of a predefined / buildin macro.
	bool isBuildinMacro( const wstring& identifier ) const;

	// Get the next token from the lexer.
	Token getNextToken();
	Token getNextToken( TokenExpression& tokenExpression );

	// Get the next token from the lexer.
	const wstring getNextIdentifier();

	// Emit the current line string to the ouput stream.
	void emitLineDirective( std::wostream& output );

	// Emit the new line chacter(s).
	void emitLineFeed( std::wostream& output, const wstring& sNewLine ) const;

	// Emit the current output buffer.
	size_t emitBuffer( std::wostream& ouput );

	// Check if the output range is restricted and if yes if the current tokens
	// are within the input range.
	bool isRootFilePositionWithinEmitRange();

	// Process the input stream to collect the macro argumentsw
	bool collectMacroArgumentValues( const Macro& macro, MacroArgumentValues& argumentValues, TokenExpressions& tokenExpressions );

	// Check if the macro tokens are valid.
	void validateMacroDefinition( const TokenExpressions& tokenExpressions, const MacroArguments& arguments );

	// Process the input stream to collect the macro argumentsw
	bool expandMacro( const Macro& macro, TokenExpressions& tokenExpressions );

	// Process the options defined at the command line (undef / define).
	void applyOptions();

	// Helper for \#if and \#endif.
	bool evaluateConditionalDirective();

	// Process the current input stream.
	void processInput();

	// Process the token.
	bool processToken( int scannerToken );

	// Scanner has found an identifier - expand macro if existent.
	void processIdentifier();

	// Process the new line token.
	void processNewLine();
	void processNewLine( const wstring* psNewLine );

	// Process a block comment.
	void processBlockComment();

	// Process a expression e.g. for the \#if or \#elif directives.
	void processExpression();

	// Finish processing a directive which is complete.
	void finishDirective( bool bEmit );

	// Process the #... directive.
	void processDirective();

	// Process the #define directive.
	void processDefineDirective();

	// Process the #undef directive.
	void processUndefDirective();

	// Process the #undefall directive.
	void processUndefallDirective();

	// Process the #include directive.
	void processIncludeDirective();

	// Process the #if directive.
	void processIfDirective();

	// Process the #ifdef directive.
	void processIfdefDirective();

	// Process the #ifndef directive.
	void processIfndefDirective();

	// Process the #else directive.
	void processElseDirective();

	// Process the #elif directive.
	void processElifDirective();

	// Process the #endif directive.
	void processEndifDirective();

	// Process the #message directive.
	void processMessageDirective();

	// Process the #error directive.
	void processErrorDirective();

	// Process the #line directive.
	void processLineDirective();

	// Process the #pragam directive.
	void processPragmaDirective();

	// Process the #pragam once directive.
	void processPragmaOnceDirective();

	// Process the #pragam message directive.
	void processPragmaMessageDirective();

	// Process the #pragam directive.
	void processImportDirective();

	// Process the #pragam directive.
	void processUsingDirective();

	// Process the #exec directive.
	void processExecDirective();

	// Process the S4M AdSalesNG directive: --[identifier] 
	void processAdSalesNGDirective();

	// Add the specified text to the output buffer.
	void appendToOutputLineBuffer( const wstring& output );
};

} // namespace sqtpp

#endif // SQTPP_PROCESSOR_H
