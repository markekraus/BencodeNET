﻿using System;
using System.Text;
using BencodeNET.Exceptions;
using BencodeNET.Objects;
using BencodeNET.Parsing;
using FluentAssertions;
using Xunit;

namespace BencodeNET.Tests.Parsing
{
    public class BStringParserTests
    {
        private BStringParser Parser { get; }

        public BStringParserTests()
        {
            Parser = new BStringParser();
        }

        [Theory]
        [InlineData("4:spam")]
        [InlineData("8:spameggs")]
        [InlineData("9:spam eggs")]
        [InlineData("9:spam:eggs")]
        [InlineData("14:!@#¤%&/()=?$|")]
        public void CanParseSimple(string bencode)
        {
            var parts = bencode.Split(new[] {':'}, 2);
            var length = int.Parse(parts[0]);
            var value = parts[1];

            var bstring = Parser.ParseString(bencode);

            bstring.Length.Should().Be(length);
            bstring.Should().Be(value);
        }

        [Fact]
        public void CanParse_EmptyString()
        {
            var bstring = Parser.ParseString("0:");

            bstring.Length.Should().Be(0);
            bstring.Should().Be("");
        }

        [Theory]
        [InlineData("5:spam")]
        [InlineData("6:spam")]
        [InlineData("100:spam")]
        public void LessCharsThanSpecified_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<InvalidBencodeException<BString>>();
        }

        [Theory]
        [InlineData("4spam")]
        [InlineData("10spam")]
        [InlineData("4-spam")]
        [InlineData("4.spam")]
        [InlineData("4;spam")]
        [InlineData("4,spam")]
        [InlineData("4|spam")]
        public void MissingDelimiter_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<InvalidBencodeException<BString>>();
        }

        [Theory]
        [InlineData("spam")]
        [InlineData("-spam")]
        [InlineData(".spam")]
        [InlineData(",spam")]
        [InlineData(";spam")]
        [InlineData("?spam")]
        [InlineData("!spam")]
        [InlineData("#spam")]
        public void NonDigitFirstChar_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<InvalidBencodeException<BString>>();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("4")]
        public void LessThanMinimumLength2_ThrowsInvalidBencodeException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<InvalidBencodeException<BString>>();
        }

        [Theory]
        [InlineData("12345678901:spam")]
        [InlineData("123456789012:spam")]
        [InlineData("1234567890123:spam")]
        [InlineData("12345678901234:spam")]
        public void LengthAboveMaxDigits10_ThrowsUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<UnsupportedBencodeException<BString>>();
        }

        [Theory]
        [InlineData("1:spam")]
        [InlineData("12:spam")]
        [InlineData("123:spam")]
        [InlineData("1234:spam")]
        [InlineData("12345:spam")]
        [InlineData("123456:spam")]
        [InlineData("1234567:spam")]
        [InlineData("12345678:spam")]
        [InlineData("123456789:spam")]
        [InlineData("1234567890:spam")]
        public void LengthAtOrBelowMaxDigits10_DoesNotThrowUnsupportedException(string bencode)
        {
            Action action = () => Parser.ParseString(bencode);
            action.ShouldNotThrow<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public void LengthAboveInt32MaxValue_ThrowsUnsupportedException()
        {
            var bencode = "2147483648:spam";
            Action action = () => Parser.ParseString(bencode);
            action.ShouldThrow<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public void LengthBelowInt32MaxValue_DoesNotThrowUnsupportedException()
        {
            var bencode = "2147483647:spam";
            Action action = () => Parser.ParseString(bencode);
            action.ShouldNotThrow<UnsupportedBencodeException<BString>>();
        }

        [Fact]
        public void CanParseEncodedAsLatin1()
        {
            var encoding = Encoding.GetEncoding("LATIN1");
            var expected = new BString("æøå", encoding);
            var parser = new BStringParser(encoding);

            // "3:æøå"
            var bytes = new byte[] {51, 58, 230, 248, 229};
            var bstring = parser.Parse(bytes);

            bstring.Should().Be(expected);
        }
    }
}
