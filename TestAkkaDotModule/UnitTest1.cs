using System;
using AkkaNetCoreTest;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule
{
    /// <summary>
    /// ���� : TestKitXunit ��� �׽�Ʈ
    /// ���� : TestKitXunit ��ӹ� �ܼ���� �׽�Ʈ
    /// </summary>
    public class UnitTests : TestKitXunit
    {
        public UnitTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory(DisplayName = "�����׽�Ʈ ���Ȯ��")]
        [InlineData(100)]
        public void Test1(int cutoff)
        {
            Console.WriteLine($"���Ȯ��.....{cutoff}");
        }
    }
}