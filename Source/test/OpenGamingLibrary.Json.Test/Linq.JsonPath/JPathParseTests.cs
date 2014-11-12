#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using OpenGamingLibrary.Json.Linq.JsonPath;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = OpenGamingLibrary.Json.Test.Assert;
#else
using Xunit;
#endif
using OpenGamingLibrary.Json.Linq;
#if NET20
using OpenGamingLibrary.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using OpenGamingLibrary.Xunit.Extensions;

namespace OpenGamingLibrary.Json.Test.Linq.JsonPath
{
    
    public class JPathParseTests : TestFixtureBase
    {
        [Fact]
        public void SingleProperty()
        {
            JPath path = new JPath("Blah");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedProperty()
        {
            JPath path = new JPath("['Blah']");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JPath path = new JPath("[  'Blah'  ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithDots()
        {
            JPath path = new JPath("['Blah.Ha']");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah.Ha", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleQuotedPropertyWithBrackets()
        {
            JPath path = new JPath("['[*]']");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("[*]", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRoot()
        {
            JPath path = new JPath("$.Blah");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JPath path = new JPath(" $.Blah ");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void RootWithBadWhitespace()
        {
            AssertException.Throws<JsonException>(() => { new JPath("$ .Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void NoFieldNameAfterDot()
        {
            AssertException.Throws<JsonException>(() => { new JPath("$.Blah."); }, @"Unexpected end while parsing path.");
        }

        [Fact]
        public void RootWithBadWhitespace2()
        {
            AssertException.Throws<JsonException>(() => { new JPath("$. Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [Fact]
        public void WildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.*");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardArrayWithRoot()
        {
            JPath path = new JPath("$.[*]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void RootArrayNoDot()
        {
            JPath path = new JPath("$[1]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArray()
        {
            JPath path = new JPath("[*]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void WildcardArrayWithProperty()
        {
            JPath path = new JPath("[ * ].derp");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal(null, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.Equal("derp", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void QuotedWildcardPropertyWithRoot()
        {
            JPath path = new JPath("$.['*']");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("*", ((FieldFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void SingleScanWithRoot()
        {
            JPath path = new JPath("$..Blah");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal("Blah", ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardScanWithRoot()
        {
            JPath path = new JPath("$..*");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void WildcardScanWithRootWithWhitespace()
        {
            JPath path = new JPath("$..* ");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [Fact]
        public void TwoProperties()
        {
            JPath path = new JPath("Blah.Two");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("Two", ((FieldFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void OnePropertyOneScan()
        {
            JPath path = new JPath("Blah..Two");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("Two", ((ScanFilter)path.Filters[1]).Name);
        }

        [Fact]
        public void SinglePropertyAndIndexer()
        {
            JPath path = new JPath("Blah[0]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [Fact]
        public void SinglePropertyAndExistsQuery()
        {
            JPath path = new JPath("Blah[ ?( @..name ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Exists, expressions.Operator);
            Assert.Equal(1, expressions.Path.Count);
            Assert.Equal("name", ((ScanFilter)expressions.Path[0]).Name);
        }

        [Fact]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JPath path = new JPath("Blah[ ?( @.name=='hi' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal("hi", (string)expressions.Value);
        }

        [Fact]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal("h'i", (string)expressions.Value);
        }

        [Fact]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JPath path = new JPath(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal("h\\i", (string)expressions.Value);
        }

        [Fact]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            AssertException.Throws<JsonException>(() => { new JPath(@"Blah[ ?( @.name=='h\i' ) ]"); }, @"Unknown escape chracter: \i");
        }

        [Fact]
        public void SinglePropertyAndFilterWithFalse()
        {
            JPath path = new JPath("Blah[ ?( @.name==false ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal(false, (bool)expressions.Value);
        }

        [Fact]
        public void SinglePropertyAndFilterWithTrue()
        {
            JPath path = new JPath("Blah[ ?( @.name==true ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal(true, (bool)expressions.Value);
        }

        [Fact]
        public void SinglePropertyAndFilterWithNull()
        {
            JPath path = new JPath("Blah[ ?( @.name==null ) ]");
            Assert.Equal(2, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.Equal(QueryOperator.Equals, expressions.Operator);
            Assert.Equal(null, expressions.Value.Value);
        }

        [Fact]
        public void FilterWithScan()
        {
            JPath path = new JPath("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal("name", ((ScanFilter)expressions.Path[0]).Name);
        }

        [Fact]
        public void FilterWithNotEquals()
        {
            JPath path = new JPath("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithNotEquals2()
        {
            JPath path = new JPath("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.NotEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThan()
        {
            JPath path = new JPath("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.LessThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithLessThanOrEquals()
        {
            JPath path = new JPath("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThan()
        {
            JPath path = new JPath("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.GreaterThan, expressions.Operator);
        }

        [Fact]
        public void FilterWithGreaterThanOrEquals()
        {
            JPath path = new JPath("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [Fact]
        public void FilterWithInteger()
        {
            JPath path = new JPath("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(12, (int)expressions.Value);
        }

        [Fact]
        public void FilterWithNegativeInteger()
        {
            JPath path = new JPath("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(-12, (int)expressions.Value);
        }

        [Fact]
        public void FilterWithFloat()
        {
            JPath path = new JPath("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(12.1d, (double)expressions.Value);
        }

        [Fact]
        public void FilterExistWithAnd()
        {
            JPath path = new JPath("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.And, expressions.Operator);
            Assert.Equal(2, expressions.Expressions.Count);
            Assert.Equal("name", ((FieldFilter)((BooleanQueryExpression)expressions.Expressions[0]).Path[0]).Name);
            Assert.Equal(QueryOperator.Exists, expressions.Expressions[0].Operator);
            Assert.Equal("title", ((FieldFilter)((BooleanQueryExpression)expressions.Expressions[1]).Path[0]).Name);
            Assert.Equal(QueryOperator.Exists, expressions.Expressions[1].Operator);
        }

        [Fact]
        public void FilterExistWithAndOr()
        {
            JPath path = new JPath("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(QueryOperator.And, andExpression.Operator);
            Assert.Equal(2, andExpression.Expressions.Count);
            Assert.Equal("name", ((FieldFilter)((BooleanQueryExpression)andExpression.Expressions[0]).Path[0]).Name);
            Assert.Equal(QueryOperator.Exists, andExpression.Expressions[0].Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.Equal(2, orExpression.Expressions.Count);
            Assert.Equal("title", ((FieldFilter)((BooleanQueryExpression)orExpression.Expressions[0]).Path[0]).Name);
            Assert.Equal(QueryOperator.Exists, orExpression.Expressions[0].Operator);
            Assert.Equal("pie", ((FieldFilter)((BooleanQueryExpression)orExpression.Expressions[1]).Path[0]).Name);
            Assert.Equal(QueryOperator.Exists, orExpression.Expressions[1].Operator);
        }

        [Fact]
        public void BadOr1()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||)]"), "Unexpected character while parsing path query: )");
        }

        [Fact]
        public void BaddOr2()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name|)]"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr3()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name|"), "Unexpected character while parsing path query: |");
        }

        [Fact]
        public void BaddOr4()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||"), "Path ended with open query.");
        }

        [Fact]
        public void NoAtAfterOr()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||s"), "Unexpected character while parsing path query: s");
        }

        [Fact]
        public void NoPathAfterAt()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||@"), @"Path ended with open query.");
        }

        [Fact]
        public void NoPathAfterDot()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||@."), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void NoPathAfterDot2()
        {
            AssertException.Throws<JsonException>(() => new JPath("[?(@.name||@.)]"), @"Unexpected end while parsing path.");
        }

        [Fact]
        public void FilterWithFloatExp()
        {
            JPath path = new JPath("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.Equal(5.56789e+0, (double)expressions.Value);
        }

        [Fact]
        public void MultiplePropertiesAndIndexers()
        {
            JPath path = new JPath("Blah[0]..Two.Three[1].Four");
            Assert.Equal(6, path.Filters.Count);
            Assert.Equal("Blah", ((FieldFilter) path.Filters[0]).Name);
            Assert.Equal(0, ((ArrayIndexFilter) path.Filters[1]).Index);
            Assert.Equal("Two", ((ScanFilter)path.Filters[2]).Name);
            Assert.Equal("Three", ((FieldFilter)path.Filters[3]).Name);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.Equal("Four", ((FieldFilter)path.Filters[5]).Name);
        }

        [Fact]
        public void BadCharactersInIndexer()
        {
            AssertException.Throws<JsonException>(() => { new JPath("Blah[[0]].Two.Three[1].Four"); }, @"Unexpected character while parsing path indexer: [");
        }

        [Fact]
        public void UnclosedIndexer()
        {
            AssertException.Throws<JsonException>(() => { new JPath("Blah[0"); }, @"Path ended with open indexer.");
        }

        [Fact]
        public void IndexerOnly()
        {
            JPath path = new JPath("[111119990]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void IndexerOnlyWithWhitespace()
        {
            JPath path = new JPath("[  10  ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [Fact]
        public void MultipleIndexes()
        {
            JPath path = new JPath("[111119990,3]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.Equal(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.Equal(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleIndexesWithWhitespace()
        {
            JPath path = new JPath("[   111119990  ,   3   ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.Equal(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.Equal(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [Fact]
        public void MultipleQuotedIndexes()
        {
            JPath path = new JPath("['111119990','3']");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.Equal("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.Equal("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JPath path = new JPath("[ '111119990' , '3' ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.Equal("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.Equal("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [Fact]
        public void SlicingIndexAll()
        {
            JPath path = new JPath("[111119990:3:2]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndex()
        {
            JPath path = new JPath("[111119990:3]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexNegative()
        {
            JPath path = new JPath("[-111119990:-3:-2]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStop()
        {
            JPath path = new JPath("[  -3  :  ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(null, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexEmptyStart()
        {
            JPath path = new JPath("[ : 1 : ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(null, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void SlicingIndexWhitespace()
        {
            JPath path = new JPath("[  -111119990  :  -3  :  -2  ]");
            Assert.Equal(1, path.Filters.Count);
            Assert.Equal(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.Equal(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.Equal(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [Fact]
        public void EmptyIndexer()
        {
            AssertException.Throws<JsonException>(() => { new JPath("[]"); }, "Array index expected.");
        }

        [Fact]
        public void IndexerCloseInProperty()
        {
            AssertException.Throws<JsonException>(() => { new JPath("]"); }, "Unexpected character while parsing path: ]");
        }

        [Fact]
        public void AdjacentIndexers()
        {
            JPath path = new JPath("[1][0][0][" + int.MaxValue + "]");
            Assert.Equal(4, path.Filters.Count);
            Assert.Equal(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.Equal(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.Equal(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [Fact]
        public void MissingDotAfterIndexer()
        {
            AssertException.Throws<JsonException>(() => { new JPath("[1]Blah"); }, "Unexpected character following indexer: B");
        }

        [Fact]
        public void PropertyFollowingEscapedPropertyName()
        {
            JPath path = new JPath("frameworks.aspnetcore50.dependencies.['System.Xml.ReaderWriter'].source");
            Assert.Equal(5, path.Filters.Count);

            Assert.Equal("frameworks", ((FieldFilter)path.Filters[0]).Name);
            Assert.Equal("aspnetcore50", ((FieldFilter)path.Filters[1]).Name);
            Assert.Equal("dependencies", ((FieldFilter)path.Filters[2]).Name);
            Assert.Equal("System.Xml.ReaderWriter", ((FieldFilter)path.Filters[3]).Name);
            Assert.Equal("source", ((FieldFilter)path.Filters[4]).Name);
        }
    }
}