using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Media.Imaging
{
    public enum StringHorizontalAlignment { Center, Left};
    public enum StringVerticalAlignment { Center, Top };
    public enum StringDirection  { Horizontal, Vertical };

    public class StringOptions
    {
        public readonly StringHorizontalAlignment HorizontalAlignment;
        public readonly StringVerticalAlignment VerticalAlignment;
        public readonly StringDirection Direction;

        public StringOptions(StringHorizontalAlignment horizontalAlignment = StringHorizontalAlignment.Left, StringVerticalAlignment verticalAlignment = StringVerticalAlignment.Top, StringDirection direction = StringDirection.Horizontal)
        {
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            Direction = direction;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return HorizontalAlignment.GetHashCode() ^ VerticalAlignment.GetHashCode() ^ Direction.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringOptions;
            if (other == null) return false;
            return HorizontalAlignment == other.HorizontalAlignment && VerticalAlignment == other.VerticalAlignment && Direction == other.Direction;
        }
    }
}
