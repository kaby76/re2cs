using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using engine;

namespace UnitTestProject3
{
    [TestClass]
    public class FindTest
    {

        // For each pattern/text pair, what is the expected output of each
        // function?  We can derive the textual results from the indexed
        // results, the non-submatch results from the submatched results, the
        // single results from the 'all' results, and the String results from
        // the UTF-8 results. Therefore the table includes only the
        // findAllUTF8SubmatchIndex result.

        public class TestTest
        {
            // The n and x parameters construct a [][]int by extracting n
            // sequences from x.  This represents n matches with len(x)/n
            // submatches each.
            public TestTest(String pat, String text, int n, params int[] x)
            {
                this.pat = pat;
                this.text = text;
                this.textUTF8 = GoTestUtils.utf8(text);
                this.matches = new int[n][];
                if (n > 0)
                {
                    int runLength = x.Length / n;
                    for (int j = 0, i = 0; i < n; i++)
                    {
                        matches[i] = new int[runLength];
                        System.Array.Copy(x, j, matches[i], 0, runLength);
                        j += runLength;
                        if (j > x.Length)
                        {
                            Assert.Fail("invalid build entry");
                        }
                    }
                }
            }

            public string pat;
            public string text;

            public byte[] textUTF8;

            // Each element is an even-length array of indices into textUTF8.  Not null.
            public int[][] matches;

            public byte[] submatchBytes(int i, int j)
            {
                return Utils.subarray(textUTF8, matches[i][2 * j], matches[i][2 * j + 1]);
            }

            public String submatchString(int i, int j)
            {
                return GoTestUtils.fromUTF8(submatchBytes(i, j)); // yikes
            }

            public override string ToString()
            {
                return String.Format("pat={0} text={1}", pat, text);
            }
        }

