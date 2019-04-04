namespace EcatIo
{
    public interface IInputOutput
    {
        bool GetInput(Input input);
        void SetOutput(Output output, bool value);
        void Setup();
    }
}