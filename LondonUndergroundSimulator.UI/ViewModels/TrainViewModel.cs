using LondonUndergroundSimulator.Engine.Models;
using System.ComponentModel;

public class TrainViewModel : INotifyPropertyChanged
{
    public Train Train { get; }

    public TrainViewModel(Train train)
    {
        Train = train;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}