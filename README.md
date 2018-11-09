# re2cs
Port of the regular expression engine re2j https://github.com/google/re2j

[RE2](https://github.com/google/re2) is a regular expression engine that runs in time linear in the size of the input. RE2/CS is a port of RE2/J (the Java port of the original RE2 C++ code) to C#.

As with other languages, C#'s regular expression api (System.Text.RegularExpressions) uses a backtracking mechanism
in matching regular expressions. This code uses [Thompson's NFA algorithm](https://dl.acm.org/citation.cfm?doid=363347.363387),
which was implemented by [Cox](https://swtch.com/~rsc/regexp/).

This port was done in two days, the first to convert the Java into C#, and the second to get the code working with a few tests.

--Ken Domino
