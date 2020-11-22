using Microsoft.VisualStudio.TestTools.UnitTesting;
using Petl.Activators;
using Petl.Builder;
using Petl.Sources;
using Petl.Targets;

namespace Petl.Tests
{
    [TestClass]
    public class TestMemberEvaluation
    {
        [TestMethod]
        public void CanHandleMemberToMember()
        {
            var evaluator = new Evaluator(
                new ReflectionActivator(),
                new object[]
                {
                    new EvalSet<Foo, Bar>(
                        new [] {
                            new ExpressionProgrammableSource<Foo, Bar>(
                                m => m.Message,
                                new []
                                {
                                    new ExpressionProgrammableTarget<Foo, Bar>(m => m.Content)
                                }
                            )
                        }
                    ){ CanHandle = (m) => true }
                }
            );

            var output = evaluator.Evaluate<Foo, Bar>(new Foo { Message = "Hello, world!" });

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Content.Equals("Hello, world!", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void CanUseBuilderForMemberToMember()
        {
            var builder = new EvaluatorBuilder();
            builder
                .WithActivator(new ReflectionActivator())
                .Map<Foo, Bar>(m => true)
                .AddSource(new ExpressionProgrammableSource<Foo, Bar>(m => m.Message))
                .AddTarget(new ExpressionProgrammableTarget<Foo, Bar>(m => m.Content));

            var evaluator = builder.Build();

            var output = evaluator.Evaluate<Foo, Bar>(new Foo { Message = "Hello, world!" });

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Content.Equals("Hello, world!", System.StringComparison.OrdinalIgnoreCase));
        }
    }

    public class Foo
    {
        public string Message { get; set; }
    }

    public class Bar
    {
        public string Content { get; set; }
    }
}
