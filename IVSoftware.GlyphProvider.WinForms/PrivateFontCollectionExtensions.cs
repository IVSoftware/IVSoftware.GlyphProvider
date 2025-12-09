using IVSoftware.Portable;
using IVSoftware.Portable.Common.Exceptions;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace IVSoftware.WinOS
{
    public static class PrivateFontCollectionExtensions
    {
        public static FontFamily? GetFontFamily(this GlyphProvider provider, string endsWith = ".ttf")
        {
            var fontFamily = provider.Name;

            if (fontFamily.Contains('.'))
            {
                nameof(PrivateFontCollectionExtensions).ThrowHard<ArgumentException>(
                    "The '.' character is not allowed in the fontFamily argument");
            }
            if (Fonts.TryGetValue(fontFamily, out var cached))
            {
                return cached;
            }

            foreach (var asm in GlyphProvider.AppDomainAssemblyCache)
            {
                if (asm.GetResourcePath(fontFamily, endsWith) is { } resourcePath)
                {
                    using (Stream fontStream = asm.GetManifestResourceStream(resourcePath)
                           ?? throw new InvalidOperationException($"Failed to load stream for '{fontFamily}'."))
                    {
                        byte[] fontData = new byte[fontStream.Length];
                        fontStream.Read(fontData, 0, fontData.Length);

                        nint fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        try
                        {
                            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                            privateFontCollection.AddMemoryFont(fontPtr, fontData.Length);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(fontPtr); // Avoid memory leak
                        }

                        if (privateFontCollection.Families.Single(_ => _.Name == fontFamily) is FontFamily created)
                        {
                            Fonts[fontFamily] = created;
                            return created;
                        }
                    }
                }
            }

            nameof(PrivateFontCollectionExtensions).ThrowSoft<KeyNotFoundException>(
                $"Resource Lookup failed for '{fontFamily}'");
            return null;
        }

        public static FontFamily? LoadEmbeddedFont(this string fontFamily, string endsWith = ".ttf")
        {
            if (fontFamily.Contains('.'))
            {
                nameof(PrivateFontCollectionExtensions).ThrowHard<ArgumentException>(
                    "The '.' character is not allowed in the fontFamily argument");
            }
            if (Fonts.TryGetValue(fontFamily, out var cached))
            {
                return cached;
            }

            foreach (var asm in GlyphProvider.AppDomainAssemblyCache)
            {
                if (asm.GetResourcePath(fontFamily, endsWith) is { } resourcePath)
                {
                    using (Stream fontStream = asm.GetManifestResourceStream(resourcePath)
                           ?? throw new InvalidOperationException($"Failed to load stream for '{fontFamily}'."))
                    {
                        byte[] fontData = new byte[fontStream.Length];
                        fontStream.Read(fontData, 0, fontData.Length);

                        nint fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        try
                        {
                            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
                            privateFontCollection.AddMemoryFont(fontPtr, fontData.Length);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(fontPtr); // Avoid memory leak
                        }

                        if (privateFontCollection.Families.Single(_ => _.Name == fontFamily) is FontFamily created)
                        {
                            Fonts[fontFamily] = created;
                            return created;
                        }
                    }
                }
            }

            nameof(PrivateFontCollectionExtensions).ThrowSoft<KeyNotFoundException>(
                $"Resource Lookup failed for '{fontFamily}'");
            return null;
        }
        private static PrivateFontCollection privateFontCollection { get; } = new PrivateFontCollection();

        static Dictionary<string, FontFamily> Fonts = new Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);
    }
}
