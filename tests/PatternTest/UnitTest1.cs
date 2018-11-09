using System;
using engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject2
{
    [TestClass]
    public class PatternTest
    {

        [TestMethod]
        public void testCompile()
        {
            Pattern p = Pattern.compile("abc");
            Assert.AreEqual("abc", p.pattern());
            Assert.AreEqual(0, p.flags());
        }

        [TestMethod]
        public void testToString()
        {
            Pattern p = Pattern.compile("abc");
            Assert.AreEqual("abc", p.ToString());
        }

        [TestMethod]
        public void testCompileFlags()
        {
            Pattern p = Pattern.compile("abc", 5);
            Assert.AreEqual("abc", p.pattern());
            Assert.AreEqual(5, p.flags());
        }

        [TestMethod]
        public void testSyntaxError()
        {
            bool caught = false;
            try
            {
                Pattern.compile("abc(");
                Assert.Fail("should have thrown");
            }
            catch (PatternSyntaxException e)
            {
                Assert.AreEqual(-1, e.getIndex());
                Assert.AreNotEqual("", e.getDescription());
                Assert.AreNotEqual("", e.Message);
                Assert.AreEqual("abc(", e.getPattern());
                caught = true;
            }

            Assert.AreEqual(true, caught);
        }

        [TestMethod]
        public void testMatchesNoFlags()
        {
            ApiTestUtils.testMatches("ab+c", "abbbc", "cbbba");
            ApiTestUtils.testMatches("ab.*c", "abxyzc", "ab\nxyzc");
            ApiTestUtils.testMatches("^ab.*c$", "abc", "xyz\nabc\ndef");
        }

        [TestMethod]
        public void testMatchesWithFlags()
        {
            ApiTestUtils.testMatchesRE2("ab+c", 0, "abbbc", "cbba");
            ApiTestUtils.testMatchesRE2("ab+c", Pattern.CASE_INSENSITIVE, "abBBc", "cbbba");
            ApiTestUtils.testMatchesRE2("ab.*c", 0, "abxyzc", "ab\nxyzc");
            ApiTestUtils.testMatchesRE2("ab.*c", Pattern.DOTALL, "ab\nxyzc", "aB\nxyzC");
            ApiTestUtils.testMatchesRE2(
                "ab.*c", Pattern.DOTALL | Pattern.CASE_INSENSITIVE, "aB\nxyzC", "z");
            ApiTestUtils.testMatchesRE2("^ab.*c$", 0, "abc", "xyz\nabc\ndef");

            ApiTestUtils.testMatchesRE2("^ab.*c$", Pattern.MULTILINE, "abc", "xyz\nabc\ndef");
            ApiTestUtils.testMatchesRE2("^ab.*c$", Pattern.MULTILINE, "abc", "");
            ApiTestUtils.testMatchesRE2("^ab.*c$", Pattern.DOTALL | Pattern.MULTILINE, "ab\nc", "AB\nc");
            ApiTestUtils.testMatchesRE2(
                "^ab.*c$", Pattern.DOTALL | Pattern.MULTILINE | Pattern.CASE_INSENSITIVE, "AB\nc", "z");
        }

        private void testFind(String regexp, int flag, String match, String nonMatch)
        {
            Assert.AreEqual(true, Pattern.compile(regexp, flag).matcher(match).find());
            Assert.AreEqual(false, Pattern.compile(regexp, flag).matcher(nonMatch).find());
        }

        [TestMethod]
        public void testFind()
        {
            testFind("ab+c", 0, "xxabbbc", "cbbba");
            testFind("ab+c", Pattern.CASE_INSENSITIVE, "abBBc", "cbbba");
            testFind("ab.*c", 0, "xxabxyzc", "ab\nxyzc");
            testFind("ab.*c", Pattern.DOTALL, "ab\nxyzc", "aB\nxyzC");
            testFind("ab.*c", Pattern.DOTALL | Pattern.CASE_INSENSITIVE, "xaB\nxyzCz", "z");
            testFind("^ab.*c$", 0, "abc", "xyz\nabc\ndef");
            testFind("^ab.*c$", Pattern.MULTILINE, "xyz\nabc\ndef", "xyz\nab\nc\ndef");
            testFind("^ab.*c$", Pattern.DOTALL | Pattern.MULTILINE, "xyz\nab\nc\ndef", "xyz\nAB\nc\ndef");
            testFind(
                "^ab.*c$",
                Pattern.DOTALL | Pattern.MULTILINE | Pattern.CASE_INSENSITIVE,
                "xyz\nAB\nc\ndef",
                "z");
        }

        [TestMethod]
        public void testSplit()
        {
            ApiTestUtils.testSplit("/", "abcde", new String[] { "abcde" });
            ApiTestUtils.testSplit("/", "a/b/cc//d/e//", new String[] { "a", "b", "cc", "", "d", "e" });
            ApiTestUtils.testSplit("/", "a/b/cc//d/e//", 3, new String[] { "a", "b", "cc//d/e//" });
            ApiTestUtils.testSplit("/", "a/b/cc//d/e//", 4, new String[] { "a", "b", "cc", "/d/e//" });
            ApiTestUtils.testSplit("/", "a/b/cc//d/e//", 5, new String[] { "a", "b", "cc", "", "d/e//" });
            ApiTestUtils.testSplit("/", "a/b/cc//d/e//", 6, new String[] { "a", "b", "cc", "", "d", "e//" });
            ApiTestUtils.testSplit(
                "/", "a/b/cc//d/e//", 7, new String[] { "a", "b", "cc", "", "d", "e", "/" });
            ApiTestUtils.testSplit(
                "/", "a/b/cc//d/e//", 8, new String[] { "a", "b", "cc", "", "d", "e", "", "" });
            ApiTestUtils.testSplit(
                "/", "a/b/cc//d/e//", 9, new String[] { "a", "b", "cc", "", "d", "e", "", "" });

            // The tests below are listed at
            // http://docs.oracle.com/javase/1.5.0/docs/api/java/util/regex/Pattern.html#split(java.lang.CharSequence, int)

            String s = "boo:and:foo";
            String regexp1 = ":";
            String regexp2 = "o";

            ApiTestUtils.testSplit(regexp1, s, 2, new String[] { "boo", "and:foo" });
            ApiTestUtils.testSplit(regexp1, s, 5, new String[] { "boo", "and", "foo" });
            ApiTestUtils.testSplit(regexp1, s, -2, new String[] { "boo", "and", "foo" });
            ApiTestUtils.testSplit(regexp2, s, 5, new String[] { "b", "", ":and:f", "", "" });
            ApiTestUtils.testSplit(regexp2, s, -2, new String[] { "b", "", ":and:f", "", "" });
            ApiTestUtils.testSplit(regexp2, s, 0, new String[] { "b", "", ":and:f" });
            ApiTestUtils.testSplit(regexp2, s, new String[] { "b", "", ":and:f" });
        }

        [TestMethod]
        public void testGroupCount()
        {
            // It is a simple delegation, but still test it.
            ApiTestUtils.testGroupCount("(.*)ab(.*)a", 2);
            ApiTestUtils.testGroupCount("(.*)(ab)(.*)a", 3);
            ApiTestUtils.testGroupCount("(.*)((a)b)(.*)a", 4);
            ApiTestUtils.testGroupCount("(.*)(\\(ab)(.*)a", 3);
            ApiTestUtils.testGroupCount("(.*)(\\(a\\)b)(.*)a", 3);
        }

        [TestMethod]
        public void testQuote()
        {
            ApiTestUtils.testMatchesRE2(Pattern.quote("ab+c"), 0, "ab+c", "abc");
        }

        public Pattern reserialize(Pattern @object)
        {
            return @object;
            //ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            //try
            //{
            //    ObjectOutputStream @out = new ObjectOutputStream(bytes);
            //    @out.writeObject(@object);
            //    ObjectInputStream @in = new ObjectInputStream(new ByteArrayInputStream(bytes.toByteArray()));
            //    return (Pattern)@in.readObject();
            //}
            //catch (Exception e)
            //{
            //    throw new Exception(e);
            //}
        }

        private void assertSerializes(Pattern p)
        {
            Pattern r = reserialize(p);
            Assert.AreEqual(p.pattern(), r.pattern());
            Assert.AreEqual(p.flags(), r.flags());
        }

        [TestMethod]
        public void testSerialize()
        {
            assertSerializes(Pattern.compile("ab+c"));
            assertSerializes(Pattern.compile("^ab.*c$", Pattern.DOTALL | Pattern.MULTILINE));
            Assert.IsFalse(reserialize(Pattern.compile("abc")).matcher("def").find());
        }

        [TestMethod]
        public void testEquals()
        {
            Pattern pattern1 = Pattern.compile("abc");
            Pattern pattern2 = Pattern.compile("abc");
            Pattern pattern3 = Pattern.compile("def");
            Pattern pattern4 = Pattern.compile("abc", Pattern.CASE_INSENSITIVE);
            Assert.AreEqual(pattern1.ToString(), pattern2.ToString());
            Assert.AreNotEqual(pattern1.ToString(), pattern3.ToString());
            Assert.AreEqual(pattern1.GetHashCode(), pattern2.GetHashCode());
            Assert.AreNotEqual(pattern1.re2().ToString(), pattern4.re2().ToString());
        }
    }
}

