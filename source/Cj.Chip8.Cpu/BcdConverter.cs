namespace Cj.Chip8.Cpu
{
    public class BcdConverter : IBcdConverter
    {
        public byte[] ConvertToBcd(byte value)
        {
            var result = new byte[3];

            var hundreds = value/100;
            var tens = (value - hundreds*100)/10;
            var singles = (value - hundreds*100 - tens*10);

            result[0] = (byte)hundreds;
            result[1] = (byte)tens;
            result[2] = (byte)singles;

            return result;
        }
    }
}