        // Used by RE2Test also.
        static TestTest[] FIND_TESTS =
        {
            new TestTest("", "", 1, 0, 0),
            new TestTest("^abcdefg", "abcdefg", 1, 0, 7),
            new TestTest("a+", "baaab", 1, 1, 4),
            new TestTest("abcd..", "abcdef", 1, 0, 6),
            new TestTest("a", "a", 1, 0, 1),
            new TestTest("x", "y", 0),
            new TestTest("b", "abc", 1, 1, 2),
            new TestTest(".", "a", 1, 0, 1),
            new TestTest(".*", "abcdef", 1, 0, 6),
            new TestTest("^", "abcde", 1, 0, 0),
            new TestTest("$", "abcde", 1, 5, 5),
            new TestTest("^abcd$", "abcd", 1, 0, 4),
            new TestTest("^bcd'", "abcdef", 0),
            new TestTest("^abcd$", "abcde", 0),
            new TestTest("a+", "baaab", 1, 1, 4),
            new TestTest("a*", "baaab", 3, 0, 0, 1, 4, 5, 5),
            new TestTest("[a-z]+", "abcd", 1, 0, 4),
            new TestTest("[^a-z]+", "ab1234cd", 1, 2, 6),
            new TestTest("[a\\-\\]z]+", "az]-bcz", 2, 0, 4, 6, 7),
            new TestTest("[^\\n]+", "abcd\n", 1, 0, 4),
            new TestTest("[日本語]+", "日本語日本語", 1, 0, 18),
            new TestTest("日本語+", "日本語", 1, 0, 9),
            new TestTest("日本語+", "日本語語語語", 1, 0, 18),
            new TestTest("()", "", 1, 0, 0, 0, 0),
            new TestTest("(a)", "a", 1, 0, 1, 0, 1),
            new TestTest("(.)(.)", "日a", 1, 0, 4, 0, 3, 3, 4),
            new TestTest("(.*)", "", 1, 0, 0, 0, 0),
            new TestTest("(.*)", "abcd", 1, 0, 4, 0, 4),
            new TestTest("(..)(..)", "abcd", 1, 0, 4, 0, 2, 2, 4),
            new TestTest("(([^xyz]*)(d))", "abcd", 1, 0, 4, 0, 4, 0, 3, 3, 4),
            new TestTest("((a|b|c)*(d))", "abcd", 1, 0, 4, 0, 4, 2, 3, 3, 4),
            new TestTest("(((a|b|c)*)(d))", "abcd", 1, 0, 4, 0, 4, 0, 3, 2, 3, 3, 4),
            new TestTest("\\a\\f\\n\\r\\t\\v", "\007\f\n\r\t\013", 1, 0, 6),
            new TestTest("[\\a\\f\\n\\r\\t\\v]+", "\007\f\n\r\t\013", 1, 0, 6),
            new TestTest("a*(|(b))c*", "aacc", 1, 0, 4, 2, 2, -1, -1),
            new TestTest("(.*).*", "ab", 1, 0, 2, 0, 2),
            new TestTest("[.]", ".", 1, 0, 1),
            new TestTest("/$", "/abc/", 1, 4, 5),
            new TestTest("/$", "/abc", 0),

            // multiple matches
            new TestTest(".", "abc", 3, 0, 1, 1, 2, 2, 3),
            new TestTest("(.)", "abc", 3, 0, 1, 0, 1, 1, 2, 1, 2, 2, 3, 2, 3),
            new TestTest(".(.)", "abcd", 2, 0, 2, 1, 2, 2, 4, 3, 4),
            new TestTest("ab*", "abbaab", 3, 0, 3, 3, 4, 4, 6),
            new TestTest("a(b*)", "abbaab", 3, 0, 3, 1, 3, 3, 4, 4, 4, 4, 6, 5, 6),

            // fixed bugs
            new TestTest("ab$", "cab", 1, 1, 3),
            new TestTest("axxb$", "axxcb", 0),
            new TestTest("data", "daXY data", 1, 5, 9),
            new TestTest("da(.)a$", "daXY data", 1, 5, 9, 7, 8),
            new TestTest("zx+", "zzx", 1, 1, 3),
            new TestTest("ab$", "abcab", 1, 3, 5),
            new TestTest("(aa)*$", "a", 1, 1, 1, -1, -1),
            new TestTest("(?:.|(?:.a))", "", 0),
            new TestTest("(?:A(?:A|a))", "Aa", 1, 0, 2),
            new TestTest("(?:A|(?:A|a))", "a", 1, 0, 1),
            new TestTest("(a){0}", "", 1, 0, 0, -1, -1),
            new TestTest("(?-s)(?:(?:^).)", "\n", 0),
            new TestTest("(?s)(?:(?:^).)", "\n", 1, 0, 1),
            new TestTest("(?:(?:^).)", "\n", 0),
            new TestTest("\\b", "x", 2, 0, 0, 1, 1),
            new TestTest("\\b", "xx", 2, 0, 0, 2, 2),
            new TestTest("\\b", "x y", 4, 0, 0, 1, 1, 2, 2, 3, 3),
            new TestTest("\\b", "xx yy", 4, 0, 0, 2, 2, 3, 3, 5, 5),
            new TestTest("\\B", "x", 0),
            new TestTest("\\B", "xx", 1, 1, 1),
            new TestTest("\\B", "x y", 0),
            new TestTest("\\B", "xx yy", 2, 1, 1, 4, 4),

            // RE2 tests
            new TestTest("[^\\S\\s]", "abcd", 0),
            new TestTest("[^\\S[:space:]]", "abcd", 0),
            new TestTest("[^\\D\\d]", "abcd", 0),
            new TestTest("[^\\D[:digit:]]", "abcd", 0),
            new TestTest("(?i)\\W", "x", 0),
            new TestTest("(?i)\\W", "k", 0),
            new TestTest("(?i)\\W", "s", 0),

            // can backslash-escape any punctuation
            new TestTest(
                "\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\{\\|\\}\\~",
                "!\"#$%&'()*+,-./:;<=>?@[\\]^_{|}~",
                1,
                0,
                31),
            new TestTest(
                "[\\!\\\"\\#\\$\\%\\&\\'\\(\\)\\*\\+\\,\\-\\.\\/\\:\\;\\<\\=\\>\\?\\@\\[\\\\\\]\\^\\_\\{\\|\\}\\~]+",
                "!\"#$%&'()*+,-./:;<=>?@[\\]^_{|}~",
                1,
                0,
                31),
            new TestTest("\\`", "`", 1, 0, 1),
            new TestTest("[\\`]+", "`", 1, 0, 1),

            // long set of matches
            new TestTest(
                ".",
                "qwertyuiopasdfghjklzxcvbnm1234567890",
                36,
                0,
                1,
                1,
                2,
                2,
                3,
                3,
                4,
                4,
                5,
                5,
                6,
                6,
                7,
                7,
                8,
                8,
                9,
                9,
                10,
                10,
                11,
                11,
                12,
                12,
                13,
                13,
                14,
                14,
                15,
                15,
                16,
                16,
                17,
                17,
                18,
                18,
                19,
                19,
                20,
                20,
                21,
                21,
                22,
                22,
                23,
                23,
                24,
                24,
                25,
                25,
                26,
                26,
                27,
                27,
                28,
                28,
                29,
                29,
                30,
                30,
                31,
                31,
                32,
                32,
                33,
                33,
                34,
                34,
                35,
                35,
                36),
        };

