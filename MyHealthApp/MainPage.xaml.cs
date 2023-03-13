using System.Numerics;

namespace MyHealthApp;

public partial class MainPage : ContentPage
{
    int stepCount = 0;
    Vector3 currentAccel = new Vector3();
    Vector3 previousAccel = new Vector3();
    double stepThreshold = 0.8; // still needs adjust ment


    public MainPage()
	{
        InitializeComponent();
        ToggleAccelerometer();
        thresholdLbl.Text = " " + stepThreshold;

    }
    static void OnProcessExit(object sender, EventArgs e)
    {
        
    }

    public void ToggleAccelerometer()
    {
        if (Accelerometer.Default.IsSupported)
        {
            if (!Accelerometer.Default.IsMonitoring)
            {
                // Turn on accelerometer
                Accelerometer.Default.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Default.Start(SensorSpeed.UI);
            }
            else
            {
                // Turn off accelerometer
                Accelerometer.Default.Stop();
                Accelerometer.Default.ReadingChanged -= Accelerometer_ReadingChanged;
            }
        }
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        //update's the current and subtracts it from the previous to get the delta
        var reading = e.Reading;
        currentAccel = reading.Acceleration;
        Vector3 deltaAccel = currentAccel - previousAccel;
        previousAccel = currentAccel;

        /*
         * https://www.aosabook.org/en/500L/a-pedometer-in-the-real-world.html#:~:text=We%20are%20going%20to%20count,in%20the%20direction%20of%20gravity.
         */
        const int filterLength = 10; // number of samples to use for the moving average filter
        Queue<Vector3> accelBuffer = new Queue<Vector3>();//first in first out
        //checks if the queue is full and if it is deletes the last one
        accelBuffer.Enqueue(deltaAccel);//adds to queue
        if (accelBuffer.Count > filterLength)
        {
            accelBuffer.Dequeue();
        }

        //gets the average of the latest added to the queue
        Vector3 filteredAccel = Vector3.Zero;
        foreach (Vector3 accel in accelBuffer)
        {
            filteredAccel += accel;
        }
        filteredAccel /= accelBuffer.Count;

        //cal the magnitutde of the filterd acceleration to get overall accelleration
        double accelMagnitude = filteredAccel.Length();
        //a step is detected when the acceleration magnitude crosses a certain threshold in the z direction
        bool isStep = accelMagnitude > stepThreshold && filteredAccel.Z < 0;

        if (isStep)
        {
            stepCount++;
            // update UI
            AccelLbl2.TextColor = Colors.Green;
            AccelLbl2.Text = "Steps:"+ stepCount;
        }

        // Update UI Label with accelerometer state
        AccelLbl1.TextColor = Colors.Green;
        AccelLbl1.Text = $"Accel: {Math.Round(accelMagnitude, 2)}";

    }

    void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
    {
        double value = args.NewValue;
        stepThreshold = value;
        thresholdLbl.Text = $"stepThreshold: {Math.Round(stepThreshold, 2)}";
    }

    private void ResetBtn_Clicked(object sender, EventArgs e)
    {
        stepCount = 0;
        AccelLbl2.Text = "Steps:" + stepCount;
    }
}

