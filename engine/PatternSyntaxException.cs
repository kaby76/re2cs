﻿using System;

namespace engine
{
    // Copyright 2010 The Go Authors. All rights reserved.
    // Use of this source code is governed by a BSD-style
    // license that can be found in the LICENSE file.

    /**
     * An exception thrown by the parser if the pattern was invalid.
     *
     * Following {@code java.util.regex.PatternSyntaxException}, this is an unchecked exception.
     */
    public class PatternSyntaxException : Exception
    {

        private string error; // the nature of the error
        private string input; // the partial input at the point of error.

        public PatternSyntaxException(string error, string input)
        {
            this.error = "error parsing regexp: " + error + ": `" + input + "`";
            this.input = input;
        }

        public PatternSyntaxException(string error)
        {
            this.error = "error parsing regexp: " + error;
            this.input = "";
        }

        /**
         * Retrieves the error index.
         *
         * @return The approximate index in the pattern of the error, or <tt>-1</tt> if the index is not
         * known
         */
        public int getIndex()
        {
            return -1;
        }

        /**
         * Retrieves the description of the error.
         *
         * @return The description of the error
         */
        public string getDescription()
        {
            return error;
        }

        /**
         * Retrieves the erroneous regular-expression pattern.
         *
         * @return The erroneous pattern
         */
        public string getPattern()
        {
            return input;
        }
    }
}
