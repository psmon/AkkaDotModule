using System;
using AkkaNetCoreTest;
using Xunit;
using Xunit.Abstractions;

namespace TestAkkaDotModule
{
    /// <summary>
    /// 설명 : TestKitXunit 상속 테스트
    /// 목적 : TestKitXunit 상속및 콘솔출력 테스트
    /// </summary>
    public class UnitTests : TestKitXunit
    {
        public UnitTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory(DisplayName = "유닛테스트 출력확인")]
        [InlineData(100)]
        public void Test1(int cutoff)
        {
            Console.WriteLine($"출력확인.....{cutoff}");
        }
    }
}