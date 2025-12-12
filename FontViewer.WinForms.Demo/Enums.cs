using IVSoftware.Portable;

namespace FontViewer.WinForms.Demo
{
    [CssName("icon-media-control")]
    public enum StdIconMediaControl
    {
        [CssName("stop")]
        Stop,

        [CssName("pause")]
        Pause,

        [CssName("to-end")]
        ToEnd,

        [CssName("to-end-alt")]
        ToEndAlt,

        [CssName("to-start")]
        ToStart,

        [CssName("to-start-alt")]
        ToStartAlt,

        [CssName("fast-fw")]
        FastFw,

        [CssName("fast-bw")]
        FastBw,

        [CssName("eject")]
        Eject,
    }
}
