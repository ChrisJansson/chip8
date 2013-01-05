namespace Cj.Chip8.Cpu
{
    public interface IKeyboard
    {
        bool IsKeyDown(byte key);
        byte WaitForKeyPress();
    }
}