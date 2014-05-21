using System;
using System.Linq.Expressions;

using Mandro.Blog.Worker.Engine;

using NUnit.Framework;

namespace Mandro.Blog.Tests
{
    public class RedirectTests
    {
        private abstract class Dummy
        {
            public dynamic Index(dynamic context)
            {
                return null;
            }
        }

        [Test]
        public void ShouldExtractControllerNameFromExpression()
        {
            // given
            Expression<Func<Dummy, Func<dynamic, dynamic>>> redirectExpression = controller => controller.Index;

            // when
            var uri = Redirect.To(redirectExpression);

            // then
            Assert.That(uri.ToString(), Is.StringStarting("/Dummy/"));
        }

        [Test]
        public void ShouldExtractMethodNameFromExpression()
        {
            // given
            Expression<Func<Dummy, Func<dynamic, dynamic>>> redirectExpression = controller => controller.Index;

            // when
            var uri = Redirect.To(redirectExpression);

            // then
            Assert.That(uri.ToString(), Is.StringStarting("/Dummy/Index"));
        }

        [Test]
        public void ShouldIncludeSingleParameter()
        {
            // given
            Expression<Func<Dummy, Func<dynamic, dynamic>>> redirectExpression = controller => controller.Index;

            // when
            var uri = Redirect.To(redirectExpression, new object[]{"Parameter"});

            // then
            Assert.That(uri.ToString(), Is.StringStarting("/Dummy/Index/Parameter"));
        }

        [Test]
        public void ShouldIncludeMultipleParameters()
        {
            // given
            Expression<Func<Dummy, Func<dynamic, dynamic>>> redirectExpression = controller => controller.Index;

            // when
            var uri = Redirect.To(redirectExpression, new [] {"Parameter1", "Parameter2"});

            // then
            Assert.That(uri.ToString(), Is.EqualTo("/Dummy/Index/Parameter1/Parameter2"));
        }
    }
}
