namespace GripperStepper
{
    public interface ISerialCommunication
    {
        void Close();
        void Connect();
        void Stepup();
        void SendCommand(string cmd);
    }
}