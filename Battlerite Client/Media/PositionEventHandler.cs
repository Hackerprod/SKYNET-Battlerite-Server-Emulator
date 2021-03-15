using System;

public class PositionChangedEventArgs : EventArgs
{
    public int newPosition { get; }

    public PositionChangedEventArgs(int num)
    {
        newPosition = num;
    }
}
public delegate void PositionEventHandler(object sender, PositionChangedEventArgs e);
