﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using Xunit;

namespace Zio.Tests
{
    public class TestSearchPattern
    {

        [Theory]
        // Test any-alone '*' match
        [InlineData("/a/b/c", "*", "x")]
        [InlineData("/a/b/c", "*", "x/y")]

        [InlineData("/a/b/c", "d/*", "x")]
        [InlineData("/a/b/c", "d/*", "x/y")]

        [InlineData("/a/b/c", "d/x", "x")]

        // Test exact match
        [InlineData("/a/b/c", "x", "x")]
        [InlineData("/a/b/c", "d/x", "x")]
        [InlineData("/a/b/c", "x", "d/x")]
        [InlineData("/a/b/c", "x", "d/e/x")]

        // Test regex match: ? pattern
        [InlineData("/a/b/c", "?", "x")]
        [InlineData("/a/b/c", "x?z", "xyz")]
        [InlineData("/a/b/c", "x?z", "d/xyz")]

        [InlineData("/a/b/c", "?yz", "xyz")]
        [InlineData("/a/b/c", "xy?", "xyz")]
        [InlineData("/a/b/c", "??", "ab")]
        [InlineData("/a/b/c", "??", "c", false)]
        [InlineData("/a/b/c", "?", "abc", false)]

        // Test regex match: * pattern
        [InlineData("/a/b/c", "x*", "x")]
        [InlineData("/a/b/c", "x*", "xyz")]
        [InlineData("/a/b/c", "*z", "z")]
        [InlineData("/a/b/c", "*z", "xyz")]
        [InlineData("/a/b/c", "x*z", "xyz")]
        [InlineData("/a/b/c", "x*z", "xblablaz")]
        [InlineData("/a/b/c", "x*z", "axyz", false)]
        [InlineData("/a/b/c", "x*z", "xyza", false)]
        [InlineData("/a/b/c", "x*.txt", "xyoyo.txt")]
        [InlineData("/a/b/c", "x*.txt", "x.txt")]
        [InlineData("/a/b/c", "*.txt", "x.txt")]
        [InlineData("/a/b/c", "x*.txt", "x_txt", false)]
        [InlineData("/a/b/c", "x?z", "d/xyz")]
        public void TestMatch(string path, string searchPattern, string pathToSearch, bool match = true)
        {
            var pathInfo = new PathInfo(path);
            var pathInfoCopy = pathInfo;
            var search = SearchPattern.Parse(ref pathInfoCopy, ref searchPattern);
            var pathToSearchInfo = new PathInfo(pathToSearch);
            Assert.Equal(match, search.Match(pathToSearchInfo));
        }


        [Fact]
        public void TestExpectedExceptions()
        {
            var path = new PathInfo("/yoyo");
            Assert.Throws<ArgumentNullException>(() =>
            {
                var nullPath = new PathInfo();
                string searchPattern = "valid";
                SearchPattern.Parse(ref nullPath, ref searchPattern);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                string searchPattern = null;
                SearchPattern.Parse(ref path, ref searchPattern);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                string searchPattern = "/notvalid";
                SearchPattern.Parse(ref path, ref searchPattern);
            });
        }

        [Fact]
        public void TestExtensions()
        {
            {
                var path = new PathInfo("/a/b/c/d.txt");
                Assert.Equal(new PathInfo("/a/b/c"), path.GetDirectory());
                Assert.Equal("d.txt", path.GetName());
                Assert.Equal("d", path.GetNameWithoutExtension());
                Assert.Equal(".txt", path.GetDotExtension());
                var newPath = path.ChangeExtension(".zip");
                Assert.Equal("/a/b/c/d.zip", newPath.FullName);
                Assert.Equal(new PathInfo("a/b/c/d.txt"), path.ToRelative());
                Assert.Equal(path, path.AssertAbsolute());
                Assert.Throws<ArgumentNullException>(() => new PathInfo().AssertNotNull());
                Assert.Throws<ArgumentException>(() => new PathInfo("not_absolute").AssertAbsolute());
            }

            {
                var path = new PathInfo("d.txt");
                Assert.Equal(PathInfo.Empty, path.GetDirectory());
                Assert.Equal("d.txt", path.GetName());
                Assert.Equal("d", path.GetNameWithoutExtension());
                Assert.Equal(".txt", path.GetDotExtension());
                var newPath = path.ChangeExtension(".zip");
                Assert.Equal("d.zip", newPath.FullName);
                Assert.Equal(new PathInfo("d.txt"), path.ToRelative());
            }
        }

    }
}