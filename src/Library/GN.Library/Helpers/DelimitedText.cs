using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Helpers
{
    public enum DelimitedTextCaseOption
    {
        LowerCase,
        UpperCase,
        CaseSensitive
    }

    public class DelimitedText
    {
        private const char DELIM = '\x06';
        private string rawText;
        private string[] parts;
        private char[] delimiters;
        private bool trimmed;
        private bool allowDuplcate;
        private char[] invalidChars;
        private DelimitedTextCaseOption caseOptions;
        private char? invalidCharReplacement;
        public bool AllowDuplicates
        {
            get { return this.allowDuplcate; }
            set { this.allowDuplcate = value; GetParts(true); }
        }
        public bool Trimmed
        {
            get { return this.trimmed; }
            set { this.trimmed = value; GetParts(true); }
        }
        public char[] InvalidChars
        {
            get { return this.invalidChars; }
            set { this.invalidChars = value; GetParts(true); }
        }
        public DelimitedTextCaseOption CaseOption
        {
            get { return this.caseOptions; }
            set { this.caseOptions = value; GetParts(true); }
        }
        public string RawText
        {
            get { return this.rawText.Replace(DELIM, this.Delimeter); }
            set { this.rawText = FixDelims(value, this.delimiters, DELIM); GetParts(true); }
        }
        public char? InvalidCharReplacement
        {
            get { return this.invalidCharReplacement; }
            set { this.invalidCharReplacement = value; GetParts(true); }
        }
        private string[] GetParts(bool refresh = false)
        {
            if (this.parts == null || refresh)
            {
                var parts = new List<string>();
                foreach (var item in this.rawText?.Split(new char[] { DELIM }, options: StringSplitOptions.RemoveEmptyEntries))
                {
                    var fixedItem = Fix(item);
                    if (!string.IsNullOrWhiteSpace(fixedItem) && (this.AllowDuplicates || !parts.Contains(fixedItem)))
                        parts.Add(fixedItem);
                }
                this.parts = parts.ToArray();
            }
            return this.parts;
        }
        private string FixDelims(string input, char[] delims, char delim)
        {
            string result = input;
            if (!string.IsNullOrWhiteSpace(result))
            {
                foreach (var ch in delims)
                {
                    result = result.Replace(ch, delim);
                }
            }
            return result;
        }
        private string Fix(string str)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(str))
            {
                switch (this.CaseOption)
                {
                    case DelimitedTextCaseOption.LowerCase:
                        result = str?.ToLowerInvariant();
                        break;
                    case DelimitedTextCaseOption.UpperCase:
                        result = str?.ToUpperInvariant();
                        break;
                }
                if (this.InvalidChars != null)
                {
                    foreach (var c in this.InvalidChars)
                        result = result.Replace(new string(c, 1),
                            (this.invalidCharReplacement == null ? "" : new string(this.invalidCharReplacement.Value, 1)));
                }
                if (this.Trimmed)
                    result = result.Trim();
            }
            return result;
        }


        public char Delimeter
        {
            get { return this.delimiters[0]; }
            set
            {
                if (this.delimiters == null || this.delimiters.Length == 0)
                    this.delimiters = new char[] { ',' };
                this.delimiters[0] = value;
            }
        }

        public DelimitedText(string text) : this(text, null) { }
        public DelimitedText(string text, char[] delimiters, DelimitedTextCaseOption caseOptions = DelimitedTextCaseOption.LowerCase,
            bool allowDuplicates = false, bool trimmed = true, char[] invalidChars = null, char? invalidCharReplacement = null)
        {
			this.rawText = text ?? "";
            this.delimiters = delimiters == null || delimiters.Length == 0
                ? new char[] { ',' }
                : delimiters;
			rawText = FixDelims(text, this.delimiters, DELIM) ?? "";
            this.allowDuplcate = allowDuplicates;
            this.trimmed = trimmed;
            this.invalidChars = invalidChars;
            this.caseOptions = caseOptions;
            this.invalidCharReplacement = invalidCharReplacement;
            GetParts(true);
        }
        public void SetText(string text)
        {
			rawText = FixDelims(text, this.delimiters, DELIM) ?? "";
            GetParts(true);
        }
        public DelimitedText Clone(string text = null, char[] delimiters = null,
            DelimitedTextCaseOption? caseOption = null,
            bool? allowDuplicate = null,
            bool? trimmed = null,
            char[] invalidChars = null, char? invalidCharReplacment = null)
        {
            return new DelimitedText(text ?? this.rawText, delimiters ?? this.delimiters,
                caseOption ?? this.caseOptions, allowDuplicate ?? this.allowDuplcate, trimmed ?? this.trimmed,
                invalidChars ?? this.invalidChars, invalidCharReplacement ?? this.invalidCharReplacement);
        }


        public IEnumerable<string> Parts { get { return this.GetParts(); } }
        public string ToString(char delim)
        {
            var result = "";
            var isfirst = true;
            foreach (var part in this.parts)
            {
                result = isfirst ? part :
                     (result + delim + part);
                isfirst = false;
            }
            return result;
        }
        public override string ToString()
        {
            return ToString(Delimeter);
        }

        private bool contains(string str)
        {
            var ignore = this.CaseOption != DelimitedTextCaseOption.CaseSensitive;
            var fixedStr = Fix(str);
            return !string.IsNullOrWhiteSpace(str) && this.parts.Any(x => string.Compare(x, fixedStr, ignore) == 0);
        }
        private void addText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                rawText = rawText + DELIM + FixDelims(text, this.delimiters, DELIM);
                GetParts(true);
            }
        }
        public DelimitedText Intersect(string[] other)
        {
            DelimitedText result = this.Clone("");
            if (other != null && other.Length > 0)
            {
                foreach (var str in other)
                {
                    if (this.contains(str))
                        result.addText(str);
                }
            }
            return result;
        }
        public int Length { get { return this.parts == null ? 0 : this.parts.Length; } }
        public DelimitedText Intersect(DelimitedText other)
        {
            return this.Intersect(other == null ? null : other.parts);
        }
        public DelimitedText Intersect(string other)
        {
            return this.Intersect(this.Clone(other));
        }

        public bool Contains(string text)
        {
            return Contains(this.Clone(text));
        }
        public bool Contains(DelimitedText other)
        {
            return Intersect(other).Length == other.Length;
        }
        public void Add(string[] items)
        {
            items = items ?? new string[] { };
            foreach (var item in items ?? new string[] { })
                this.addText(item);
        }
        public void Add(DelimitedText other, bool allowDuplicate = false, bool? caseSensitive = null)
        {
            if (other != null && other.Length > 0)
            {
                Add(other.Parts.ToArray());
            }
        }
        public void Add(string str)
        {
            this.addText(str);
        }
        public bool Equals(DelimitedText other)
        {
            return this.Length == other.Length && this.Contains(other);
        }

        public bool IsComaptibleWith(DelimitedText other)
        {
            return this.allowDuplcate == other.AllowDuplicates && this.caseOptions == other.caseOptions
                && this.trimmed == other.trimmed;
        }
        public int CompareSequence(DelimitedText other)
        {
            if (other == null)
                return 0;
            int i;
            bool ignoreCase = this.caseOptions != DelimitedTextCaseOption.CaseSensitive;
            for (i = 0; i < this.Length && i < other.Length; i++)
            {
                if (string.Compare(this.parts[i], Fix(other.parts[i]), ignoreCase) != 0)
                    return i;
            }
            return i - 1;
        }
        public void Clear()
        {
            this.RawText = "";
        }
        public DelimitedText Substring(int start, int? len)
        {
            var result = this.Clone("");
            int i = start < 0 ? 0 : start;
            int last = i + len ?? this.Length;
            for (i = start; i < this.Length && i < last; i++)
                result.Add(this.parts[i]);
            return result;
        }
		public static implicit operator DelimitedText(string value)
		{
			return new DelimitedText(value);
		}
	}
}
