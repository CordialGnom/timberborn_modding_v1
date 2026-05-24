using System.Collections.Generic;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public enum CutterPattern
    {
        All = 0,
        Checkered,
        LinesX,
        LinesY
    }

    /// <summary>
    /// Plain data transfer object replacing the old CutterToolConfigChangeEvent
    /// which carried a reference to the entire UI fragment. Cleaner separation.
    /// </summary>
    public class CutterToolConfigChangeEvent
    {
        public CutterPattern Pattern         { get; }
        public bool          TreeMarkOnly    { get; }
        public bool          IgnoreStumps    { get; }
        public bool          ClearCutArea    { get; }
        public bool          IgnoreDeadSapling { get; }
        public Dictionary<string, bool> TreeDict { get; }

        public CutterToolConfigChangeEvent(
            CutterPattern pattern,
            bool treeMarkOnly,
            bool ignoreStumps,
            bool clearCutArea,
            bool ignoreDeadSapling,
            Dictionary<string, bool> treeDict)
        {
            Pattern           = pattern;
            TreeMarkOnly      = treeMarkOnly;
            IgnoreStumps      = ignoreStumps;
            ClearCutArea      = clearCutArea;
            IgnoreDeadSapling = ignoreDeadSapling;
            TreeDict          = treeDict;
        }
    }

    public class CutterToolSelectedEvent
    {
        public CutterToolSelectedEvent() { }
    }

    public class CutterToolUnselectedEvent
    {
        public CutterToolUnselectedEvent() { }
    }
}
