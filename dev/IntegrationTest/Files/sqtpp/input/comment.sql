-- Repro for issue 13732: A single quote within a comment should not yield to error C2001: String exceeds end of line.
-- A string within a comment should not be treated specially.
-- A single quoted 'string'
-- A double quoted "string"
-- A single single 'quote
-- A single double "quote
