using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using AssettoCorsaSharedMemory;
using ScottPlot;

namespace love
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Random rand = new Random();
        double[] liveData = new double[400];
        DataGen.Electrocardiogram ecg = new DataGen.Electrocardiogram();
        Stopwatch sw = Stopwatch.StartNew();

        private Timer _updateDataTimer;
        private DispatcherTimer _renderTimer;

        public float speed;

        public MainWindow()
        {
            AssettoCorsa ac = new AssettoCorsa();
            
            //ac.StaticInfoInterval = 5000;
            
            // add event listener for StaticInfo
            // probably want to use this later
            //ac.StaticInfoUpdated += ac_StaticInfoUpdated;

            ac.PhysicsUpdated += ac_PhysicsInfoUpdated;

            // connect to shared memory and start interval timers 
            ac.Start();

            InitializeComponent();
            wpfPlot1.Configuration.MiddleClickAutoAxisMarginX = 0;

            // plot the data array only once
            wpfPlot1.Plot.AddSignal(liveData);
            wpfPlot1.Plot.AxisAutoX(margin: 0);
            wpfPlot1.Plot.SetAxisLimits(yMin: -1, yMax: 350);

            // create a traditional timer to update the data
            _updateDataTimer = new Timer(_ => UpdateData(), null, 0, 5);

            // create a separate timer to update the GUI
            _renderTimer = new DispatcherTimer();
            _renderTimer.Interval = TimeSpan.FromMilliseconds(10);
            _renderTimer.Tick += Render;
            _renderTimer.Start();

            Closed += (sender, args) =>
            {
                _updateDataTimer?.Dispose();
                _renderTimer?.Stop();
            };
        }

        public void ac_PhysicsInfoUpdated(object sender, PhysicsEventArgs e)
        {
            speed = e.Physics.SpeedKmh;
            
        }

        void UpdateData()
        {
            // "scroll" the whole chart to the left
            Array.Copy(liveData, 1, liveData, 0, liveData.Length - 1);

            // place the newest data point at the end
            double nextValue = ecg.GetVoltage(sw.Elapsed.TotalSeconds);
            liveData[liveData.Length - 1] = speed;
            Application.Current.Dispatcher.Invoke(new Action(() => { wpfPlot1.Render(); }));
        }

        void Render(object sender, EventArgs e)
        {
            wpfPlot1.Render();
        }
    }
}