        private TestTest test;

        public FindTest() { }

        [TestMethod]
        public void DoFrigginTests()
        {
            foreach (var p in FIND_TESTS)
            {
                test = p;
                testFindUTF8();
                testFind();
                testFindUTF8Index();
                testFindIndex();
                testFindAllUTF8();
                testFindAll();
                testFindAllUTF8Index();
                testFindUTF8Submatch();
                testFindSubmatch();
                testFindUTF8SubmatchIndex();
                testFindSubmatchIndex();
                testFindAllUTF8Submatch();
                testFindAllSubmatch();
                testFindAllUTF8SubmatchIndex();
                testFindAllSubmatchIndex();
            }
        }

        // First the simple cases.

        public void testFindUTF8()
        {
            RE2 re = RE2.compile(test.pat);
            if (!re.ToString().Equals(test.pat))
            {
                Assert.Fail(String.Format("RE2.toString() = \"{0}\"; should be \"{1}\"", re.ToString(), test.pat));
            }

            byte[] result = re.findUTF8(test.textUTF8);
            if (test.matches.Length == 0 && GoTestUtils.len(result) == 0)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("findUTF8: expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("findUTF8: expected match; got none: {0}", test));
            }
            else
            {
                byte[] expect = test.submatchBytes(0, 0);
                if (!expect.SequenceEqual(result))
                {
                    Assert.Fail(
                        String.Format(
                            "findUTF8: expected {0}; got {1}: {2}",
                            GoTestUtils.fromUTF8(expect),
                            GoTestUtils.fromUTF8(result),
                            test));
                }
            }
        }

