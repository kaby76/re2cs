using System;
using System.Text.RegularExpressions;
using engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject2
{
    public class ApiTestUtils
    {

        /**
         * Asserts that IllegalArgumentException is thrown from compile with flags.
         */
        public static void assertCompileFails(String regex, int flag)
        {
            try
            {
                Pattern.compile(regex, flag);
                Assert.Fail(
                    "Compiling Pattern with regex: "
                    + regex
                    + " and flag: "
                    + flag
                    + " passed, when it should have failed.");
            }
            catch (Exception e)
            {
                if ("Flags UNIX_LINES and COMMENTS unsupported" != e.Message)
                {
                    throw e;
                }
            }
        }

        /**
         * Asserts all strings in array equal.
         */
        public static void assertArrayEquals(Object[] expected, Object[] actual)
        {
            Assert.AreEqual(
                expected.Length,
                actual.Length);
            for (int idx = 0; idx < expected.Length; ++idx)
            {
                Assert.AreEqual(expected[idx], actual[idx]);
            }
        }

        /**
         * Tests that both RE2's and JDK's pattern class act as we expect them. The regular expression
         * {@code regexp} matches the string {@code match} and doesn't match {@code nonMatch}
         *
         * @param regexp
         * @param match
         * @param nonMatch
         */
        public static void testMatches(string regexp, string match, String nonMatch)
        {
            string errorString = "Pattern with regexp: " + regexp;
            Assert.IsTrue(
                Regex.IsMatch(match, regexp),
                "Regex " + errorString + " doesn't match: " + match);
            Assert.IsFalse(
                Regex.IsMatch(nonMatch, regexp),
                "Regex " + errorString + " matches: " + nonMatch);
            Assert.IsTrue(
                Pattern.matches(regexp, match),
                errorString + " doesn't match: " + match);
            Assert.IsFalse(
                Pattern.matches(regexp, nonMatch),
                errorString + " matches: " + nonMatch);
        }

        // Test matches via a matcher.
        public static void testMatcherMatches(String regexp, String match, String nonMatch)
        {
            testMatcherMatches(regexp, match);
            testMatcherNotMatches(regexp, nonMatch);
        }

        public static void testMatcherMatches(String regexp, String match)
        {
            Regex p = new Regex(regexp);
            Assert.IsTrue(
                p.IsMatch(match),
                "Regexp: " + regexp + " doesn't match: " + match);
            Pattern pr = Pattern.compile(regexp);
            Assert.IsTrue(
                pr.matcher(match).matches(),
                "Pattern with regexp: " + regexp + " doesn't match: " + match);
        }

        public static void testMatcherNotMatches(String regexp, String nonMatch)
        {
            Regex p = new Regex(regexp);
            Assert.IsFalse(
                p.IsMatch(nonMatch),
                "JDK Pattern with regexp: " + regexp + " matches: " + nonMatch);
            Pattern pr = Pattern.compile(regexp);
            Assert.IsFalse(
                pr.matcher(nonMatch).matches(),
                "Pattern with regexp: " + regexp + " matches: " + nonMatch);
        }

        /**
         * This takes a regex and it's compile time flags, a string that is expected to match the regex
         * and a string that is not expected to match the regex.
         *
         * We don't check for JDK compatibility here, since the flags are not in a 1-1 correspondence.
         *
         */
        public static void testMatchesRE2(String regexp, int flags, String match, String nonMatch)
        {
            Pattern p = Pattern.compile(regexp, flags);
            String errorString = "Pattern with regexp: " + regexp + " and flags: " + flags;
            Assert.IsTrue(
                p.matches(match),
                errorString + " doesn't match: " + match);
            Assert.IsFalse(
                p.matches(nonMatch),
                errorString + " matches: " + nonMatch);
        }

        public static void testMatchesRE2(
            String regexp, int flags, String[] matches, String[] nonMatches)
        {
            Pattern p = Pattern.compile(regexp, flags);
            foreach (string s in matches)
            {
                Assert.IsTrue(p.matches(s));
            }

            foreach (string s in nonMatches)
            {
                Assert.IsFalse(p.matches(s));
            }
        }

        /**
         * Tests that both RE2 and JDK split the string on the regex in the same way, and that that way
         * matches our expectations.
         */
        public static void testSplit(String regexp, String text, String[] expected)
        {
            testSplit(regexp, text, 0, expected);
        }

        public static void testSplit(String regexp, String text, int limit, String[] expected)
        {
            //          assertArrayEquals(expected, java.util.regex.Pattern.compile(regexp).split(text, limit));
            assertArrayEquals(expected, Pattern.compile(regexp).split(text, limit));
        }

        // Helper methods for RE2Matcher's test.

        // Tests that both RE2 and JDK's Matchers do the same replaceFist.
        public static void testReplaceAll(String orig, String regex, String repl, String actual)
        {
            Pattern p = Pattern.compile(regex);
            Matcher m = p.matcher(orig);
            String replaced = m.replaceAll(repl);
            Assert.AreEqual(actual, replaced);

            // Regex
            Regex pj = new Regex(regex);
            replaced = pj.Replace(orig, repl);
            Assert.AreEqual(actual, replaced);
        }

        // Tests that both RE2 and JDK's Matchers do the same replaceFist.
        public static void testReplaceFirst(String orig, String regex, String repl, String actual)
        {
            Pattern p = Pattern.compile(regex);
            Matcher m = p.matcher(orig);
            String replaced = m.replaceFirst(repl);
            Assert.AreEqual(actual, replaced);

            // Regex
            Regex pj = new Regex(regex);
            replaced = pj.Replace(orig, repl, 1);
            Assert.AreEqual(actual, replaced);
        }

        // Tests that both RE2 and JDK's Patterns/Matchers give the same groupCount.
        public static void testGroupCount(String pattern, int count)
        {
            // RE2
            Pattern p = Pattern.compile(pattern);
            Matcher m = p.matcher("x");
            Assert.AreEqual(count, p.groupCount());
            Assert.AreEqual(count, m.groupCount());

            // Regex
            //Regex pj = new Regex(pattern);
            //var matches = pj.Matches("x");
            //Assert.AreEqual(count, matches.Count);
        }

        public static void testGroup(String text, String regexp, String[] output)
        {
            // RE2
            Pattern p = Pattern.compile(regexp);
            Matcher matchString = p.matcher(text);
            Assert.AreEqual(true, matchString.find());
            Assert.AreEqual(output[0], matchString.group());
            for (int i = 0; i < output.Length; i++)
            {
                Assert.AreEqual(output[i], matchString.group(i));
            }

            Assert.AreEqual(output.Length - 1, matchString.groupCount());

            //// JDK
            //java.util.regex.Pattern pj = java.util.regex.Pattern.compile(regexp);
            //java.util.regex.Matcher matchStringj = pj.matcher(text);
            //// java.util.regex.Matcher matchBytes =
            ////   p.matcher(text.getBytes(Charsets.UTF_8));
            //Assert.AreEqual(true, matchStringj.find());
            //// assertEquals(true, matchBytes.find());
            //Assert.AreEqual(output[0], matchStringj.group());
            //// assertEquals(output[0], matchBytes.group());
            //for (int i = 0; i < output.Length; i++)
            //{
            //    Assert.AreEqual(output[i], matchStringj.group(i));
            //    // assertEquals(output[i], matchBytes.group(i));
            //}
        }

        public static void testFind(String text, String regexp, int start, String output)
        {
            // RE2
            Pattern p = Pattern.compile(regexp);
            Matcher matchString = p.matcher(text);
            // RE2Matcher matchBytes = p.matcher(text.getBytes(Charsets.UTF_8));
            Assert.IsTrue(matchString.find(start));
            // assertTrue(matchBytes.find(start));
            Assert.AreEqual(output, matchString.group());
            // assertEquals(output, matchBytes.group());

            //// JDK
            //java.util.regex.Pattern pj = java.util.regex.Pattern.compile(regexp);
            //java.util.regex.Matcher matchStringj = pj.matcher(text);
            //Assert.IsTrue(matchStringj.find(start));
            //Assert.AreEqual(output, matchStringj.group());
        }

        public static void testFindNoMatch(String text, String regexp, int start)
        {
            // RE2
            Pattern p = Pattern.compile(regexp);
            Matcher matchString = p.matcher(text);
            // RE2Matcher matchBytes = p.matcher(text.getBytes(Charsets.UTF_8));
            Assert.IsFalse(matchString.find(start));
            // assertFalse(matchBytes.find(start));

            // JDK
            //java.util.regex.Pattern pj = java.util.regex.Pattern.compile(regexp);
            //java.util.regex.Matcher matchStringj = pj.matcher(text);
            //Assert.IsFalse(matchStringj.find(start));
        }

        public static void testInvalidGroup(String text, String regexp, int group)
        {
            Pattern p = Pattern.compile(regexp);
            Matcher m = p.matcher(text);
            m.find();
            m.group(group);
            Assert.Fail(); // supposed to have exception by now
        }

        public static void verifyLookingAt(String text, String regexp, bool output)
        {
            Assert.AreEqual(output, Pattern.compile(regexp).matcher(text).lookingAt());
            //    assertEquals(output, java.util.regex.Pattern.compile(regexp).matcher(text).lookingAt());
        }
    }
}
