using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Net.Expressions.Test
{
    public class PredicateTest
    {
        [Fact]
        public void Test1()
        {
            var items = new[] { "ali", "veli", "49", "50" };
            var query = Predicate<string>.New(p => p.StartsWith("a"))
                .Or(p => p.EndsWith("i"));
            var result = items.Where(query.AsFunc()).ToArray();
        }
    }
}
