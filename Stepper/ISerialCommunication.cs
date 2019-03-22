namespace GripperStepper
{
    public interface ISerialCommunication
    {
        void Close();
        void Connect();
        void Setup();
        void SendCommand(string cmd);
    }
}