        public void testFind()
        {
            String result = RE2.compile(test.pat).find(test.text);
            if (test.matches.Length == 0 && result.Length == 0)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result.Length != 0)
            {
                Assert.Fail(String.Format("find: expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result.Length == 0)
            {
                // Tricky because an empty result has two meanings:
                // no match or empty match.
                int[] match = test.matches[0];
                if (match[0] != match[1])
                {
                    Assert.Fail(String.Format("find: expected match; got none: %s", test));
                }
            }
            else
            {
                String expect = test.submatchString(0, 0);
                if (!expect.Equals(result))
                {
                    Assert.Fail(String.Format("find: expected {0} got {1}: {2}", expect, result, test));
                }
            }
        }

        private void testFindIndexCommon(
            String testName, TestTest test, int[] result, bool resultIndicesAreUTF8)
        {
            if (test.matches.Length == 0 && GoTestUtils.len(result) == 0)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("{0}: expected no match; got one: {1}", testName, test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("{0}: expected match; got none: {1}", testName, test));
            }
            else
            {
                if (!resultIndicesAreUTF8)
                {
                    result = GoTestUtils.utf16IndicesToUtf8(result, test.text);
                }

                int[] expect = test.matches[0]; // UTF-8 indices
                if (expect[0] != result[0] || expect[1] != result[1])
                {
                    Assert.Fail(
                        String.Format(
                            "{0}: expected {1} got {2}: {3}",
                            testName,
                            expect.ToString(),
                            result.ToString(),
                            test));
                }
            }
        }

        public void testFindUTF8Index()
        {
            testFindIndexCommon(
                "testFindUTF8Index", test, RE2.compile(test.pat).findUTF8Index(test.textUTF8), true);
        }

        public void testFindIndex()
        {
            int[] result = RE2.compile(test.pat).findIndex(test.text);
            testFindIndexCommon("testFindIndex", test, result, false);
        }

        // Now come the simple All cases.

        public void testFindAllUTF8()
        {
            List<byte[]> result = RE2.compile(test.pat).findAllUTF8(test.textUTF8, -1);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("findAllUTF8: expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                throw new Exception("findAllUTF8: expected match; got none: " + test);
            }
            else
            {
                if (test.matches.Length != result.Count)
                {
                    Assert.Fail(
                        String.Format(
                            "findAllUTF8: expected {0} matches; got {1}: {2}",
                            test.matches.Length,
                            result.Count),
                            test);
                }

                for (int i = 0; i < test.matches.Length; i++)
                {
                    byte[] expect = test.submatchBytes(i, 0);
                    if (!expect.SequenceEqual(result[i]))
                    {
                        Assert.Fail(
                            String.Format(
                                "findAllUTF8: match {0}: expected {1}; got {2}: {3}",
                                i / 2,
                                GoTestUtils.fromUTF8(expect),
                                GoTestUtils.fromUTF8(result[i]),
                                test));
                    }
                }
            }
        }

        public void testFindAll()
        {
            List<String> result = RE2.compile(test.pat).findAll(test.text, -1);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("findAll: expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("findAll: expected match; got none: {0}", test));
            }
            else
            {
                if (test.matches.Length != result.Count)
                {
                    Assert.Fail(
                        String.Format(
                            "findAll: expected {0} matches; got {1}: {2}",
                            test.matches.Length,
                            result.Count,
                            test));
                }

                for (int i = 0; i < test.matches.Length; i++)
                {
                    String expect = test.submatchString(i, 0);
                    if (!expect.Equals(result[i]))
                    {
                        Assert.Fail(String.Format("findAll: expected {0}; got {1}: {2}", expect, result, test));
                    }
                }
            }
        }

        private void testFindAllIndexCommon(
            String testName, TestTest test, List<int[]> result, bool resultIndicesAreUTF8)
        {
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("{0}: expected no match; got one: {1}", testName, test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("{0}: expected match; got none: {1}", testName, test));
            }
            else
            {
                if (test.matches.Length != result.Count)
                {
                    Assert.Fail(
                        String.Format(
                            "{0}: expected {1} matches; got {2}: {3}",
                            testName,
                            test.matches.Length,
                            result.Count,
                            test));
                }

                for (int k = 0; k < test.matches.Length; k++)
                {
                    int[] e = test.matches[k];
                    int[] res = result[k];
                    if (!resultIndicesAreUTF8)
                    {
                        res = GoTestUtils.utf16IndicesToUtf8(res, test.text);
                    }

                    if (e[0] != res[0] || e[1] != res[1])
                    {
                        Assert.Fail(
                            String.Format(
                                "{0}: match {2}: expected {2}; got {3}: {4}",
                                testName,
                                k,
                                e.ToString(), // (only 1st two elements matter here)
                                res.ToString(),
                                test));
                    }
                }
            }
        }

        public void testFindAllUTF8Index()
        {
            testFindAllIndexCommon(
                "testFindAllUTF8Index",
                test,
                RE2.compile(test.pat).findAllUTF8Index(test.textUTF8, -1),
                true);
        }

        public void testFindAllIndex()
        {
            testFindAllIndexCommon(
                "testFindAllIndex", test, RE2.compile(test.pat).findAllIndex(test.text, -1), false);
        }

