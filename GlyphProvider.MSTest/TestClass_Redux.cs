

namespace GlyphProvider.MSTest;

[TestClass]
public class TestClass_Redux
{
    [TestMethod]
    public void Test_Redux()
    {
        var asm = typeof(IVSoftware.Portable.GlyphProvider).Assembly;
        var names = asm.GetManifestResourceNames();

    }
}
