namespace Nofun.Driver.Input
{
    public interface IInputDriver
    {
        uint GetButtonData();
        bool KeyPressed(uint keyCodeAsciiOrImplDefined);
        void EndFrame();
    };
}