namespace GripperStepper
{
    public interface ISerialCommunication
    {
        void Close();
        void Connect();
        void Initialization();
        void SendCommand(string cmd);
    }
}