using CSweet.Compute.Runtime;
using Xunit;

namespace CSweet.Compute.Tests;

public class MacComputeProtectedPathsTests
{
    [Fact]
    public void Darwin_checks_each_vnode_and_rejects_writable_targets_and_links()
    {
        if (!OperatingSystem.IsMacOS()) return;
        MacComputeProtectedPaths.Verify("/private");
        Assert.Throws<UnauthorizedAccessException>(() => MacComputeProtectedPaths.Verify("/private/tmp"));
        Assert.Throws<UnauthorizedAccessException>(() => MacComputeProtectedPaths.Verify("/tmp"));
    }
}
