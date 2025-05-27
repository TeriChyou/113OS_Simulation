// Models/PageFrame.cs
using System;

namespace FinalTermOS.Models
{
    public class PageFrame
    {
        public int FrameId { get; set; } // Identifier for the physical frame
        public Page Page { get; set; } // The page currently loaded in this frame (null if empty)
        public DateTime LoadedTime { get; set; } // For FIFO: when the page was loaded into this frame
        public DateTime LastUsedTime { get; set; } // For LRU: when the page was last accessed

        public PageFrame(int frameId)
        {
            FrameId = frameId;
            Page = null; // Initially empty
            LoadedTime = DateTime.MinValue;
            LastUsedTime = DateTime.MinValue;
        }

        // Helper to update properties when a page is loaded or accessed
        public void LoadPage(Page page)
        {
            Page = page;
            LoadedTime = DateTime.Now; // Update loaded time
            LastUsedTime = DateTime.Now; // Update last used time
        }

        public void AccessPage()
        {
            LastUsedTime = DateTime.Now; // Only update last used time
        }
    }
}