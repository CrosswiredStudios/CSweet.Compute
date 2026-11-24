using System.Runtime.InteropServices;

namespace CSweet.Compute.Runtime;

/// <summary>Checks the Darwin vnode itself and every ancestor before using protected Office state.</summary>
public static class MacComputeProtectedPaths
{
    public static void Verify(string path)
    {
        if (!OperatingSystem.IsMacOS()) throw new PlatformNotSupportedException();
        if (!Path.IsPathFullyQualified(path)) throw new UnauthorizedAccessException("An absolute protected path is required.");
        var target = Path.GetFullPath(path);
        for (string? current = target; current is not null; current = Path.GetDirectoryName(current))
        {
            // getattrlist returns the requested common attributes in bit order. FSOPT_NOFOLLOW
            // returns the type of the link itself. Darwin VREG = 1, VDIR = 2.
            var attributes = new AttributeList { Count = 5, Common = 0x8 | 0x8000 | 0x20000 };
            if (getattrlist(current, ref attributes, out var info, (nuint)Marshal.SizeOf<Vnode>(), 1) != 0 ||
                info.Length != Marshal.SizeOf<Vnode>() || info.Owner != 0 ||
                (current == target ? info.Type is not (1 or 2) : info.Type != 2))
                throw new UnauthorizedAccessException("Compute paths must be real, root-owned files or directories.");
            // A root-owned sticky ancestor (e.g. /private/tmp) protects its root-owned children.
            if ((info.Mode & 0x12) != 0 && (current == target || (info.Mode & 0x200) == 0))
                throw new UnauthorizedAccessException("Compute paths must be protected against group and other writes.");
            var acl = acl_get_link_np(current, 0x100);
            if (acl != IntPtr.Zero)
            {
                acl_free(acl);
                throw new UnauthorizedAccessException("Extended ACLs are unsupported on protected Compute paths.");
            }
            if (Marshal.GetLastPInvokeError() != 2)
                throw new UnauthorizedAccessException("Could not verify the extended ACL of a Compute path.");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AttributeList { public ushort Count, Reserved; public uint Common, Volume, Directory, File, Fork; }
    [StructLayout(LayoutKind.Sequential)]
    private struct Vnode { public uint Length, Type, Owner, Mode; }
    [DllImport("libSystem", SetLastError = true)]
    private static extern int getattrlist(string path, ref AttributeList attributes, out Vnode data, nuint size, uint options);
    [DllImport("libSystem", SetLastError = true)] private static extern IntPtr acl_get_link_np(string path, int type);
    [DllImport("libSystem")] private static extern int acl_free(IntPtr acl);
}
