// 	LumenWorks.Framework.IO.CSV.CsvReader
// 	Copyright (c) 2005 S�bastien Lorion
//
// 	MIT license (http://en.wikipedia.org/wiki/MIT_License)
//
// 	Permission is hereby granted, free of charge, to any person obtaining a copy
// 	of this software and associated documentation files (the "Software"), to deal
// 	in the Software without restriction, including without limitation the rights
// 	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// 	of the Software, and to permit persons to whom the Software is furnished to do so,
// 	subject to the following conditions:
//
// 	The above copyright notice and this permission notice shall be included in all
// 	copies or substantial portions of the Software.
//
// 	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// 	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// 	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// 	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// 	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using LumenWorks.Framework.IO.Csv.Resources;
using Debug = System.Diagnostics.Debug;

namespace LumenWorks.Framework.IO.Csv
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to CSV data.  
    /// </summary>
    internal partial class CsvDataReader
        : IDataReader, IEnumerable<string[]>, IDisposable
    {
        #region Constants

        /// <summary>
        /// Defines the default buffer size.
        /// </summary>
        public const int DefaultBufferSize = 0x1000;

        /// <summary>
        /// Defines the default delimiter character separating each field.
        /// </summary>
        public const char DefaultDelimiter = ',';

        /// <summary>
        /// Defines the default quote character wrapping every field.
        /// </summary>
        public const char DefaultQuote = '"';

        /// <summary>
        /// Defines the default escape character letting insert quotation characters inside a quoted field.
        /// </summary>
        public const char DefaultEscape = '"';

        /// <summary>
        /// Defines the default comment character indicating that a line is commented out.
        /// </summary>
        public const char DefaultComment = '#';

        #endregion

        #region Fields

        /// <summary>
        /// Contains the field header comparer.
        /// </summary>
        static readonly StringComparer fieldHeaderComparer = StringComparer.CurrentCultureIgnoreCase;

        #region Settings

        /// <summary>
        /// Contains the <see cref="T:TextReader"/> pointing to the CSV file.
        /// </summary>
        TextReader reader;

        /// <summary>
        /// Contains the buffer size.
        /// </summary>
        int bufferSize;

        /// <summary>
        /// Contains the comment character indicating that a line is commented out.
        /// </summary>
        char comment;

        /// <summary>
        /// Contains the escape character letting insert quotation characters inside a quoted field.
        /// </summary>
        char escape;

        /// <summary>
        /// Contains the delimiter character separating each field.
        /// </summary>
        char delimiter;

        /// <summary>
        /// Contains the quotation character wrapping every field.
        /// </summary>
        char quote;

        /// <summary>
        /// Determines which values should be trimmed.
        /// </summary>
        ValueTrimmingOptions trimmingOptions;

        /// <summary>
        /// Indicates if field names are located on the first non commented line.
        /// </summary>
        bool hasHeaders;

        #endregion

        #region State

        /// <summary>
        /// Indicates if the class is initialized.
        /// </summary>
        bool initialized;

        /// <summary>
        /// Contains the field headers.
        /// </summary>
        string[] fieldHeaders;

        /// <summary>
        /// Contains the dictionary of field indexes by header. The key is the field name and the value is its index.
        /// </summary>
        Dictionary<string, int> fieldHeaderIndexes;

        /// <summary>
        /// Contains the current record index in the CSV file.
        /// A value of <see cref="M:Int32.MinValue"/> means that the reader has not been initialized yet.
        /// Otherwise, a negative value means that no record has been read yet.
        /// </summary>
        long currentRecordIndex;

        /// <summary>
        /// Contains the starting position of the next unread field.
        /// </summary>
        int nextFieldStart;

        /// <summary>
        /// Contains the index of the next unread field.
        /// </summary>
        int nextFieldIndex;

        /// <summary>
        /// Contains the array of the field values for the current record.
        /// A null value indicates that the field have not been parsed.
        /// </summary>
        string[] fields;

        /// <summary>
        /// Contains the maximum number of fields to retrieve for each record.
        /// </summary>
        int fieldCount;

        /// <summary>
        /// Contains the read buffer.
        /// </summary>
        char[] buffer;

        /// <summary>
        /// Contains the current read buffer length.
        /// </summary>
        int bufferLength;

        /// <summary>
        /// Indicates if the end of the reader has been reached.
        /// </summary>
        bool eof;

        /// <summary>
        /// Indicates if the last read operation reached an EOL character.
        /// </summary>
        bool eol;

        /// <summary>
        /// Indicates if the first record is in cache.
        /// This can happen when initializing a reader with no headers
        /// because one record must be read to get the field count automatically
        /// </summary>
        bool firstRecordInCache;

        /// <summary>
        /// Indicates if one or more field are missing for the current record.
        /// Resets after each successful record read.
        /// </summary>
        bool missingFieldFlag;

        /// <summary>
        /// Indicates if a parse error occured for the current record.
        /// Resets after each successful record read.
        /// </summary>
        bool parseErrorFlag;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		Cannot read from <paramref name="reader"/>.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders)
            : this(reader, hasHeaders, DefaultDelimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <param name="bufferSize">The buffer size in bytes.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		Cannot read from <paramref name="reader"/>.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders, int bufferSize)
            : this(reader, hasHeaders, DefaultDelimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		Cannot read from <paramref name="reader"/>.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders, char delimiter)
            : this(reader, hasHeaders, delimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
        /// <param name="bufferSize">The buffer size in bytes.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		Cannot read from <paramref name="reader"/>.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders, char delimiter, int bufferSize)
            : this(reader, hasHeaders, delimiter, DefaultQuote, DefaultEscape, DefaultComment, ValueTrimmingOptions.UnquotedOnly, bufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
        /// <param name="quote">The quotation character wrapping every field (default is ''').</param>
        /// <param name="escape">
        /// The escape character letting insert quotation characters inside a quoted field (default is '\').
        /// If no escape character, set to '\0' to gain some performance.
        /// </param>
        /// <param name="comment">The comment character indicating that a line is commented out (default is '#').</param>
        /// <param name="trimmingOptions">Determines which values should be trimmed.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		Cannot read from <paramref name="reader"/>.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders, char delimiter, char quote, char escape, char comment, ValueTrimmingOptions trimmingOptions)
            : this(reader, hasHeaders, delimiter, quote, escape, comment, trimmingOptions, DefaultBufferSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CsvReader class.
        /// </summary>
        /// <param name="reader">A <see cref="T:TextReader"/> pointing to the CSV file.</param>
        /// <param name="hasHeaders"><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</param>
        /// <param name="delimiter">The delimiter character separating each field (default is ',').</param>
        /// <param name="quote">The quotation character wrapping every field (default is ''').</param>
        /// <param name="escape">
        /// The escape character letting insert quotation characters inside a quoted field (default is '\').
        /// If no escape character, set to '\0' to gain some performance.
        /// </param>
        /// <param name="comment">The comment character indicating that a line is commented out (default is '#').</param>
        /// <param name="trimmingOptions">Determines which values should be trimmed.</param>
        /// <param name="bufferSize">The buffer size in bytes.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="reader"/> is a <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<paramref name="bufferSize"/> must be 1 or more.
        /// </exception>
        public CsvDataReader(TextReader reader, bool hasHeaders, char delimiter, char quote, char escape, char comment, ValueTrimmingOptions trimmingOptions, int bufferSize)
        {
#if DEBUG
            allocStack = new System.Diagnostics.StackTrace();
#endif

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, ExceptionMessage.BufferSizeTooSmall);

            this.bufferSize = bufferSize;

            if (reader is StreamReader)
            {
                var stream = ((StreamReader)reader).BaseStream;

                if (stream.CanSeek)
                {
                    // Handle bad implementations returning 0 or less
                    if (stream.Length > 0)
                        this.bufferSize = (int)Math.Min(bufferSize, stream.Length);
                }
            }

            this.reader = reader;
            this.delimiter = delimiter;
            this.quote = quote;
            this.escape = escape;
            this.comment = comment;

            this.hasHeaders = hasHeaders;
            this.trimmingOptions = trimmingOptions;
            SupportsMultiline = true;
            SkipEmptyLines = true;
            DefaultHeaderName = "Column";

            currentRecordIndex = -1;
            DefaultParseErrorAction = ParseErrorAction.RaiseEvent;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when there is an error while parsing the CSV stream.
        /// </summary>
        public event EventHandler<ParseErrorEventArgs> ParseError;

        /// <summary>
        /// Raises the <see cref="M:ParseError"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ParseErrorEventArgs"/> that contains the event data.</param>
        protected virtual void OnParseError(ParseErrorEventArgs e) => ParseError?.Invoke(this, e);

        #endregion

        #region Properties

        #region Settings

        /// <summary>
        /// Gets the comment character indicating that a line is commented out.
        /// </summary>
        /// <value>The comment character indicating that a line is commented out.</value>
        public char Comment => comment;

        /// <summary>
        /// Gets the escape character letting insert quotation characters inside a quoted field.
        /// </summary>
        /// <value>The escape character letting insert quotation characters inside a quoted field.</value>
        public char Escape => escape;

        /// <summary>
        /// Gets the delimiter character separating each field.
        /// </summary>
        /// <value>The delimiter character separating each field.</value>
        public char Delimiter => delimiter;

        /// <summary>
        /// Gets the quotation character wrapping every field.
        /// </summary>
        /// <value>The quotation character wrapping every field.</value>
        public char Quote => quote;

        /// <summary>
        /// Indicates if field names are located on the first non commented line.
        /// </summary>
        /// <value><see langword="true"/> if field names are located on the first non commented line, otherwise, <see langword="false"/>.</value>
        public bool HasHeaders => hasHeaders;

        /// <summary>
        /// Indicates if spaces at the start and end of a field are trimmed.
        /// </summary>
        /// <value><see langword="true"/> if spaces at the start and end of a field are trimmed, otherwise, <see langword="false"/>.</value>
        public ValueTrimmingOptions TrimmingOption => trimmingOptions;

        /// <summary>
        /// Gets the buffer size.
        /// </summary>
        public int BufferSize => bufferSize;

        /// <summary>
        /// Gets or sets the default action to take when a parsing error has occured.
        /// </summary>
        /// <value>The default action to take when a parsing error has occured.</value>
        public ParseErrorAction DefaultParseErrorAction { get; set; }

        /// <summary>
        /// Gets or sets the action to take when a field is missing.
        /// </summary>
        /// <value>The action to take when a field is missing.</value>
        public MissingFieldAction MissingFieldAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the reader supports multiline fields.
        /// </summary>
        /// <value>A value indicating if the reader supports multiline field.</value>
        public bool SupportsMultiline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the reader will skip empty lines.
        /// </summary>
        /// <value>A value indicating if the reader will skip empty lines.</value>
        public bool SkipEmptyLines { get; set; }

        /// <summary>
        /// Gets or sets the default header name when it is an empty string or only whitespaces.
        /// The header index will be appended to the specified name.
        /// </summary>
        /// <value>The default header name when it is an empty string or only whitespaces.</value>
        public string DefaultHeaderName { get; set; }

        #endregion

        #region State

        /// <summary>
        /// Gets the maximum number of fields to retrieve for each record.
        /// </summary>
        /// <value>The maximum number of fields to retrieve for each record.</value>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public int FieldCount
        {
            get
            {
                EnsureInitialize();
                return fieldCount;
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current stream position is at the end of the stream.
        /// </summary>
        /// <value><see langword="true"/> if the current stream position is at the end of the stream; otherwise <see langword="false"/>.</value>
        public virtual bool EndOfStream => eof;

        /// <summary>
        /// Gets the field headers.
        /// </summary>
        /// <returns>The field headers or an empty array if headers are not supported.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public string[] GetFieldHeaders()
        {
            EnsureInitialize();
            Debug.Assert(this.fieldHeaders != null, "Field headers must be non null.");

            var fieldHeaders = new string[this.fieldHeaders.Length];

            for (var i = 0; i < fieldHeaders.Length; i++)
                fieldHeaders[i] = this.fieldHeaders[i];

            return fieldHeaders;
        }

        /// <summary>
        /// Gets the current record index in the CSV file.
        /// </summary>
        /// <value>The current record index in the CSV file.</value>
        public virtual long CurrentRecordIndex => currentRecordIndex;

        /// <summary>
        /// Indicates if one or more field are missing for the current record.
        /// Resets after each successful record read.
        /// </summary>
        public bool MissingFieldFlag => missingFieldFlag;

        /// <summary>
        /// Indicates if a parse error occured for the current record.
        /// Resets after each successful record read.
        /// </summary>
        public bool ParseErrorFlag => parseErrorFlag;

        #endregion

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the field with the specified name and record position. <see cref="M:hasHeaders"/> must be <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The field with the specified name and record position.
        /// </value>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="field"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="T:InvalidOperationException">
        ///	The CSV does not have headers (<see cref="M:HasHeaders"/> property is <see langword="false"/>).
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		<paramref name="field"/> not found.
        /// </exception>
        /// <exception cref="T:ArgumentOutOfRangeException">
        ///		Record index must be > 0.
        /// </exception>
        /// <exception cref="T:InvalidOperationException">
        ///		Cannot move to a previous record in forward-only mode.
        /// </exception>
        /// <exception cref="T:EndOfStreamException">
        ///		Cannot read record at <paramref name="record"/>.
        ///	</exception>
        ///	<exception cref="T:MalformedCsvException">
        ///		The CSV appears to be corrupt at the current position.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public string this[int record, string field]
        {
            get
            {
                if (!MoveTo(record))
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.CannotReadRecordAtIndex, record));

                return this[field];
            }
        }

        /// <summary>
        /// Gets the field at the specified index and record position.
        /// </summary>
        /// <value>
        /// The field at the specified index and record position.
        /// A <see langword="null"/> is returned if the field cannot be found for the record.
        /// </value>
        /// <exception cref="T:ArgumentOutOfRangeException">
        ///		<paramref name="field"/> must be included in [0, <see cref="M:FieldCount"/>[.
        /// </exception>
        /// <exception cref="T:ArgumentOutOfRangeException">
        ///		Record index must be > 0.
        /// </exception>
        /// <exception cref="T:InvalidOperationException">
        ///		Cannot move to a previous record in forward-only mode.
        /// </exception>
        /// <exception cref="T:EndOfStreamException">
        ///		Cannot read record at <paramref name="record"/>.
        /// </exception>
        /// <exception cref="T:MalformedCsvException">
        ///		The CSV appears to be corrupt at the current position.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public string this[int record, int field]
        {
            get
            {
                if (!MoveTo(record))
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.CannotReadRecordAtIndex, record));

                return this[field];
            }
        }

        /// <summary>
        /// Gets the field with the specified name. <see cref="M:hasHeaders"/> must be <see langword="true"/>.
        /// </summary>
        /// <value>
        /// The field with the specified name.
        /// </value>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="field"/> is <see langword="null"/> or an empty string.
        /// </exception>
        /// <exception cref="T:InvalidOperationException">
        ///	The CSV does not have headers (<see cref="M:HasHeaders"/> property is <see langword="false"/>).
        /// </exception>
        /// <exception cref="T:ArgumentException">
        ///		<paramref name="field"/> not found.
        /// </exception>
        /// <exception cref="T:MalformedCsvException">
        ///		The CSV appears to be corrupt at the current position.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public string this[string field]
        {
            get
            {
                if (string.IsNullOrEmpty(field))
                    throw new ArgumentNullException(nameof(field));

                if (!hasHeaders)
                    throw new InvalidOperationException(ExceptionMessage.NoHeaders);

                var index = GetFieldIndex(field);

                if (index < 0)
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldHeaderNotFound, field), "field");

                return this[index];
            }
        }

        /// <summary>
        /// Gets the field at the specified index.
        /// </summary>
        /// <value>The field at the specified index.</value>
        /// <exception cref="T:ArgumentOutOfRangeException">
        ///		<paramref name="field"/> must be included in [0, <see cref="M:FieldCount"/>[.
        /// </exception>
        /// <exception cref="T:InvalidOperationException">
        ///		No record read yet. Call ReadLine() first.
        /// </exception>
        /// <exception cref="T:MalformedCsvException">
        ///		The CSV appears to be corrupt at the current position.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public virtual string this[int field]
        {
            get
            {
                return ReadField(field, initializing: false, discardValue: false);
            }
        }

        #endregion

        #region Methods

        #region EnsureInitialize

        /// <summary>
        /// Ensures that the reader is initialized.
        /// </summary>
        void EnsureInitialize()
        {
            if (!initialized)
                ReadNextRecord(onlyReadHeaders: true, skipToNextLine: false);

            Debug.Assert(fieldHeaders != null);
            Debug.Assert(fieldHeaders.Length > 0 || (fieldHeaders.Length == 0 && fieldHeaderIndexes == null));
        }

        #endregion

        #region GetFieldIndex

        /// <summary>
        /// Gets the field index for the provided header.
        /// </summary>
        /// <param name="header">The header to look for.</param>
        /// <returns>The field index for the provided header. -1 if not found.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public int GetFieldIndex(string header)
        {
            EnsureInitialize();

            if (fieldHeaderIndexes != null && fieldHeaderIndexes.TryGetValue(header, out var index))
                return index;
            else
                return -1;
        }

        #endregion

        #region CopyCurrentRecordTo

        /// <summary>
        /// Copies the field array of the current record to a one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the fields of the current record.</param>	
        public void CopyCurrentRecordTo(string[] array) => CopyCurrentRecordTo(array, 0);

        /// <summary>
        /// Copies the field array of the current record to a one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array"> The one-dimensional <see cref="T:Array"/> that is the destination of the fields of the current record.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:ArgumentNullException">
        ///		<paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="T:ArgumentOutOfRangeException">
        ///		<paramref name="index"/> is les than zero or is equal to or greater than the length <paramref name="array"/>. 
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///	No current record.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///		The number of fields in the record is greater than the available space from <paramref name="index"/> to the end of <paramref name="array"/>.
        /// </exception>
        public void CopyCurrentRecordTo(string[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException("index", index, string.Empty);

            if (currentRecordIndex < 0 || !initialized)
                throw new InvalidOperationException(ExceptionMessage.NoCurrentRecord);

            if (array.Length - index < fieldCount)
                throw new ArgumentException(ExceptionMessage.NotEnoughSpaceInArray, "array");

            for (var i = 0; i < fieldCount; i++)
            {
                if (parseErrorFlag) array[index + i] = null;
                else array[index + i] = this[i];
            }
        }

        #endregion

        #region GetCurrentRawData

        /// <summary>
        /// Gets the current raw CSV data.
        /// </summary>
        /// <remarks>Used for exception handling purpose.</remarks>
        /// <returns>The current raw CSV data.</returns>
        public string GetCurrentRawData()
        {
            if (buffer != null && bufferLength > 0)
                return new string(buffer, 0, bufferLength);
            else
                return string.Empty;
        }

        #endregion

        #region IsWhiteSpace

        /// <summary>
        /// Indicates whether the specified Unicode character is categorized as white space.
        /// </summary>
        bool IsWhiteSpace(char character)
        {
            // Handle cases where the delimiter is a whitespace (e.g. tab)
            if (character == delimiter)
                return false;
            else
            {
                // See char.IsLatin1(char c) in Reflector
                if (character <= '\x00ff')
                    return (character == ' ' || character == '\t');
                else
                    return (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.SpaceSeparator);
            }
        }

        #endregion

        #region MoveTo

        /// <summary>
        /// Moves to the specified record index.
        /// </summary>
        /// <param name="record">The record index.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public virtual bool MoveTo(long record)
        {
            if (record < currentRecordIndex) return false;

            // Get number of record to read
            var offset = record - currentRecordIndex;

            while (offset > 0)
            {
                if (!ReadNextRecord()) return false;

                offset--;
            }

            return true;
        }

        #endregion

        #region ParseNewLine

        /// <summary>
        /// Parses a new line delimiter.
        /// </summary>
        /// <param name="pos">The starting position of the parsing. Will contain the resulting end position.</param>
        /// <returns><see langword="true"/> if a new line delimiter was found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        bool ParseNewLine(ref int pos)
        {
            Debug.Assert(pos <= bufferLength);

            // Check if already at the end of the buffer
            if (pos == bufferLength)
            {
                pos = 0;

                if (!ReadBuffer()) return false;
            }

            var character = buffer[pos];

            // Treat \r as new line only if it's not the delimiter

            if (character == '\r' && delimiter != '\r')
            {
                pos++;

                // Skip following \n (if there is one)

                if (pos < bufferLength)
                {
                    if (buffer[pos] == '\n') pos++;
                }
                else
                {
                    if (ReadBuffer())
                    {
                        if (buffer[0] == '\n') pos = 1;
                        else pos = 0;
                    }
                }

                if (pos >= bufferLength)
                {
                    ReadBuffer();
                    pos = 0;
                }

                return true;
            }
            else if (character == '\n')
            {
                pos++;

                if (pos >= bufferLength)
                {
                    ReadBuffer();
                    pos = 0;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the character at the specified position is a new line delimiter.
        /// </summary>
        /// <param name="pos">The position of the character to verify.</param>
        /// <returns>
        /// 	<see langword="true"/> if the character at the specified position is a new line delimiter; otherwise, <see langword="false"/>.
        /// </returns>
        bool IsNewLine(int pos)
        {
            Debug.Assert(pos < bufferLength);

            var c = buffer[pos];

            if (c == '\n') return true;
            else if (c == '\r' && delimiter != '\r') return true;
            else return false;
        }

        #endregion

        #region ReadBuffer

        /// <summary>
        /// Fills the buffer with data from the reader.
        /// </summary>
        /// <returns><see langword="true"/> if data was successfully read; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        bool ReadBuffer()
        {
            if (eof) return false;

            CheckDisposed();

            bufferLength = reader.Read(buffer, 0, bufferSize);

            if (bufferLength > 0) return true;
            else
            {
                eof = true;
                buffer = null;

                return false;
            }
        }

        #endregion

        #region ReadField

        /// <summary>
        /// Reads the field at the specified index.
        /// Any unread fields with an inferior index will also be read as part of the required parsing.
        /// </summary>
        /// <param name="field">The field index.</param>
        /// <param name="initializing">Indicates if the reader is currently initializing.</param>
        /// <param name="discardValue">Indicates if the value(s) are discarded.</param>
        /// <returns>
        /// The field at the specified index. 
        /// A <see langword="null"/> indicates that an error occured or that the last field has been reached during initialization.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<paramref name="field"/> is out of range.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///		There is no current record.
        /// </exception>
        /// <exception cref="MissingFieldCsvException">
        ///		The CSV data appears to be missing a field.
        /// </exception>
        /// <exception cref="MalformedCsvException">
        ///		The CSV data appears to be malformed.
        /// </exception>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        string ReadField(int field, bool initializing, bool discardValue)
        {
            if (!initializing)
            {
                if (field < 0 || field >= fieldCount)
                    throw new ArgumentOutOfRangeException("field", field, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, field));

                if (currentRecordIndex < 0)
                    throw new InvalidOperationException(ExceptionMessage.NoCurrentRecord);

                // Directly return field if cached
                if (fields[field] != null)
                    return fields[field];
                else if (missingFieldFlag)
                    return HandleMissingField(null, field, ref nextFieldStart);
            }

            CheckDisposed();

            var index = nextFieldIndex;

            while (index < field + 1)
            {
                // Handle case where stated start of field is past buffer
                // This can occur because _nextFieldStart is simply 1 + last char position of previous field
                if (nextFieldStart == bufferLength)
                {
                    nextFieldStart = 0;

                    // Possible EOF will be handled later (see Handle_EOF1)
                    ReadBuffer();
                }

                string value = null;

                if (missingFieldFlag)
                {
                    value = HandleMissingField(value, index, ref nextFieldStart);
                }
                else if (nextFieldStart == bufferLength)
                {
                    // Handle_EOF1: Handle EOF here

                    // If current field is the requested field, then the value of the field is "" as in "f1,f2,f3,(\s*)"
                    // otherwise, the CSV is malformed

                    if (index == field)
                    {
                        if (!discardValue)
                        {
                            value = string.Empty;
                            fields[index] = value;
                        }

                        missingFieldFlag = true;
                    }
                    else
                    {
                        value = HandleMissingField(value, index, ref nextFieldStart);
                    }
                }
                else
                {
                    // Trim spaces at start
                    if ((trimmingOptions & ValueTrimmingOptions.UnquotedOnly) != 0)
                        SkipWhiteSpaces(ref nextFieldStart);

                    if (eof)
                    {
                        value = string.Empty;
                        fields[field] = value;
                    }
                    else if (buffer[nextFieldStart] != quote)
                    {
                        // Non-quoted field

                        var start = nextFieldStart;
                        var pos = nextFieldStart;

                        for (; ; )
                        {
                            while (pos < bufferLength)
                            {
                                var c = buffer[pos];

                                if (c == delimiter)
                                {
                                    nextFieldStart = pos + 1;

                                    break;
                                }
                                else if (c == '\r' || c == '\n')
                                {
                                    nextFieldStart = pos;
                                    eol = true;

                                    break;
                                }
                                else
                                    pos++;
                            }

                            if (pos < bufferLength) break;
                            else
                            {
                                if (!discardValue)
                                    value += new string(buffer, start, pos - start);

                                start = 0;
                                pos = 0;
                                nextFieldStart = 0;

                                if (!ReadBuffer()) break;
                            }
                        }

                        if (!discardValue)
                        {
                            if ((trimmingOptions & ValueTrimmingOptions.UnquotedOnly) == 0)
                            {
                                if (!eof && pos > start)
                                    value += new string(buffer, start, pos - start);
                            }
                            else
                            {
                                if (!eof && pos > start)
                                {
                                    // Do the trimming
                                    pos--;
                                    while (pos > -1 && IsWhiteSpace(buffer[pos]))
                                        pos--;
                                    pos++;

                                    if (pos > 0)
                                        value += new string(buffer, start, pos - start);
                                }
                                else
                                    pos = -1;

                                // If pos <= 0, that means the trimming went past buffer start,
                                // and the concatenated value needs to be trimmed too.
                                if (pos <= 0)
                                {
                                    pos = (value == null ? -1 : value.Length - 1);

                                    // Do the trimming
                                    while (pos > -1 && IsWhiteSpace(value[pos]))
                                        pos--;

                                    pos++;

                                    if (pos > 0 && pos != value.Length)
                                        value = value.Substring(0, pos);
                                }
                            }

                            if (value == null) value = string.Empty;
                        }

                        if (eol || eof)
                        {
                            eol = ParseNewLine(ref nextFieldStart);

                            // Reaching a new line is ok as long as the parser is initializing or it is the last field
                            if (!initializing && index != fieldCount - 1)
                            {
                                if (value != null && value.Length == 0)
                                    value = null;

                                value = HandleMissingField(value, index, ref nextFieldStart);
                            }
                        }

                        if (!discardValue) fields[index] = value;
                    }
                    else
                    {
                        // Quoted field

                        // Skip quote
                        var start = nextFieldStart + 1;
                        var pos = start;

                        var quoted = true;
                        var escaped = false;

                        if ((trimmingOptions & ValueTrimmingOptions.QuotedOnly) != 0)
                        {
                            SkipWhiteSpaces(ref start);
                            pos = start;
                        }

                        for (; ; )
                        {
                            while (pos < bufferLength)
                            {
                                var c = buffer[pos];

                                if (escaped)
                                {
                                    escaped = false;
                                    start = pos;
                                }
                                // IF current char is escape AND (escape and quote are different OR next char is a quote)
                                else if (c == escape && (escape != quote || (pos + 1 < bufferLength && buffer[pos + 1] == quote) || (pos + 1 == bufferLength && reader.Peek() == quote)))
                                {
                                    if (!discardValue)
                                        value += new string(buffer, start, pos - start);

                                    escaped = true;
                                }
                                else if (c == quote)
                                {
                                    quoted = false;
                                    break;
                                }

                                pos++;
                            }

                            if (!quoted) break;
                            else
                            {
                                if (!discardValue && !escaped)
                                    value += new string(buffer, start, pos - start);

                                start = 0;
                                pos = 0;
                                nextFieldStart = 0;

                                if (!ReadBuffer())
                                {
                                    HandleParseError(new MalformedCsvException(GetCurrentRawData(), nextFieldStart, Math.Max(0, currentRecordIndex), index), ref nextFieldStart);
                                    return null;
                                }
                            }
                        }

                        if (!eof)
                        {
                            // Append remaining parsed buffer content
                            if (!discardValue && pos > start)
                                value += new string(buffer, start, pos - start);

                            if (!discardValue && value != null && (trimmingOptions & ValueTrimmingOptions.QuotedOnly) != 0)
                            {
                                var newLength = value.Length;
                                while (newLength > 0 && IsWhiteSpace(value[newLength - 1]))
                                    newLength--;

                                if (newLength < value.Length)
                                    value = value.Substring(0, newLength);
                            }

                            // Skip quote
                            nextFieldStart = pos + 1;

                            // Skip whitespaces between the quote and the delimiter/eol
                            SkipWhiteSpaces(ref nextFieldStart);

                            // Skip delimiter
                            bool delimiterSkipped;

                            if (nextFieldStart < bufferLength && buffer[nextFieldStart] == delimiter)
                            {
                                nextFieldStart++;
                                delimiterSkipped = true;
                            }
                            else
                            {
                                delimiterSkipped = false;
                            }

                            // Skip new line delimiter if initializing or last field
                            // (if the next field is missing, it will be caught when parsed)
                            if (!eof && !delimiterSkipped && (initializing || index == fieldCount - 1))
                                eol = ParseNewLine(ref nextFieldStart);

                            // If no delimiter is present after the quoted field and it is not the last field, then it is a parsing error
                            if (!delimiterSkipped && !eof && !(eol || IsNewLine(nextFieldStart)))
                                HandleParseError(new MalformedCsvException(GetCurrentRawData(), nextFieldStart, Math.Max(0, currentRecordIndex), index), ref nextFieldStart);
                        }

                        if (!discardValue)
                        {
                            if (value == null) value = string.Empty;

                            fields[index] = value;
                        }
                    }
                }

                nextFieldIndex = Math.Max(index + 1, nextFieldIndex);

                if (index == field)
                {
                    // If initializing, return null to signify the last field has been reached

                    if (initializing)
                    {
                        if (eol || eof) return null;
                        else
                            return string.IsNullOrEmpty(value) ? string.Empty : value;
                    }
                    else
                        return value;
                }

                index++;
            }

            // Getting here is bad ...
            HandleParseError(new MalformedCsvException(GetCurrentRawData(), nextFieldStart, Math.Max(0, currentRecordIndex), index), ref nextFieldStart);
            return null;
        }

        #endregion

        #region ReadNextRecord

        /// <summary>
        /// Reads the next record.
        /// </summary>
        /// <returns><see langword="true"/> if a record has been successfully reads; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public bool ReadNextRecord() => ReadNextRecord(onlyReadHeaders: false, skipToNextLine: false);

        /// <summary>
        /// Reads the next record.
        /// </summary>
        /// <param name="onlyReadHeaders">
        /// Indicates if the reader will proceed to the next record after having read headers.
        /// <see langword="true"/> if it stops after having read headers; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="skipToNextLine">
        /// Indicates if the reader will skip directly to the next line without parsing the current one. 
        /// To be used when an error occurs.
        /// </param>
        /// <returns><see langword="true"/> if a record has been successfully reads; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        protected virtual bool ReadNextRecord(bool onlyReadHeaders, bool skipToNextLine)
        {
            if (eof)
            {
                if (firstRecordInCache)
                {
                    firstRecordInCache = false;
                    currentRecordIndex++;

                    return true;
                }
                else
                    return false;
            }

            CheckDisposed();

            if (!initialized)
            {
                buffer = new char[bufferSize];

                // will be replaced if and when headers are read
                fieldHeaders = new string[0];

                if (!ReadBuffer()) return false;

                if (!SkipEmptyAndCommentedLines(ref nextFieldStart))
                    return false;

                // Keep growing _fields array until the last field has been found
                // and then resize it to its final correct size

                fieldCount = 0;
                fields = new string[16];

                while (ReadField(fieldCount, initializing: true, discardValue: false) != null)
                {
                    if (parseErrorFlag)
                    {
                        fieldCount = 0;
                        Array.Clear(fields, 0, fields.Length);
                        parseErrorFlag = false;
                        nextFieldIndex = 0;
                    }
                    else
                    {
                        fieldCount++;

                        if (fieldCount == fields.Length)
                            Array.Resize<string>(ref fields, (fieldCount + 1) * 2);
                    }
                }

                // _fieldCount contains the last field index, but it must contains the field count,
                // so increment by 1
                fieldCount++;

                if (fields.Length != fieldCount)
                    Array.Resize<string>(ref fields, fieldCount);

                initialized = true;

                // If headers are present, call ReadNextRecord again
                if (hasHeaders)
                {
                    // Don't count first record as it was the headers
                    currentRecordIndex = -1;

                    firstRecordInCache = false;

                    fieldHeaders = new string[fieldCount];
                    fieldHeaderIndexes = new Dictionary<string, int>(fieldCount, fieldHeaderComparer);

                    for (var i = 0; i < fields.Length; i++)
                    {
                        var headerName = fields[i];
                        if (string.IsNullOrEmpty(headerName) || headerName.Trim().Length == 0)
                            headerName = DefaultHeaderName + i.ToString();

                        fieldHeaders[i] = headerName;
                        fieldHeaderIndexes.Add(headerName, i);
                    }

                    // Proceed to first record
                    if (!onlyReadHeaders)
                    {
                        // Calling again ReadNextRecord() seems to be simpler,
                        // but in fact would probably cause many subtle bugs because a derived class does not expect a recursive behavior
                        // so simply do what is needed here and no more.

                        if (!SkipEmptyAndCommentedLines(ref nextFieldStart))
                            return false;

                        Array.Clear(fields, 0, fields.Length);
                        nextFieldIndex = 0;
                        eol = false;

                        currentRecordIndex++;
                        return true;
                    }
                }
                else
                {
                    if (onlyReadHeaders)
                    {
                        firstRecordInCache = true;
                        currentRecordIndex = -1;
                    }
                    else
                    {
                        firstRecordInCache = false;
                        currentRecordIndex = 0;
                    }
                }
            }
            else
            {
                if (skipToNextLine)
                    SkipToNextLine(ref nextFieldStart);
                else if (currentRecordIndex > -1 && !missingFieldFlag)
                {
                    // If not already at end of record, move there
                    if (!eol && !eof)
                    {
                        if (!SupportsMultiline)
                            SkipToNextLine(ref nextFieldStart);
                        else
                        {
                            // a dirty trick to handle the case where extra fields are present
                            while (ReadField(nextFieldIndex, initializing: true, discardValue: true) != null)
                            {
                            }
                        }
                    }
                }

                if (!firstRecordInCache && !SkipEmptyAndCommentedLines(ref nextFieldStart))
                    return false;

                if (hasHeaders || !firstRecordInCache)
                    eol = false;

                // Check to see if the first record is in cache.
                // This can happen when initializing a reader with no headers
                // because one record must be read to get the field count automatically
                if (firstRecordInCache)
                    firstRecordInCache = false;
                else
                {
                    Array.Clear(fields, 0, fields.Length);
                    nextFieldIndex = 0;
                }

                missingFieldFlag = false;
                parseErrorFlag = false;
                currentRecordIndex++;
            }

            return true;
        }

        #endregion

        #region SkipEmptyAndCommentedLines

        /// <summary>
        /// Skips empty and commented lines.
        /// If the end of the buffer is reached, its content be discarded and filled again from the reader.
        /// </summary>
        /// <param name="pos">
        /// The position in the buffer where to start parsing. 
        /// Will contains the resulting position after the operation.
        /// </param>
        /// <returns><see langword="true"/> if the end of the reader has not been reached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        bool SkipEmptyAndCommentedLines(ref int pos)
        {
            if (pos < bufferLength)
                DoSkipEmptyAndCommentedLines(ref pos);

            while (pos >= bufferLength && !eof)
            {
                if (ReadBuffer())
                {
                    pos = 0;
                    DoSkipEmptyAndCommentedLines(ref pos);
                }
                else
                    return false;
            }

            return !eof;
        }

        /// <summary>
        /// <para>Worker method.</para>
        /// <para>Skips empty and commented lines.</para>
        /// </summary>
        /// <param name="pos">
        /// The position in the buffer where to start parsing. 
        /// Will contains the resulting position after the operation.
        /// </param>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        void DoSkipEmptyAndCommentedLines(ref int pos)
        {
            while (pos < bufferLength)
            {
                if (buffer[pos] == comment)
                {
                    pos++;
                    SkipToNextLine(ref pos);
                }
                else if (SkipEmptyLines && ParseNewLine(ref pos))
                    continue;
                else
                    break;
            }
        }

        #endregion

        #region SkipWhiteSpaces

        /// <summary>
        /// Skips whitespace characters.
        /// </summary>
        /// <param name="pos">The starting position of the parsing. Will contain the resulting end position.</param>
        /// <returns><see langword="true"/> if the end of the reader has not been reached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        bool SkipWhiteSpaces(ref int pos)
        {
            for (; ; )
            {
                while (pos < bufferLength && IsWhiteSpace(buffer[pos]))
                    pos++;

                if (pos < bufferLength) break;
                else
                {
                    pos = 0;

                    if (!ReadBuffer()) return false;
                }
            }

            return true;
        }

        #endregion

        #region SkipToNextLine

        /// <summary>
        /// Skips ahead to the next NewLine character.
        /// If the end of the buffer is reached, its content be discarded and filled again from the reader.
        /// </summary>
        /// <param name="pos">
        /// The position in the buffer where to start parsing. 
        /// Will contains the resulting position after the operation.
        /// </param>
        /// <returns><see langword="true"/> if the end of the reader has not been reached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        bool SkipToNextLine(ref int pos)
        {
            // ((pos = 0) == 0) is a little trick to reset position inline
            while ((pos < bufferLength || (ReadBuffer() && ((pos = 0) == 0))) && !ParseNewLine(ref pos))
                pos++;

            return !eof;
        }

        #endregion

        #region HandleParseError

        /// <summary>
        /// Handles a parsing error.
        /// </summary>
        /// <param name="error">The parsing error that occured.</param>
        /// <param name="pos">The current position in the buffer.</param>
        /// <exception cref="ArgumentNullException">
        ///	<paramref name="error"/> is <see langword="null"/>.
        /// </exception>
        void HandleParseError(MalformedCsvException error, ref int pos)
        {
            if (error == null)
                throw new ArgumentNullException("error");

            parseErrorFlag = true;

            switch (DefaultParseErrorAction)
            {
                case ParseErrorAction.ThrowException:
                    throw error;

                case ParseErrorAction.RaiseEvent:
                    var e = new ParseErrorEventArgs(error, ParseErrorAction.ThrowException);
                    OnParseError(e);

                    switch (e.Action)
                    {
                        case ParseErrorAction.ThrowException:
                            throw e.Error;

                        case ParseErrorAction.RaiseEvent:
                            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.ParseErrorActionInvalidInsideParseErrorEvent, e.Action), e.Error);

                        case ParseErrorAction.AdvanceToNextLine:
                            // already at EOL when fields are missing, so don't skip to next line in that case
                            if (!missingFieldFlag && pos >= 0)
                                SkipToNextLine(ref pos);
                            break;

                        default:
                            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.ParseErrorActionNotSupported, e.Action), e.Error);
                    }

                    break;

                case ParseErrorAction.AdvanceToNextLine:
                    // already at EOL when fields are missing, so don't skip to next line in that case
                    if (!missingFieldFlag && pos >= 0)
                        SkipToNextLine(ref pos);
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.ParseErrorActionNotSupported, DefaultParseErrorAction), error);
            }
        }

        #endregion

        #region HandleMissingField

        /// <summary>
        /// Handles a missing field error.
        /// </summary>
        /// <param name="value">The partially parsed value, if available.</param>
        /// <param name="fieldIndex">The missing field index.</param>
        /// <param name="currentPosition">The current position in the raw data.</param>
        /// <returns>
        /// The resulting value according to <see cref="M:MissingFieldAction"/>.
        /// If the action is set to <see cref="T:MissingFieldAction.TreatAsParseError"/>,
        /// then the parse error will be handled according to <see cref="DefaultParseErrorAction"/>.
        /// </returns>
        string HandleMissingField(string value, int fieldIndex, ref int currentPosition)
        {
            if (fieldIndex < 0 || fieldIndex >= fieldCount)
                throw new ArgumentOutOfRangeException("fieldIndex", fieldIndex, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, fieldIndex));

            missingFieldFlag = true;

            for (var i = fieldIndex + 1; i < fieldCount; i++)
                fields[i] = null;

            if (value != null)
                return value;
            else
            {
                switch (MissingFieldAction)
                {
                    case MissingFieldAction.ParseError:
                        HandleParseError(new MissingFieldCsvException(GetCurrentRawData(), currentPosition, Math.Max(0, currentRecordIndex), fieldIndex), ref currentPosition);
                        return value;

                    case MissingFieldAction.ReplaceByEmpty:
                        return string.Empty;

                    case MissingFieldAction.ReplaceByNull:
                        return null;

                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.MissingFieldActionNotSupported, MissingFieldAction));
                }
            }
        }

        #endregion

        #endregion

        #region IDataReader support methods

        /// <summary>
        /// Validates the state of the data reader.
        /// </summary>
        /// <param name="validations">The validations to accomplish.</param>
        /// <exception cref="InvalidOperationException">
        ///	No current record.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///	This operation is invalid when the reader is closed.
        /// </exception>
        void ValidateDataReader(DataReaderValidations validations)
        {
            if ((validations & DataReaderValidations.IsInitialized) != 0 && !initialized)
                throw new InvalidOperationException(ExceptionMessage.NoCurrentRecord);

            if ((validations & DataReaderValidations.IsNotClosed) != 0 && isDisposed)
                throw new InvalidOperationException(ExceptionMessage.ReaderClosed);
        }

        /// <summary>
        /// Copy the value of the specified field to an array.
        /// </summary>
        /// <param name="field">The index of the field.</param>
        /// <param name="fieldOffset">The offset in the field value.</param>
        /// <param name="destinationArray">The destination array where the field value will be copied.</param>
        /// <param name="destinationOffset">The destination array offset.</param>
        /// <param name="length">The number of characters to copy from the field value.</param>
        /// <returns></returns>
        long CopyFieldToArray(int field, long fieldOffset, Array destinationArray, int destinationOffset, int length)
        {
            EnsureInitialize();

            if (field < 0 || field >= fieldCount)
                throw new ArgumentOutOfRangeException("field", field, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, field));

            if (fieldOffset < 0 || fieldOffset >= int.MaxValue)
                throw new ArgumentOutOfRangeException("fieldOffset");

            // Array.Copy(...) will do the remaining argument checks

            if (length == 0)
                return 0;

            var value = this[field] ?? string.Empty;

            Debug.Assert(fieldOffset < int.MaxValue);

            Debug.Assert(destinationArray.GetType() == typeof(char[]) || destinationArray.GetType() == typeof(byte[]));

            if (destinationArray.GetType() == typeof(char[]))
                Array.Copy(value.ToCharArray((int)fieldOffset, length), 0, destinationArray, destinationOffset, length);
            else
            {
                var chars = value.ToCharArray((int)fieldOffset, length);
                var source = new byte[chars.Length]; ;

                for (var i = 0; i < chars.Length; i++)
                    source[i] = Convert.ToByte(chars[i]);

                Array.Copy(source, 0, destinationArray, destinationOffset, length);
            }

            return length;
        }

        #endregion

        #region IDataReader Members

        int IDataReader.RecordsAffected => -1;

        bool IDataReader.IsClosed => eof;

        bool IDataReader.NextResult()
        {
            ValidateDataReader(DataReaderValidations.IsNotClosed);

            return false;
        }

        void IDataReader.Close() => Dispose();

        bool IDataReader.Read()
        {
            ValidateDataReader(DataReaderValidations.IsNotClosed);

            return ReadNextRecord();
        }

        int IDataReader.Depth
        {
            get
            {
                ValidateDataReader(DataReaderValidations.IsNotClosed);

                return 0;
            }
        }

        DataTable IDataReader.GetSchemaTable()
        {
            EnsureInitialize();
            ValidateDataReader(DataReaderValidations.IsNotClosed);

            var schema = new DataTable("SchemaTable")
            {
                Locale = CultureInfo.InvariantCulture,
                MinimumCapacity = fieldCount
            };

            schema.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
            schema.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;

            schema.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
            schema.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;

            string[] columnNames;

            if (hasHeaders) columnNames = fieldHeaders;
            else
            {
                columnNames = new string[fieldCount];

                for (var i = 0; i < fieldCount; i++)
                    columnNames[i] = "Column" + i.ToString(CultureInfo.InvariantCulture);
            }

            // null marks columns that will change for each row
            var schemaRow = new object[] {
                    true,					// 00- AllowDBNull
					null,					// 01- BaseColumnName
					string.Empty,			// 02- BaseSchemaName
					string.Empty,			// 03- BaseTableName
					null,					// 04- ColumnName
					null,					// 05- ColumnOrdinal
					int.MaxValue,			// 06- ColumnSize
					typeof(string),			// 07- DataType
					false,					// 08- IsAliased
					false,					// 09- IsExpression
					false,					// 10- IsKey
					false,					// 11- IsLong
					false,					// 12- IsUnique
					DBNull.Value,			// 13- NumericPrecision
					DBNull.Value,			// 14- NumericScale
					(int) DbType.String,	// 15- ProviderType

					string.Empty,			// 16- BaseCatalogName
					string.Empty,			// 17- BaseServerName
					false,					// 18- IsAutoIncrement
					false,					// 19- IsHidden
					true,					// 20- IsReadOnly
					false					// 21- IsRowVersion
			  };

            for (var i = 0; i < columnNames.Length; i++)
            {
                schemaRow[1] = columnNames[i]; // Base column name
                schemaRow[4] = columnNames[i]; // Column name
                schemaRow[5] = i; // Column ordinal

                schema.Rows.Add(schemaRow);
            }

            return schema;
        }

        #endregion

        #region IDataRecord Members

        int IDataRecord.GetInt32(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            var value = this[i];

            return int.Parse(value ?? string.Empty, CultureInfo.CurrentCulture);
        }

        object IDataRecord.this[string name]
        {
            get
            {
                ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
                return this[name];
            }
        }

        object IDataRecord.this[int i]
        {
            get
            {
                ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
                return this[i];
            }
        }

        object IDataRecord.GetValue(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            if (((IDataRecord)this).IsDBNull(i))
                return DBNull.Value;
            else
                return this[i];
        }

        bool IDataRecord.IsDBNull(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return (string.IsNullOrEmpty(this[i]));
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            return CopyFieldToArray(i, fieldOffset, buffer, bufferoffset, length);
        }

        byte IDataRecord.GetByte(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return byte.Parse(this[i], CultureInfo.CurrentCulture);
        }

        Type IDataRecord.GetFieldType(int i)
        {
            EnsureInitialize();
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            if (i < 0 || i >= fieldCount)
                throw new ArgumentOutOfRangeException("i", i, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, i));

            return typeof(string);
        }

        decimal IDataRecord.GetDecimal(int number)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return decimal.Parse(this[number], CultureInfo.CurrentCulture);
        }

        int IDataRecord.GetValues(object[] values)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            var record = (IDataRecord)this;

            for (var i = 0; i < fieldCount; i++)
                values[i] = record.GetValue(i);

            return fieldCount;
        }

        string IDataRecord.GetName(int i)
        {
            EnsureInitialize();
            ValidateDataReader(DataReaderValidations.IsNotClosed);

            if (i < 0 || i >= fieldCount)
                throw new ArgumentOutOfRangeException("i", i, string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldIndexOutOfRange, i));

            if (hasHeaders) return fieldHeaders[i];
            else
                return "Column" + i.ToString(CultureInfo.InvariantCulture);
        }

        long IDataRecord.GetInt64(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return long.Parse(this[i], CultureInfo.CurrentCulture);
        }

        double IDataRecord.GetDouble(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return double.Parse(this[i], CultureInfo.CurrentCulture);
        }

        bool IDataRecord.GetBoolean(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            var value = this[i];


            if (int.TryParse(value, out var result))
                return (result != 0);
            else
                return bool.Parse(value);
        }

        Guid IDataRecord.GetGuid(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return new Guid(this[i]);
        }

        DateTime IDataRecord.GetDateTime(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return DateTime.Parse(this[i], CultureInfo.CurrentCulture);
        }

        int IDataRecord.GetOrdinal(string name)
        {
            EnsureInitialize();
            ValidateDataReader(DataReaderValidations.IsNotClosed);


            if (!fieldHeaderIndexes.TryGetValue(name, out var index))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ExceptionMessage.FieldHeaderNotFound, name), "name");

            return index;
        }

        string IDataRecord.GetDataTypeName(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return typeof(string).FullName;
        }

        float IDataRecord.GetFloat(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return float.Parse(this[i], CultureInfo.CurrentCulture);
        }

        IDataReader IDataRecord.GetData(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            if (i == 0) return this;
            else return null;
        }

        long IDataRecord.GetChars(int field, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);

            return CopyFieldToArray(field, fieldoffset, buffer, bufferoffset, length);
        }

        string IDataRecord.GetString(int field)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return this[field];
        }

        char IDataRecord.GetChar(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return char.Parse(this[i]);
        }

        short IDataRecord.GetInt16(int i)
        {
            ValidateDataReader(DataReaderValidations.IsInitialized | DataReaderValidations.IsNotClosed);
            return short.Parse(this[i], CultureInfo.CurrentCulture);
        }

        #endregion

        #region IEnumerable<string[]> Members

        /// <summary>
        /// Returns an <see cref="T:RecordEnumerator"/>  that can iterate through CSV records.
        /// </summary>
        /// <returns>An <see cref="T:RecordEnumerator"/>  that can iterate through CSV records.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        public CsvDataReader.RecordEnumerator GetEnumerator()
        {
            return new CsvDataReader.RecordEnumerator(this);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.Generics.IEnumerator"/>  that can iterate through CSV records.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.Generics.IEnumerator"/>  that can iterate through CSV records.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        IEnumerator<string[]> IEnumerable<string[]>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IEnumerator"/>  that can iterate through CSV records.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"/>  that can iterate through CSV records.</returns>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        ///	The instance has been disposed of.
        /// </exception>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region IDisposable members

#if DEBUG
        /// <summary>
        /// Contains the stack when the object was allocated.
        /// </summary>
        System.Diagnostics.StackTrace allocStack;
#endif

        /// <summary>
        /// Contains the disposed status flag.
        /// </summary>
        bool isDisposed;

        /// <summary>
        /// Contains the locking object for multi-threading purpose.
        /// </summary>
        readonly object _lock = new object();

        /// <summary>
        /// Occurs when the instance is disposed of.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Gets a value indicating whether the instance has been disposed of.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the instance has been disposed of; otherwise, <see langword="false"/>.
        /// </value>
        [System.ComponentModel.Browsable(false)]
        public bool IsDisposed => isDisposed;

        /// <summary>
        /// Raises the <see cref="M:Disposed"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected virtual void OnDisposed(EventArgs e) => Disposed?.Invoke(this, e);

        /// <summary>
        /// Checks if the instance has been disposed of, and if it has, throws an <see cref="T:System.ComponentModel.ObjectDisposedException"/>; otherwise, does nothing.
        /// </summary>
        /// <exception cref="T:System.ComponentModel.ObjectDisposedException">
        /// 	The instance has been disposed of.
        /// </exception>
        /// <remarks>
        /// 	Derived classes should call this method at the start of all methods and properties that should not be accessed after a call to <see cref="M:Dispose()"/>.
        /// </remarks>
        protected void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        /// <summary>
        /// Releases all resources used by the instance.
        /// </summary>
        /// <remarks>
        /// 	Calls <see cref="M:Dispose(Boolean)"/> with the disposing parameter set to <see langword="true"/> to free unmanaged and managed resources.
        /// </remarks>
        public void Dispose()
        {
            if (!isDisposed)
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by this instance and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// 	<see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Refer to http://www.bluebytesoftware.com/blog/PermaLink,guid,88e62cdf-5919-4ac7-bc33-20c06ae539ae.aspx
            // Refer to http://www.gotdotnet.com/team/libraries/whitepapers/resourcemanagement/resourcemanagement.aspx

            // No exception should ever be thrown except in critical scenarios.
            // Unhandled exceptions during finalization will tear down the process.
            if (!isDisposed)
            {
                try
                {
                    // Dispose-time code should call Dispose() on all owned objects that implement the IDisposable interface.
                    // "owned" means objects whose lifetime is solely controlled by the container.
                    // In cases where ownership is not as straightforward, techniques such as HandleCollector can be used.
                    // Large managed object fields should be nulled out.

                    // Dispose-time code should also set references of all owned objects to null, after disposing them. This will allow the referenced objects to be garbage collected even if not all references to the "parent" are released. It may be a significant memory consumption win if the referenced objects are large, such as big arrays, collections, etc.
                    if (disposing)
                    {
                        // Acquire a lock on the object while disposing.

                        if (reader != null)
                        {
                            lock (_lock)
                            {
                                if (reader != null)
                                {
                                    reader.Dispose();

                                    reader = null;
                                    buffer = null;
                                    eof = true;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    // Ensure that the flag is set
                    isDisposed = true;

                    // Catch any issues about firing an event on an already disposed object.
                    try
                    {
                        OnDisposed(EventArgs.Empty);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the instance is reclaimed by garbage collection.
        /// </summary>
        ~CsvDataReader()
        {
#if DEBUG
            Debug.WriteLine("FinalizableObject was not disposed" + allocStack.ToString());
#endif

            Dispose(disposing: false);
        }

        #endregion
    }
}