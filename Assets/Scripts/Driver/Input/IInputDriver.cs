namespace Nofun.Driver.Input
{
    public interface IInputDriver
    {
        uint GetButtonData();
        void EndFrame();
    };
}