        // Now come the Submatch cases.

        private void testSubmatchBytes(String testName, FindTest.TestTest test, int n, byte[][] result)
        {
            int[] submatches = test.matches[n];
            if (submatches.Length != GoTestUtils.len(result) * 2)
            {
                Assert.Fail(
                    String.Format(
                        "{0} {1}: expected {2} submatches; got {3}: {4}",
                        testName,
                        n,
                        submatches.Length / 2,
                        GoTestUtils.len(result),
                        test));
            }

            for (int k = 0; k < GoTestUtils.len(result); k++)
            {
                if (submatches[k * 2] == -1)
                {
                    if (result[k] != null)
                    {
                        Assert.Fail(String.Format("{0} {1}: expected null got {2}: {3}", testName, n, result, test));
                    }

                    continue;
                }

                byte[] expect = test.submatchBytes(n, k);
                if (!expect.SequenceEqual(result[k]))
                {
                    Assert.Fail(
                        String.Format(
                            "{0} {1}: expected {2}; got {3}: {4}",
                            testName,
                            n,
                            GoTestUtils.fromUTF8(expect),
                            GoTestUtils.fromUTF8(result[k]),
                            test));
                }
            }
        }

        public void testFindUTF8Submatch()
        {
            byte[][] result = RE2.compile(test.pat).findUTF8Submatch(test.textUTF8);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("expected match; got none: {0}", test));
            }
            else
            {
                testSubmatchBytes("testFindUTF8Submatch", test, 0, result);
            }
        }

        // (Go: testSubmatchString)
        private void testSubmatch(String testName, TestTest test, int n, String[] result)
        {
            int[] submatches = test.matches[n];
            if (submatches.Length != GoTestUtils.len(result) * 2)
            {
                Assert.Fail(
                    String.Format(
                        "{0} {1}: expected {2} submatches; got {3}: {4}",
                        testName,
                        n,
                        submatches.Length / 2,
                        GoTestUtils.len(result),
                        test));
            }

            for (int k = 0; k < submatches.Length; k += 2)
            {
                if (submatches[k] == -1)
                {
                    if (result[k / 2] != null && result[k / 2].Length != 0)
                    {
                        Assert.Fail(
                            String.Format(
                                "{0} {1}: expected null got {2}: {3}", testName, n, result.ToString(), test));
                    }

                    continue;
                }

                System.Console.WriteLine(testName + "  " + test + " " + n + " " + k + " ");
                String expect = test.submatchString(n, k / 2);
                if (!expect.Equals(result[k / 2]))
                {
                    Assert.Fail(String.Format("{0} {1}: expected {2} got {3}: {4}", testName, n, expect, result, test));
                }
            }
        }

        // (Go: TestFindStringSubmatch)
        public void testFindSubmatch()
        {
            String[] result = RE2.compile(test.pat).findSubmatch(test.text);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("expected match; got none: {0}", test));
            }
            else
            {
                testSubmatch("testFindSubmatch", test, 0, result);
            }
        }

        private void testSubmatchIndices(
            String testName, TestTest test, int n, int[] result, bool resultIndicesAreUTF8)
        {
            int[] expect = test.matches[n];
            if (expect.Length != GoTestUtils.len(result))
            {
                Assert.Fail(
                    String.Format(
                        "{0} {1}: expected {2} matches; got {3}: {4}",
                        testName,
                        n,
                        expect.Length / 2,
                        GoTestUtils.len(result) / 2,
                        test));
                return;
            }

            if (!resultIndicesAreUTF8)
            {
                result = GoTestUtils.utf16IndicesToUtf8(result, test.text);
            }

            for (int k = 0; k < expect.Length; ++k)
            {
                if (expect[k] != result[k])
                {
                    Assert.Fail(
                        String.Format(
                            "{0} {1}: submatch error: expected {2} got {3}: {4}",
                            testName,
                            n,
                            expect.ToString(),
                            result.ToString(),
                            test));
                }
            }
        }

        private void testFindSubmatchIndexCommon(
            String testName, TestTest test, int[] result, bool resultIndicesAreUTF8)
        {
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("{0}: expected no match; got one: {1}", testName, test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("{0}: expected match; got none: {1}", testName, test));
            }
            else
            {
                testSubmatchIndices(testName, test, 0, result, resultIndicesAreUTF8);
            }
        }

        public void testFindUTF8SubmatchIndex()
        {
            testFindSubmatchIndexCommon(
                "testFindSubmatchIndex",
                test,
                RE2.compile(test.pat).findUTF8SubmatchIndex(test.textUTF8),
                true);
        }

        // (Go: TestFindStringSubmatchIndex)
        public void testFindSubmatchIndex()
        {
            testFindSubmatchIndexCommon(
                "testFindStringSubmatchIndex",
                test,
                RE2.compile(test.pat).findSubmatchIndex(test.text),
                false);
        }

        // Now come the monster AllSubmatch cases.

        // (Go: TestFindAllSubmatch)
        public void testFindAllUTF8Submatch()
        {
            List<byte[][]> result = RE2.compile(test.pat).findAllUTF8Submatch(test.textUTF8, -1);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("expected match; got none: {0}", test));
            }
            else if (test.matches.Length != result.Count)
            {
                Assert.Fail(
                    String.Format(
                        "expected {0} matches; got {1}: {2}", test.matches.Length, result.Count, test));
            }
            else
            {
                for (int k = 0; k < test.matches.Length; ++k)
                {
                    testSubmatchBytes("testFindAllSubmatch", test, k, result[k]);
                }
            }
        }

        // (Go: TestFindAllStringSubmatch)
        public void testFindAllSubmatch()
        {
            List<String[]> result = RE2.compile(test.pat).findAllSubmatch(test.text, -1);
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("expected no match; got one: {0}", test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("expected match; got none: {0}", test));
            }
            else if (test.matches.Length != result.Count)
            {
                Assert.Fail(
                    String.Format(
                        "expected {0} matches; got {1}: {2}", test.matches.Length, result.Count, test));
            }
            else
            {
                for (int k = 0; k < test.matches.Length; ++k)
                {
                    testSubmatch("testFindAllStringSubmatch", test, k, result[k]);
                }
            }
        }

        // (Go: testFindSubmatchIndex)
        private void testFindAllSubmatchIndexCommon(
            String testName, TestTest test, List<int[]> result, bool resultIndicesAreUTF8)
        {
            if (test.matches.Length == 0 && result == null)
            {
                // ok
            }
            else if (test.matches.Length == 0 && result != null)
            {
                Assert.Fail(String.Format("{0}: expected no match; got one: {1}", testName, test));
            }
            else if (test.matches.Length > 0 && result == null)
            {
                Assert.Fail(String.Format("{0}: expected match; got none: {1}", testName, test));
            }
            else if (test.matches.Length != result.Count)
            {
                Assert.Fail(
                    String.Format(
                        "{0}: expected {1} matches; got {2}: {3}",
                        testName,
                        test.matches.Length,
                        result.Count,
                        test));
            }
            else
            {
                for (int k = 0; k < test.matches.Length; ++k)
                {
                    testSubmatchIndices(testName, test, k, result[k], resultIndicesAreUTF8);
                }
            }
        }

        // (Go: TestFindAllSubmatchIndex)
        public void testFindAllUTF8SubmatchIndex()
        {
            testFindAllSubmatchIndexCommon(
                "testFindAllUTF8SubmatchIndex",
                test,
                RE2.compile(test.pat).findAllUTF8SubmatchIndex(test.textUTF8, -1),
                true);
        }

        // (Go: TestFindAllStringSubmatchIndex)
        public void testFindAllSubmatchIndex()
        {
            testFindAllSubmatchIndexCommon(
                "testFindAllSubmatchIndex",
                test,
                RE2.compile(test.pat).findAllSubmatchIndex(test.text, -1),
                false);
        }

        // The find_test.go benchmarks are ported to Benchmarks.java.
    }
}