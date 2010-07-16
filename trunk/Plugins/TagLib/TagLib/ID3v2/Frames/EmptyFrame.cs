using TagLib.Id3v2;

namespace TagLib.Id3v2
{
    /// <summary>
    /// Faster then throwing exceptions
    /// </summary>
    public class EmptyFrame:Frame
    {
        public EmptyFrame() { }
        protected override void ParseFields(ByteVector data, byte version)
        {
            throw new System.NotImplementedException();
        }

        protected override ByteVector RenderFields(byte version)
        {
            throw new System.NotImplementedException();
        }
    }
}
