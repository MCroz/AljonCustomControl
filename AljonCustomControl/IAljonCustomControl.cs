namespace AljonCustomControl
{
    interface IAljonCustomControl
    {
        int Depth { get; set; }
        MouseState MouseState { get; set; }
    }

    public enum MouseState
    {
        HOVER,
        DOWN,
        OUT
    }
}
