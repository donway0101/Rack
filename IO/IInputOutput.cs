namespace IO
{
    public interface IInputOutput
    {
        bool Getinput(Input input);
        void SetOutput(Output output, bool value);
        void Setup();
    }
}