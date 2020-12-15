#region hardware settings

//#define TRUEALAZAR
#undef TRUEALAZAR

//#define TRUEDAQ
#undef TRUEDAQ

//#define TRUEIMAQ
#undef TRUEIMAQ

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NationalInstruments;
using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using NationalInstruments.Visa;
using NationalInstruments.DAQmx;
using Task = NationalInstruments.DAQmx.Task;

using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Security;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Windows.Forms;
using System.Reflection;

#region more hardware settings

#if TRUEALAZAR

#endif

#if TRUEDAQ

#endif

#if TRUEIMAQ

#endif

#endregion



namespace nOCT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CUIData UIData;
        LinkedList<CDataNode> nodeList = new LinkedList<CDataNode>();
        private CThreadData threadData = new CThreadData();
        DispatcherTimer timerUIUpdate = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        nOCTcudaWrapper cudaWrapper;
        nOCTippWrapper ippWrapper;
        nOCTimaqWrapper imaqWrapper;

        public MainWindow()
        {
            InitializeComponent();

            // initialize UI
            UIData = new CUIData { };
            this.DataContext = UIData;
            InitializeUI();

            #region start update timer
            timerUIUpdate.Tick += new EventHandler(UIUpdate);
            timerUIUpdate.IsEnabled = true;
            #endregion

            GC.Collect();
        }

        private void InitializeUI()
        {

            #region populate list of CUDA devices
            int nDeviceCount = 0;
            StringBuilder strDeviceName = new StringBuilder(256);
            int nRet = nOCTcudaWrapper.getDeviceCount(ref nDeviceCount);
            for (int nDevice = 0; nDevice < nDeviceCount; nDevice++)
            {
                nRet = nOCTcudaWrapper.getDeviceName(nDevice, strDeviceName);
                cbLLCUDADevice.Items.Add(nDevice + ":" + strDeviceName);
            }   // for (int nDevice
            #endregion

            #region load last parameter file (if it exists)
            if (File.Exists("lastparameterfilename.txt"))
            {
                string line;
                StreamReader lastParameter = new StreamReader("lastparameterfilename.txt");
                line = lastParameter.ReadLine();
                lastParameter.Close();
                if (File.Exists(line))
                {
                    UIData.strLLConfigurationFilename = line;
                    LoadParameterFile(line);
                }   // if (File.Exists(line
            }   // if (File.Exists("lastparameterfilename.txt"
            #endregion

            UIData.pnLinkedList = new int[2, 2];
            Array.Clear(UIData.pnLinkedList, 0, UIData.pnLinkedList.Length);
            graphLRDiagnostics.DataSource = UIData.pnLinkedList;

        }   // private void InitializeUI

        void LoadParameterFile(string strFile)
        {
            StreamReader sr = new StreamReader(strFile);
            string line = sr.ReadLine();
            while (line != null)
            {
                int n1 = line.IndexOf("`");
                int n2 = line.IndexOf("'");
                if (n1 != -1)
                {
                    string parametername = line.Substring(0, n1 - 1);
                    string parametervalue = line.Substring(n1 + 1, n2 - n1 - 1);

                    #region template
                    if (parametername == UIData.name_strXXX) UIData.strXXX = parametervalue;
                    if (parametername == UIData.name_nXXX) UIData.nXXX = int.Parse(parametervalue);
                    if (parametername == UIData.name_bXXX) UIData.bXXX = bool.Parse(parametervalue);
                    if (parametername == UIData.name_dXXX) UIData.dXXX = double.Parse(parametervalue);
                    #endregion
                    #region UL
                    if (parametername == UIData.name_nULDisplayIndex) UIData.nULDisplayIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULAlazarReferenceIndex) UIData.nULAlazarReferenceIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULAlazarTop) UIData.nULAlazarTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULAlazarLeft) UIData.nULAlazarLeft = int.Parse(parametervalue);
                    if (parametername == UIData.name_dULAlazarMax) UIData.dULAlazarMax = double.Parse(parametervalue);
                    if (parametername == UIData.name_dULAlazarMin) UIData.dULAlazarMin = double.Parse(parametervalue);
                    if (parametername == UIData.name_nULDAQReferenceIndex) UIData.nULDAQReferenceIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULDAQTop) UIData.nULDAQTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULDAQLeft) UIData.nULDAQLeft = int.Parse(parametervalue);
                    if (parametername == UIData.name_dULDAQMax) UIData.dULDAQMax = double.Parse(parametervalue);
                    if (parametername == UIData.name_dULDAQMin) UIData.dULDAQMin = double.Parse(parametervalue);
                    if (parametername == UIData.name_nULIMAQReferenceIndex) UIData.nULIMAQReferenceIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULIMAQTop) UIData.nULIMAQTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULIMAQLeft) UIData.nULIMAQLeft = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULIMAQCameraIndex) UIData.nULIMAQCameraIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_dULIMAQMax) UIData.dULIMAQMax = double.Parse(parametervalue);
                    if (parametername == UIData.name_dULIMAQMin) UIData.dULIMAQMin = double.Parse(parametervalue);
                    if (parametername == UIData.name_nULIntensityTop) UIData.nULIntensityTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nULIntensityLeft) UIData.nULIntensityLeft = int.Parse(parametervalue);
                    if (parametername == UIData.name_dULIntensityMax) UIData.dULIntensityMax = double.Parse(parametervalue);
                    if (parametername == UIData.name_dULIntensityMin) UIData.dULIntensityMin = double.Parse(parametervalue);
                    #endregion
                    #region UR
                    if (parametername == UIData.name_nURDisplayIndex) UIData.nURDisplayIndex = int.Parse(parametervalue);
                    if (parametername == UIData.name_nURIntensityTop) UIData.nURIntensityTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nURIntensityLeft) UIData.nURIntensityLeft = int.Parse(parametervalue);
                    if (parametername == UIData.name_nURSpectralBinningTop) UIData.nURSpectralBinningTop = int.Parse(parametervalue);
                    if (parametername == UIData.name_nURSpectralBinningLeft) UIData.nURSpectralBinningLeft = int.Parse(parametervalue);
                    #endregion
                    #region LL
                    if (parametername == UIData.name_strLLConfigurationFilename) UIData.strLLConfigurationFilename = parametervalue;
                    if (parametername == UIData.name_strLLAlazarBoard) UIData.strLLAlazarBoard = parametervalue;
                    if (parametername == UIData.name_bLLAlazarCh1) UIData.bLLAlazarCh1 = bool.Parse(parametervalue);
                    if (parametername == UIData.name_bLLAlazarCh2) UIData.bLLAlazarCh2 = bool.Parse(parametervalue);
                    if (parametername == UIData.name_nLLAlazarLineLength) UIData.nLLAlazarLineLength = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLAlazarSamplingRate) UIData.nLLAlazarSamplingRate = int.Parse(parametervalue);
                    if (parametername == UIData.name_strLLDAQDevice) UIData.strLLDAQDevice = parametervalue;
                    if (parametername == UIData.name_strLLIMAQParallel) UIData.strLLIMAQParallel = parametervalue;
                    if (parametername == UIData.name_strLLIMAQPerpendicular) UIData.strLLIMAQPerpendicular = parametervalue;
                    if (parametername == UIData.name_nLLIMAQLineLength) UIData.nLLIMAQLineLength = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLIMAQRingBuffers) UIData.nLLIMAQRingBuffers = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLSystemType) UIData.nLLSystemType = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLSystemActual) UIData.nLLSystemActual = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLLineRate) UIData.nLLLineRate = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLLinesPerChunk) UIData.nLLLinesPerChunk = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLChunksPerImage) UIData.nLLChunksPerImage = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLImagesPerVolume) UIData.nLLImagesPerVolume = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLLinkedListLength) UIData.nLLLinkedListLength = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLCUDADevice) UIData.nLLCUDADevice = int.Parse(parametervalue);
                    if (parametername == UIData.name_strLLFileDirectory) UIData.strLLFileDirectory = parametervalue;
                    if (parametername == UIData.name_strLLFilePrefix) UIData.strLLFilePrefix = parametervalue;
                    if (parametername == UIData.name_nLLFileNumber) UIData.nLLFileNumber = int.Parse(parametervalue);
                    if (parametername == UIData.name_bLLFileRecord) UIData.bLLFileRecord = bool.Parse(parametervalue);
                    if (parametername == UIData.name_nLLFileCycle) UIData.nLLFileCycle = int.Parse(parametervalue);
                    if (parametername == UIData.name_dLLCenterX) UIData.dLLCenterX = double.Parse(parametervalue);
                    if (parametername == UIData.name_dLLCenterY) UIData.dLLCenterY = double.Parse(parametervalue);
                    if (parametername == UIData.name_dLLFastAngle) UIData.dLLFastAngle = double.Parse(parametervalue);
                    if (parametername == UIData.name_dLLRangeFast) UIData.dLLRangeFast = double.Parse(parametervalue);
                    if (parametername == UIData.name_dLLRangeSlow) UIData.dLLRangeSlow = double.Parse(parametervalue);
                    if (parametername == UIData.name_nLLDwellFast) UIData.nLLDwellFast = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLDwellSlow) UIData.nLLDwellSlow = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLRoundingFast) UIData.nLLRoundingFast = int.Parse(parametervalue);
                    if (parametername == UIData.name_nLLRoundingSlow) UIData.nLLRoundingSlow = int.Parse(parametervalue);
                    #endregion

                }   // if (n1
                line = sr.ReadLine();
            }   // while (line
            sr.Close();
        }   // void LoadParameterFile

        void SaveParameterFile(string strFile)
        {

            StreamWriter sw = new StreamWriter(strFile);

            #region template
            sw.WriteLine(UIData.name_strXXX + "=`" + UIData.strXXX + "'");
            sw.WriteLine(UIData.name_nXXX + "=`" + UIData.nXXX + "'");
            sw.WriteLine(UIData.name_bXXX + "=`" + UIData.bXXX + "'");
            sw.WriteLine(UIData.name_dXXX + "=`" + UIData.dXXX + "'");
            #endregion
            #region UL
            sw.WriteLine(UIData.name_nULDisplayIndex + "=`" + UIData.nULDisplayIndex + "'");
            sw.WriteLine(UIData.name_nULAlazarReferenceIndex + "=`" + UIData.nULAlazarReferenceIndex + "'");
            sw.WriteLine(UIData.name_nULAlazarTop + "=`" + UIData.nULAlazarTop + "'");
            sw.WriteLine(UIData.name_nULAlazarLeft + "=`" + UIData.nULAlazarLeft + "'");
            sw.WriteLine(UIData.name_dULAlazarMax + "=`" + UIData.dULAlazarMax + "'");
            sw.WriteLine(UIData.name_dULAlazarMin + "=`" + UIData.dULAlazarMin + "'");
            sw.WriteLine(UIData.name_nULDAQReferenceIndex + "=`" + UIData.nULDAQReferenceIndex + "'");
            sw.WriteLine(UIData.name_nULDAQTop + "=`" + UIData.nULDAQTop + "'");
            sw.WriteLine(UIData.name_nULDAQLeft + "=`" + UIData.nULDAQLeft + "'");
            sw.WriteLine(UIData.name_dULDAQMax + "=`" + UIData.dULDAQMax + "'");
            sw.WriteLine(UIData.name_dULDAQMin + "=`" + UIData.dULDAQMin + "'");
            sw.WriteLine(UIData.name_nULIMAQReferenceIndex + "=`" + UIData.nULIMAQReferenceIndex + "'");
            sw.WriteLine(UIData.name_nULIMAQTop + "=`" + UIData.nULIMAQTop + "'");
            sw.WriteLine(UIData.name_nULIMAQLeft + "=`" + UIData.nULIMAQLeft + "'");
            sw.WriteLine(UIData.name_nULIMAQCameraIndex + "=`" + UIData.nULIMAQCameraIndex + "'");
            sw.WriteLine(UIData.name_dULIMAQMax + "=`" + UIData.dULIMAQMax + "'");
            sw.WriteLine(UIData.name_dULIMAQMin + "=`" + UIData.dULIMAQMin + "'");
            sw.WriteLine(UIData.name_nULIntensityTop + "=`" + UIData.nULIntensityTop + "'");
            sw.WriteLine(UIData.name_nULIntensityLeft + "=`" + UIData.nULIntensityLeft + "'");
            sw.WriteLine(UIData.name_dULIntensityMax + "=`" + UIData.dULIntensityMax + "'");
            sw.WriteLine(UIData.name_dULIntensityMin + "=`" + UIData.dULIntensityMin + "'");
            #endregion
            #region UR
            sw.WriteLine(UIData.name_nURDisplayIndex + "=`" + UIData.nURDisplayIndex + "'");
            sw.WriteLine(UIData.name_nURIntensityTop + "=`" + UIData.nURIntensityTop + "'");
            sw.WriteLine(UIData.name_nURIntensityLeft + "=`" + UIData.nURIntensityLeft + "'");
            sw.WriteLine(UIData.name_nURSpectralBinningTop + "=`" + UIData.nURSpectralBinningTop + "'");
            sw.WriteLine(UIData.name_nURSpectralBinningLeft + "=`" + UIData.nURSpectralBinningLeft + "'");
            #endregion
            #region LL
            sw.WriteLine(UIData.name_strLLConfigurationFilename + "=`" + UIData.strLLConfigurationFilename + "'");
            sw.WriteLine(UIData.name_strLLAlazarBoard + "=`" + UIData.strLLAlazarBoard + "'");
            sw.WriteLine(UIData.name_bLLAlazarCh1 + "=`" + UIData.bLLAlazarCh1 + "'");
            sw.WriteLine(UIData.name_bLLAlazarCh2 + "=`" + UIData.bLLAlazarCh2 + "'");
            sw.WriteLine(UIData.name_nLLAlazarLineLength + "=`" + UIData.nLLAlazarLineLength + "'");
            sw.WriteLine(UIData.name_nLLAlazarSamplingRate + "=`" + UIData.nLLAlazarSamplingRate + "'");
            sw.WriteLine(UIData.name_strLLDAQDevice + "=`" + UIData.strLLDAQDevice + "'");
            sw.WriteLine(UIData.name_strLLIMAQParallel + "=`" + UIData.strLLIMAQParallel + "'");
            sw.WriteLine(UIData.name_strLLIMAQPerpendicular + "=`" + UIData.strLLIMAQPerpendicular + "'");
            sw.WriteLine(UIData.name_nLLIMAQLineLength + "=`" + UIData.nLLIMAQLineLength + "'");
            sw.WriteLine(UIData.name_nLLIMAQRingBuffers + "=`" + UIData.nLLIMAQRingBuffers + "'");
            sw.WriteLine(UIData.name_nLLSystemType + "=`" + UIData.nLLSystemType + "'");
            sw.WriteLine(UIData.name_nLLSystemActual + "=`" + UIData.nLLSystemActual + "'");
            sw.WriteLine(UIData.name_nLLLineRate + "=`" + UIData.nLLLineRate + "'");
            sw.WriteLine(UIData.name_nLLLinesPerChunk + "=`" + UIData.nLLLinesPerChunk + "'");
            sw.WriteLine(UIData.name_nLLChunksPerImage + "=`" + UIData.nLLChunksPerImage + "'");
            sw.WriteLine(UIData.name_nLLImagesPerVolume + "=`" + UIData.nLLImagesPerVolume + "'");
            sw.WriteLine(UIData.name_nLLLinkedListLength + "=`" + UIData.nLLLinkedListLength + "'");
            sw.WriteLine(UIData.name_nLLCUDADevice + "=`" + UIData.nLLCUDADevice + "'");
            sw.WriteLine(UIData.name_strLLFileDirectory + "=`" + UIData.strLLFileDirectory + "'");
            sw.WriteLine(UIData.name_strLLFilePrefix + "=`" + UIData.strLLFilePrefix + "'");
            sw.WriteLine(UIData.name_nLLFileNumber + "=`" + UIData.nLLFileNumber + "'");
            sw.WriteLine(UIData.name_bLLFileRecord + "=`" + UIData.bLLFileRecord + "'");
            sw.WriteLine(UIData.name_nLLFileCycle + "=`" + UIData.nLLFileCycle + "'");
            sw.WriteLine(UIData.name_dLLCenterX + "=`" + UIData.dLLCenterX + "'");
            sw.WriteLine(UIData.name_dLLCenterY + "=`" + UIData.dLLCenterY + "'");
            sw.WriteLine(UIData.name_dLLFastAngle + "=`" + UIData.dLLFastAngle + "'");
            sw.WriteLine(UIData.name_dLLRangeFast + "=`" + UIData.dLLRangeFast + "'");
            sw.WriteLine(UIData.name_dLLRangeSlow + "=`" + UIData.dLLRangeSlow + "'");
            sw.WriteLine(UIData.name_nLLDwellFast + "=`" + UIData.nLLDwellFast + "'");
            sw.WriteLine(UIData.name_nLLDwellSlow + "=`" + UIData.nLLDwellSlow + "'");
            sw.WriteLine(UIData.name_nLLRoundingFast + "=`" + UIData.nLLRoundingFast + "'");
            sw.WriteLine(UIData.name_nLLRoundingSlow + "=`" + UIData.nLLRoundingSlow + "'");
            #endregion

            sw.Close();
        }   // void SaveParameterFile

        void UIUpdate(object sender, EventArgs e)
        {
            // UL display
            graphULLeft.Refresh();
            graphULTop.Refresh();
            graphULMain.Refresh();

            // UR display
            graphURLeft.Refresh();
            graphURTop.Refresh();
            graphURMain.Refresh();

            // LL display
            UIData.nLLFileNumber = threadData.nFileNumber;
            UIData.nLLFileCycle = threadData.nFramePosition;

            // LR display
            // diagnostic tab
            // determine amount of available physical memory
            ComputerInfo CI = new ComputerInfo();
            UIData.dLRAvailableMemory = Convert.ToDouble(CI.AvailablePhysicalMemory) / 1048576.0 / 1024.0;

            UIData.nLRLinkedListLength = nodeList.Count();
            if (UIData.nLRLinkedListLength > 1)
            {
                threadData.bRecord = UIData.bLLFileRecord;
                Array.Clear(UIData.pnLinkedList, 0, UIData.pnLinkedList.Length);
                LinkedListNode<CDataNode> nodeTemp = nodeList.First;
                for (int nNode = 0; nNode < nodeList.Count; nNode++)
                {
                    if (nodeTemp.Value.rwls.TryEnterReadLock(0) == true)
                    {
                        UIData.pnLinkedList[nNode, 5] = nodeTemp.Value.nSaved;
                        UIData.pnLinkedList[nNode, 4] = nodeTemp.Value.nAcquired;
                        UIData.pnLinkedList[nNode, 3] = nodeTemp.Value.nProcessed;
                        nodeTemp.Value.rwls.ExitReadLock();
                    }
                    else
                    {  // if (nodeTemp.Value.rwls.TryEnterReadLock
                        UIData.pnLinkedList[nNode, 5] = -1;
                        UIData.pnLinkedList[nNode, 4] = -1;
                        UIData.pnLinkedList[nNode, 3] = -1;
                    }  // if (nodeTemp.Value.rwls.TryEnterReadLock
                    nodeTemp = nodeTemp.Next;
                }  // for (int nNode
                if (threadData.nProcess1Node > -1)
                    UIData.pnLinkedList[threadData.nProcess1Node, 2] = 1;
                if (threadData.nProcess2Node > -1)
                    UIData.pnLinkedList[threadData.nProcess2Node, 1] = 1;
            }   // if (UIData.nLRLinkedListLength
            graphLRDiagnostics.Refresh();

            // update thread status lines
            UIData.strLRMainThreadStatus = threadData.strMainThreadStatus;
            UIData.strLROutputThreadStatus = threadData.strOutputThreadStatus;
            UIData.strLRAcquireThreadStatus = threadData.strAcquireThreadStatus + " (A:" + threadData.strAcquireAlazarThreadStatus + "; D:" + threadData.strAcquireDAQThreadStatus + "; I:" + threadData.strAcquireIMAQThreadStatus + ")";
            UIData.strLRSaveThreadStatus = threadData.strSaveThreadStatus;
            UIData.strLRProcessThreadStatus = threadData.strProcessThreadStatus;
            UIData.strLRProcess1ThreadStatus = threadData.strProcess1ThreadStatus;
            UIData.strLRProcess2ThreadStatus = threadData.strProcess2ThreadStatus;
            UIData.strLRCleanupThreadStatus = threadData.strCleanupThreadStatus;

        }   // void UIUpdate

        private void btnTestCUDA_Click(object sender, RoutedEventArgs e)
        {
            int nA = -10;
//            nA = nOCTcudaWrapper.initialize(UIData.nLLCUDADevice);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < 10000; i++)
                nA = nOCTcudaWrapper.calibrateMZI();
            stopwatch.Stop();
            long ts = stopwatch.ElapsedMilliseconds;
            //            UIData.strTestCUDA2 = "" + 1e-4 * ts;
        }   // private void btnTestCUDA_Click

        private void btnTestIPP_Click(object sender, RoutedEventArgs e)
        {
        }   // private void btnTestIPP_Click

        private void DAQTest_Click(object sender, RoutedEventArgs e)
        {
            Task taskDigitalOutput = new Task();
            taskDigitalOutput.DOChannels.CreateChannel("/Dev1/port0/line0", "digital0", ChannelLineGrouping.OneChannelForEachLine);
            taskDigitalOutput.DOChannels.CreateChannel("/Dev1/port0/line1", "digital1", ChannelLineGrouping.OneChannelForEachLine);
            taskDigitalOutput.DOChannels.CreateChannel("/Dev1/port0/line2", "digital2", ChannelLineGrouping.OneChannelForEachLine);
            taskDigitalOutput.Timing.ConfigureSampleClock("/Dev1/pfi7", 100000, SampleClockActiveEdge.Falling, SampleQuantityMode.ContinuousSamples);
            taskDigitalOutput.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger("/Dev1/pfi0", DigitalEdgeStartTriggerEdge.Falling);

            taskDigitalOutput.Control(TaskAction.Verify);
            DigitalMultiChannelWriter digitalWriter = new DigitalMultiChannelWriter(taskDigitalOutput.Stream);


            Task taskDAQOutput = new Task();
            taskDAQOutput.AOChannels.CreateVoltageChannel("/Dev1/ao0", "analog0", -5.0, 5.0, AOVoltageUnits.Volts);
            taskDAQOutput.AOChannels.CreateVoltageChannel("/Dev1/ao1", "analog1", -5.0, 5.0, AOVoltageUnits.Volts);
            taskDAQOutput.AOChannels.CreateVoltageChannel("/Dev1/ao2", "analog2", -5.0, 5.0, AOVoltageUnits.Volts);
            taskDAQOutput.AOChannels.CreateVoltageChannel("/Dev1/ao3", "analog3", -5.0, 5.0, AOVoltageUnits.Volts);
            taskDAQOutput.Timing.ConfigureSampleClock("/Dev1/pfi7", 100000, SampleClockActiveEdge.Falling, SampleQuantityMode.ContinuousSamples);
            taskDAQOutput.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger("/Dev1/pfi0", DigitalEdgeStartTriggerEdge.Falling);

            taskDAQOutput.Control(TaskAction.Verify);
            AnalogMultiChannelWriter analogWriter = new AnalogMultiChannelWriter(taskDAQOutput.Stream);

        }

        private void btnDAQTest_Click(object sender, RoutedEventArgs e)
        {

            Task taskInput = new Task();

        }

        private void btnLLConfigurationLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                UIData.strLLConfigurationFilename = openFileDialog.FileName;
                LoadParameterFile(openFileDialog.FileName);
                StreamWriter lastParameter = new StreamWriter("lastparameterfilename.txt");
                lastParameter.WriteLine(openFileDialog.FileName);
                lastParameter.Close();
            }   // if (openFileDialog.ShowDialog
        }

        private void btnLLConfigurationSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                UIData.strLLConfigurationFilename = saveFileDialog.FileName;
                SaveParameterFile(saveFileDialog.FileName);
                StreamWriter lastParameter = new StreamWriter("lastparameterfilename.txt");
                lastParameter.WriteLine(saveFileDialog.FileName);
                lastParameter.Close();
            }   // if (saveFileDialog.ShowDialog
        }

        private void btnLLFileDirectoryBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "file directory";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                UIData.strLLFileDirectory = fbd.SelectedPath;
            }
        }

        private void btnLLGalvoUpdate_Click(object sender, RoutedEventArgs e)
        {
            threadData.mreOutputUpdate.Set();
        }

        private void btnLRConfigurationStart_Click(object sender, RoutedEventArgs e)
        {
            #region structure sizes

            switch (UIData.nLLSystemType)
            {
                case 0: // SD-OCT
                    threadData.nRawNumberAlines = UIData.nLLChunksPerImage * UIData.nLLLinesPerChunk;
                    threadData.nRawAlineLength = UIData.nLLIMAQLineLength;
                    threadData.pnProcess1DAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess1IMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ADAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2AIMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ComplexRealParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.nProcessedNumberAlines = threadData.nRawNumberAlines;
                    threadData.nProcessedAlineLength = threadData.nRawAlineLength / 2;

                    break;
                case 1: // PS SD-OCT
                    threadData.nRawNumberAlines = UIData.nLLChunksPerImage * UIData.nLLLinesPerChunk;  // total number of even and odd for each camera (parallel and perpendicular)
                    threadData.nRawAlineLength = UIData.nLLIMAQLineLength;
                    threadData.pnProcess1DAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess1IMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess1IMAQPerpendicular = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ADAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2AIMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2AIMAQPerpendicular = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ComplexRealParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexRealPerpendicular = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagPerpendicular = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.nProcessedNumberAlines = threadData.nRawNumberAlines / 2;  // by combining even and odds
                    threadData.nProcessedAlineLength = threadData.nRawAlineLength / 2;
                    break;
                case 2: // line field
                    threadData.nRawNumberAlines = UIData.nLLIMAQLineLength;
                    threadData.nRawAlineLength = UIData.nLLAlazarLineLength;  // using the alazar length even though acquisition will be on DAQ
                    threadData.pnProcess1DAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess1IMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ADAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2AIMAQParallel = new Int16[threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ComplexRealParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.nProcessedNumberAlines = threadData.nRawNumberAlines;
                    threadData.nProcessedAlineLength = threadData.nRawAlineLength / 2;
                    break;
                case 3: // OFDI
                    threadData.nRawNumberAlines = UIData.nLLChunksPerImage * UIData.nLLLinesPerChunk;  // will be two channels, each with this number of lines
                    threadData.nRawAlineLength = UIData.nLLAlazarLineLength;
                    threadData.pnProcess1Alazar = new UInt16[2 * threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess1DAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2AAlazar = new UInt16[2 * threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ADAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2ComplexRealParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.nProcessedNumberAlines = threadData.nRawNumberAlines;
                    threadData.nProcessedAlineLength = threadData.nRawAlineLength / 2;
                    break;
                case 4: // PS OFDI
                    threadData.nRawNumberAlines = UIData.nLLChunksPerImage * UIData.nLLLinesPerChunk;  // will be two channels, each with this number of lines.  even and odd simultaneously
                    threadData.nRawAlineLength = UIData.nLLAlazarLineLength;
                    threadData.pnProcess1Alazar = new UInt16[2 * threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess1DAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2AAlazar = new UInt16[2 * threadData.nRawNumberAlines * threadData.nRawAlineLength];
                    threadData.pnProcess2ADAQ = new double[4 * threadData.nRawNumberAlines];
                    threadData.pnProcess2ComplexRealParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagParallel = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexRealPerpendicular = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.pnProcess2ComplexImagPerpendicular = new double[threadData.nProcessedNumberAlines * threadData.nProcessedAlineLength];
                    threadData.nProcessedNumberAlines = threadData.nRawNumberAlines;  // no need to reduce number of lines
                    threadData.nProcessedAlineLength = threadData.nRawAlineLength / 2;
                    break;
            }   // switch (UIData.nLLSystemType

            #endregion

            #region set up linked list

            CDataNode datanode;

            // clear linked list
            while (nodeList.Count > 0)
            {
                datanode = nodeList.Last();
                nodeList.RemoveLast();
                datanode = null;
            }   // while (nodeList.Count

            // create first node
            datanode = new CDataNode(UIData, 0);
            nodeList.AddLast(datanode);
            ulong nNodeSize = datanode.nSize;

            // calculate number of nodes to create
            for (int nID = 1; nID < UIData.nLLLinkedListLength; nID++)
            {
                datanode = new CDataNode(UIData, nID);
                nodeList.AddLast(datanode);
            }   // for (int nID

            // update UI structures
            UIData.pnLinkedList = new int[nodeList.Count, 8];
            Array.Clear(UIData.pnLinkedList, 0, UIData.pnLinkedList.Length);
            graphLRDiagnostics.DataSource = UIData.pnLinkedList;

            #endregion

            #region set up threads

            threadData.Initialize();
            threadData.threadMain = new Thread(MainThread);
            threadData.threadMain.Priority = ThreadPriority.AboveNormal;
            threadData.threadMain.Start();
            threadData.mreMainReady.WaitOne();

            #endregion

            #region set up graphs

            UIData.pdULLeft = new double[20, threadData.nRawAlineLength];
            UIData.pdULTop = new double[20, threadData.nRawNumberAlines];
            UIData.pnULImage = new Int16[threadData.nRawNumberAlines, threadData.nRawAlineLength];

            graphULLeft.DataSource = UIData.pdULLeft;
            axisULLeftHorizontal.Range = new Range<int>(0, threadData.nRawAlineLength - 1);
            axisULLeftVertical.Range = new Range<double>(0.0, 16438.0);
            graphULTop.DataSource = UIData.pdULTop;
            axisULTopHorizontal.Range = new Range<int>(0, threadData.nRawNumberAlines - 1);
            axisULTopVertical.Range = new Range<double>(0.0, 16384.0);
            graphULMain.DataSource = UIData.pnULImage;

            UIData.pdURLeft = new double[10, threadData.nProcessedAlineLength];
            UIData.pdURTop = new double[10, threadData.nProcessedNumberAlines];
            UIData.pdURImage = new double[threadData.nProcessedNumberAlines, threadData.nProcessedAlineLength];

            graphURLeft.DataSource = UIData.pdURLeft;
            axisURLeftHorizontal.Range = new Range<int>(0, threadData.nProcessedAlineLength - 1);
            axisURLeftVertical.Range = new Range<double>(0.0, 16384.0);
            graphURTop.DataSource = UIData.pdURTop;
            axisURTopHorizontal.Range = new Range<int>(0, threadData.nProcessedNumberAlines - 1);
            axisURTopVertical.Range = new Range<double>(0.0, 16384.0);
            graphURMain.DataSource = UIData.pdURImage;

            #endregion

            #region set up dlls
            // int ndllCUDA = nOCTcudaWrapper.initialize(UIData.nLLSystemType, threadData.nRawNumberAlines, threadData.nRawAlineLength, 256, threadData.nProcessedNumberAlines, threadData.nProcessedAlineLength);

            #endregion
        }

        private void btnLRConfigurationStop_Click(object sender, RoutedEventArgs e)
        {
            #region linked list
            CDataNode datanode;
            // clear linked list
            while (nodeList.Count > 0)
            {
                datanode = nodeList.Last();
                nodeList.RemoveLast();
                datanode = null;
            }   // while (nodeList.Count

            UIData.pnLinkedList = new int[2, 2];
            Array.Clear(UIData.pnLinkedList, 0, UIData.pnLinkedList.Length);
            graphLRDiagnostics.DataSource = UIData.pnLinkedList;

            GC.Collect();
            #endregion

            #region threads

            threadData.mreMainKill.Set();
            threadData.mreMainDead.WaitOne();

            #endregion

            #region dlls

            // int ndllCUDA = nOCTcudaWrapper.cleanup();
            #endregion
        }

        private void btnLROperationStart_Click(object sender, RoutedEventArgs e)
        {
            #region threads
            threadData.mreMainRun.Set();
            #endregion
        }

        private void btnLROperationStop_Click(object sender, RoutedEventArgs e)
        {
            threadData.mreMainKill.Set();
            threadData.mreMainDead.WaitOne();

        }   // private void btnLROperationStop_Click

        private void btnLRGCCollect_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }   // private void btnLRGCCollect_Click

        void MainThread()
        {
            #region initializing
            threadData.strMainThreadStatus = "Initializing...";

            // start other threads
            threadData.threadOutput = new Thread(OutputThread);
            threadData.threadOutput.Priority = ThreadPriority.Normal;
            threadData.threadOutput.Start();

            threadData.threadAcquire = new Thread(AcquireThread);
            threadData.threadAcquire.Priority = ThreadPriority.Normal;
            threadData.threadAcquire.Start();

            threadData.threadSave = new Thread(SaveThread);
            threadData.threadSave.Priority = ThreadPriority.Normal;
            threadData.threadSave.Start();

            threadData.threadProcess = new Thread(ProcessThread);
            threadData.threadProcess.Priority = ThreadPriority.Normal;
            threadData.threadProcess.Start();

            threadData.threadProcess1 = new Thread(Process1Thread);
            threadData.threadProcess1.Priority = ThreadPriority.Normal;
            threadData.threadProcess1.Start();

            threadData.threadProcess2 = new Thread(Process2Thread);
            threadData.threadProcess2.Priority = ThreadPriority.Normal;
            threadData.threadProcess2.Start();

            threadData.threadCleanup = new Thread(CleanupThread);
            threadData.threadCleanup.Priority = ThreadPriority.Normal;
            threadData.threadCleanup.Start();

            // wait for threads
            threadData.mreOutputReady.WaitOne();
            threadData.mreAcquireReady.WaitOne();
            threadData.mreSaveReady.WaitOne();
            threadData.mreProcessReady.WaitOne();
            threadData.mreProcess1Ready.WaitOne();
            threadData.mreProcess2Ready.WaitOne();
            threadData.mreCleanupReady.WaitOne();

            // set up wait handles for starting
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreMainKill;
            pweStart[1] = threadData.mreMainRun;
            // set up wait handles for main loop
            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreMainKill;
            pweLoop[1] = threadData.ssAcquireComplete.AvailableWaitHandle;

            // initialization complete
            threadData.mreMainReady.Set();
            threadData.strMainThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strMainThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.mreOutputRun.Set();
                threadData.mreAcquireRun.Set();
                threadData.mreSaveRun.Set();
                threadData.mreProcessRun.Set();
                threadData.mreProcess1Run.Set();
                threadData.mreProcess2Run.Set();
                threadData.mreCleanupRun.Set();

                threadData.strMainThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) != 0)
                {
                    threadData.strMainThreadStatus = "waiting";
                    threadData.ssAcquireComplete.Wait();
                    threadData.strMainThreadStatus = "updating";
                    threadData.mreCleanupAction.Set();
                    Thread.Sleep(1);
                }

            }
            #endregion

            #region cleanup
            threadData.strMainThreadStatus = "Cleaning up...";

            // send kill command to other threads
            threadData.mreAcquireKill.Set();
            threadData.mreAcquireDead.WaitOne();

            threadData.mreSaveKill.Set();
            threadData.mreSaveDead.WaitOne();

            threadData.mreProcessKill.Set();
            threadData.mreProcessDead.WaitOne();

            threadData.mreProcess1Kill.Set();
            threadData.mreProcess1Dead.WaitOne();

            threadData.mreProcess2Kill.Set();
            threadData.mreProcess2Dead.WaitOne();

            threadData.mreCleanupAction.Set();
            threadData.mreCleanupKill.Set();
            threadData.mreCleanupDead.WaitOne();

            // wait for threads to end
            threadData.mreOutputKill.Set();
            threadData.mreOutputDead.WaitOne();

            // all done
            threadData.mreMainDead.Set();
            threadData.strMainThreadStatus = "Done.";
            #endregion

        }

        void OutputThread()
        {
            #region initializing
            threadData.strOutputThreadStatus = "Initializing...";


            // initialization
            double dLineTriggerRate = UIData.nLLLineRate;
            int nNumberLines = 2048;
            int nNumberFrames = 512;

            // counter task
            Task taskCtr = new Task();
            taskCtr.COChannels.CreatePulseChannelFrequency("Dev1/ctr0", "ctrClock", COPulseFrequencyUnits.Hertz, COPulseIdleState.Low, 0.0, 2 * dLineTriggerRate, 0.5);
            taskCtr.Timing.ConfigureImplicit(SampleQuantityMode.ContinuousSamples, 1000);

            // digital task
            Task taskDig = new Task();
            DigitalMultiChannelWriter digWriter = new DigitalMultiChannelWriter(taskDig.Stream);
            taskDig.DOChannels.CreateChannel("Dev1/port0/line0", "digLineTrigger", ChannelLineGrouping.OneChannelForEachLine);
            taskDig.DOChannels.CreateChannel("Dev1/port0/line1", "digFrameTrigger", ChannelLineGrouping.OneChannelForEachLine);
            taskDig.DOChannels.CreateChannel("Dev1/port0/line2", "digVolumeTrigger", ChannelLineGrouping.OneChannelForEachLine);
            taskDig.Timing.ConfigureSampleClock("/Dev1/Ctr0InternalOutput", 2 * dLineTriggerRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);
            taskDig.Control(TaskAction.Verify);

            DigitalWaveform[] digWFM;
            digWFM = new DigitalWaveform[3];
            // line trigger
            int i = 0, j, k;
            digWFM[i] = new DigitalWaveform(nNumberFrames * 2 * nNumberLines, 1);
            for (j = 0; j < nNumberFrames; j++)
            {
                for (k = 0; k < 2 * nNumberLines; k += 2)
                {
                    digWFM[i].Signals[0].States[j * 2 * nNumberLines + k] = DigitalState.ForceDown; // j * 2 * nNumberLines
                    digWFM[i].Signals[0].States[j * 2 * nNumberLines + k + 1] = DigitalState.ForceUp;
                }
            }
            // frame trigger
            i = 1;
            digWFM[i] = new DigitalWaveform(nNumberFrames * 2 * nNumberLines, 1);
            for (j = 0; j < nNumberFrames; j++)
            {
                k = 0;
                digWFM[i].Signals[0].States[j * 2 * nNumberLines + k] = DigitalState.ForceUp;
                for (k = 1; k < 2 * nNumberLines; k++)
                    digWFM[i].Signals[0].States[j * 2 * nNumberLines + k] = DigitalState.ForceDown;
            }

            // volume trigger
            i = 2;
            digWFM[i] = new DigitalWaveform(nNumberFrames * 2 * nNumberLines, 1);
            for (j = 0; j < nNumberFrames; j++)
                for (k = 1; k < 2 * nNumberLines; k++)
                    digWFM[i].Signals[0].States[j * 2 * nNumberLines + k] = DigitalState.ForceDown;
            digWFM[i].Signals[0].States[0] = DigitalState.ForceUp;
            // write waveform
            digWriter.WriteWaveform(false, digWFM);


            // analog waveform
            Task taskAna = new Task();
            AnalogMultiChannelWriter anaWriter = new AnalogMultiChannelWriter(taskAna.Stream);
            taskAna.AOChannels.CreateVoltageChannel("Dev1/ao0", "anaGalvoFast", -5.0, +5.0, AOVoltageUnits.Volts);
            taskAna.AOChannels.CreateVoltageChannel("Dev1/ao1", "anaGalvoSlow", -5.0, +5.0, AOVoltageUnits.Volts);
            taskAna.AOChannels.CreateVoltageChannel("Dev1/ao2", "anaPolMod", -5.0, +5.0, AOVoltageUnits.Volts);
            taskAna.Timing.ConfigureSampleClock("/Dev1/PFI7", dLineTriggerRate, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples);


            double[,] anaWFM = new double[3, nNumberFrames * nNumberLines];
            // fast galvo
            i = 0;
            for (j = 0; j < nNumberFrames; j++)
            {
                for (k = 0; k < nNumberLines; k++)
                {
                    anaWFM[i, j * nNumberLines + k] = (1.0 * k) / (0.5 * nNumberLines) * 2.0;
                }
            }
            // slow galvo
            i = 1;
            for (j = 0; j < nNumberFrames; j++)
            {
                for (k = 0; k < nNumberLines; k++)
                {
                    anaWFM[i, j * nNumberLines + k] = (1.0 * j) / (0.5 * nNumberLines) * 2.0;
                }
            }
            // pol mod
            i = 2;
            for (j = 0; j < nNumberFrames; j++)
            {
                for (k = 0; k < nNumberLines; k += 2)
                {
                    anaWFM[i, j * nNumberLines + k] = -1.0;
                    anaWFM[i, j * nNumberLines + k + 1] = 1.0;
                }
            }
            anaWriter.WriteMultiSample(false, anaWFM);


            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreOutputKill;
            pweStart[1] = threadData.mreOutputRun;
            // set up wait handles for main loop
            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreOutputKill;
            pweLoop[1] = threadData.mreOutputUpdate;

            // initialization complete
            threadData.mreOutputReady.Set();
            threadData.strOutputThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strOutputThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strOutputThreadStatus = "GO!";
                // start tasks
                taskCtr.Start();
                taskDig.Control(TaskAction.Start);
                taskAna.Control(TaskAction.Start);

                while (WaitHandle.WaitAny(pweLoop) == 1)
                {
                    threadData.mreOutputUpdate.Reset();
                    threadData.strOutputThreadStatus = "updating...";


                    // fast galvo
                    i = 0;
                    for (j = 0; j < nNumberFrames; j++)
                    {
                        for (k = 0; k < nNumberLines; k++)
                        {
                            anaWFM[i, j * nNumberLines + k] = (1.0 * k) / (0.5 * nNumberLines) * 2.0;
                        }
                    }
                    // slow galvo
                    i = 1;
                    for (j = 0; j < nNumberFrames; j++)
                    {
                        for (k = 0; k < nNumberLines; k++)
                        {
                            anaWFM[i, j * nNumberLines + k] = (1.0 * j) / (0.5 * nNumberLines) * 2.0;
                        }
                    }
                    // pol mod
                    i = 2;
                    for (j = 0; j < nNumberFrames; j++)
                    {
                        for (k = 0; k < nNumberLines; k += 2)
                        {
                            anaWFM[i, j * nNumberLines + k] = -1.0;
                            anaWFM[i, j * nNumberLines + k + 1] = 1.0;
                        }
                    }
                    anaWriter.BeginWriteMultiSample(false, anaWFM, null, null);

                    threadData.strOutputThreadStatus = "idle...";
                }
            }
            #endregion

            #region cleanup
            threadData.strOutputThreadStatus = "Cleaning up...";

            // clean up code
            taskAna.Control(TaskAction.Stop);
            taskDig.Control(TaskAction.Stop);
            taskCtr.Stop();

            taskAna.Dispose();
            taskDig.Dispose();
            taskCtr.Dispose();


            threadData.mreOutputDead.Set();
            threadData.strOutputThreadStatus = "Done.";
            #endregion

        }

        void AcquireThread()
        {
            #region initializing
            threadData.strAcquireThreadStatus = "init";

            // initialize sub-threads
            threadData.threadAcquireAlazar = new Thread(AcquireAlazarThread);
            threadData.threadAcquireAlazar.Priority = ThreadPriority.Normal;
            threadData.threadAcquireAlazar.Start();

            threadData.threadAcquireDAQ = new Thread(AcquireDAQThread);
            threadData.threadAcquireDAQ.Priority = ThreadPriority.Normal;
            threadData.threadAcquireDAQ.Start();

            threadData.threadAcquireIMAQ = new Thread(AcquireIMAQThread);
            threadData.threadAcquireIMAQ.Priority = ThreadPriority.Normal;
            threadData.threadAcquireIMAQ.Start();

            // wait for sub-threads to be ready
            threadData.mreAcquireAlazarReady.WaitOne();
            threadData.mreAcquireDAQReady.WaitOne();
            threadData.mreAcquireIMAQReady.WaitOne();

            // initialization
            bool bTroublemaker = false;
            threadData.nodeAcquire = nodeList.First;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreAcquireKill;
            pweStart[1] = threadData.mreAcquireRun;

            // wait handles to signal running
            WaitHandle[] pweGo = new WaitHandle[3];
            pweGo[0] = threadData.areAcquireAlazarGo;
            pweGo[1] = threadData.areAcquireDAQGo;
            pweGo[2] = threadData.areAcquireIMAQGo;

            // wait handles to signal completion
            WaitHandle[] pweComplete = new WaitHandle[3];
            pweComplete[0] = threadData.areAcquireAlazarComplete;
            pweComplete[1] = threadData.areAcquireDAQComplete;
            pweComplete[2] = threadData.areAcquireIMAQComplete;

            // initialization complete
            threadData.mreAcquireReady.Set();
            threadData.strAcquireThreadStatus = "Ready!";
            #endregion

            int nFileNumber = UIData.nLLFileNumber;  // initial value = 100001 
            int nFramePosition = 1;

            #region main loop
            threadData.strAcquireThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strAcquireThreadStatus = "GO!";
                threadData.mreAcquireAlazarRun.Set();
                threadData.mreAcquireDAQRun.Set();
                threadData.mreAcquireIMAQRun.Set();

                while (threadData.mreAcquireKill.WaitOne(0) == false)
                {
                    threadData.strAcquireThreadStatus = "acq";
                    if (threadData.nodeAcquire.Value.rwls.TryEnterWriteLock(1000) == true)
                    {
                        if (threadData.nodeAcquire.Value.nAcquired > 0)
                        {
                            // something wrong!
                            threadData.strAcquireThreadStatus = "overrun";
                            threadData.nodeAcquire.Value.rwls.ExitWriteLock();
                            bTroublemaker = true;
                            threadData.mreAcquireKill.Set();
                            threadData.mreMainKill.Set();
                        }
                        else
                        {  // if (nodeAcquire.Value.nAcquired

                            // actual acquisition
                            threadData.mreAcquireNodeReady.Set();
                            threadData.nodeAcquire.Value.bRecord = threadData.bRecord;

                            /* Begin: 20201208 editing by JL */                            
                            threadData.nodeAcquire.Value.strFilename = UIData.strLLFileDirectory + "//" + UIData.strLLFilePrefix + String.Format("{0}", nFileNumber) + ".dat";
                            threadData.nodeAcquire.Value.nFramePosition = nFramePosition;
                            threadData.nodeAcquire.Value.nFileNumber = nFileNumber;
                            threadData.nFramePosition = nFramePosition;
                            threadData.nFileNumber = nFileNumber;
                            nFileNumber++;
                            
                            nFramePosition++;

                            if (nFramePosition > UIData.nLLImagesPerVolume)
                                nFramePosition = 1; 
                            /* End: 20201208 editing by JL */


                            if (WaitHandle.WaitAll(pweGo, 10000) == true)
                            {
                                threadData.mreAcquireNodeReady.Reset();
                                if (WaitHandle.WaitAll(pweComplete, 10000) == true)
                                {
                                    // acquisition complete
                                    if (threadData.nodeAcquire.Value.bRecord)
                                        threadData.nodeAcquire.Value.nAcquired = 1;
                                    else
                                        threadData.nodeAcquire.Value.nAcquired = 2;
                                    threadData.nodeAcquire.Value.rwls.ExitWriteLock();
                                    // find next node
                                    threadData.nodeAcquire = threadData.nodeAcquire.Next;
                                    if (threadData.nodeAcquire == null)
                                        threadData.nodeAcquire = nodeList.First;
                                    // signal other threads
                                    threadData.ssAcquireComplete.Release();
                                    threadData.ssSaveAction.Release();
                                    threadData.ssProcessAction.Release();
                                    threadData.strAcquireThreadStatus = "done";
                                }
                                else
                                {  // if (WaitHandle.WaitAll(pweComplete
                                    // something wrong!
                                    threadData.strAcquireThreadStatus = "stuck";
                                    bTroublemaker = true;
                                    threadData.mreAcquireKill.Set();
                                    threadData.mreMainKill.Set();
                                }  // if (WaitHandle.WaitAll(pweComplete
                            }
                            else
                            {  // if (WaitHandle.WaitAll(pweGo
                                // something wrong!
                                threadData.strAcquireThreadStatus = "no go!";
                                bTroublemaker = true;
                                threadData.mreAcquireKill.Set();
                                threadData.mreMainKill.Set();
                            }  // if (WaitHandle.WaitAll(pweGo
                        }  // if (nodeAcquire.Value.nAcquired
                    }
                    else
                    {  // if (nodeAcquire.Value.rwls.TryEnterWriteLock
                        // something wrong!
                        threadData.strAcquireThreadStatus = "timeout!";
                        bTroublemaker = true;
                        threadData.mreAcquireKill.Set();
                        threadData.mreMainKill.Set();
                    }  // if (nodeAcquire.Value.rwls.TryEnterWriteLock
                }  // while (threadData.mreAcquireKill.WaitOne
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreAcquireAlazarKill.Set();
                threadData.mreAcquireDAQKill.Set();
                threadData.mreAcquireIMAQKill.Set();
                threadData.mreAcquireAlazarDead.WaitOne();
                threadData.mreAcquireDAQDead.WaitOne();
                threadData.mreAcquireIMAQDead.WaitOne();

                threadData.mreAcquireDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strAcquireThreadStatus = "Cleaning up...";
                // clean up code
                threadData.mreAcquireAlazarKill.Set();
                threadData.mreAcquireDAQKill.Set();
                threadData.mreAcquireIMAQKill.Set();
                threadData.mreAcquireAlazarDead.WaitOne();
                threadData.mreAcquireDAQDead.WaitOne();
                threadData.mreAcquireIMAQDead.WaitOne();

                // signal other threads
                threadData.mreAcquireDead.Set();
                threadData.strAcquireThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion

        }  // void AcquireThread

        void AcquireAlazarThread()
        {
            #region initializing
            threadData.strAcquireAlazarThreadStatus = "i";

            // initialization
            bool bTroublemaker = false;
            int nMode = -1;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreAcquireAlazarKill;
            pweStart[1] = threadData.mreAcquireAlazarRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreAcquireAlazarKill;
            pweLoop[1] = threadData.mreAcquireNodeReady;
            int nStatus;

            // initialization complete
            switch (UIData.nLLSystemType)
            {
                case 0: // SD-OCT
                    nMode = 0;
                    break;
                case 1: // PS SD-OCT
                    nMode = 0;
                    break;
                case 2: // line field
                    nMode = 0;
                    break;
                case 3: // OFDI
                    nMode = 1;
                    break;
                case 4: // PS OFDI
                    nMode = 2;
                    break;
            }
            threadData.nSystemActual = UIData.nLLSystemActual;

            threadData.mreAcquireAlazarReady.Set();
            threadData.strAcquireAlazarThreadStatus = "r";
            #endregion

            #region main loop
            threadData.strAcquireAlazarThreadStatus = "s";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strAcquireAlazarThreadStatus = "g";
                while (threadData.mreAcquireAlazarKill.WaitOne(0) == false)
                {
                    nStatus = WaitHandle.WaitAny(pweLoop, 10000);
                    if (nStatus == 0)
                    {
                        // kill
                    }  // if (nStatus
                    if (nStatus == 1)
                    {
                        threadData.areAcquireAlazarGo.Set();
                        threadData.strAcquireAlazarThreadStatus = "G";
                        if (nMode > 0)
                        {
                            if (threadData.nSystemActual == 0)
                            {
                                threadData.strAcquireAlazarThreadStatus = "Wa";
                                ; // real acquisition
                                Thread.Sleep(1);
                            }
                            else
                            {
                                threadData.strAcquireAlazarThreadStatus = "Wd";
                                ; // read from file
                                Thread.Sleep(1);    // pgreg002 replace the sleep command with a block of code to read in a data file
                                                    // see section (marked with pgreg002) in the IMAQ acquisition thread for reference
                            }
                        }
                        threadData.areAcquireAlazarComplete.Set();
                        threadData.strAcquireAlazarThreadStatus = "D";
                    }  // if (nStatus
                    if (nStatus == WaitHandle.WaitTimeout)
                    {
                        threadData.strAcquireAlazarThreadStatus = "!";
                        bTroublemaker = true;
                        threadData.mreAcquireAlazarKill.Set();
                        threadData.mreMainKill.Set();
                    }  // if (nStatus
                }  // while (threadData.mreAcquireAlazarKill.WaitOne
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreAcquireAlazarDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strAcquireAlazarThreadStatus = "c";
                // clean up code
                ;
                // signal other threads
                threadData.mreAcquireAlazarDead.Set();
                threadData.strAcquireAlazarThreadStatus = "d";
                #endregion
            }  // if (bTroublemaker
            #endregion
        }

        void AcquireDAQThread()
        {
            #region initializing
            threadData.strAcquireDAQThreadStatus = "i";

            // initialization
            bool bTroublemaker = false;
            int nMode = -1;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreAcquireDAQKill;
            pweStart[1] = threadData.mreAcquireDAQRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreAcquireDAQKill;
            pweLoop[1] = threadData.mreAcquireNodeReady;
            int nStatus;

            // initialization complete
            switch (UIData.nLLSystemType)
            {
                case 0: // SD-OCT
                    nMode = 1;
                    break;
                case 1: // PS SD-OCT
                    nMode = 1;
                    break;
                case 2: // line field
                    nMode = 1;
                    break;
                case 3: // OFDI
                    nMode = 1;
                    break;
                case 4: // PS OFDI
                    nMode = 1;
                    break;
            }
            threadData.mreAcquireDAQReady.Set();
            threadData.strAcquireDAQThreadStatus = "r";
            #endregion

            #region main loop
            threadData.strAcquireDAQThreadStatus = "s";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strAcquireDAQThreadStatus = "g";

                while (threadData.mreAcquireDAQKill.WaitOne(0) == false)
                {
                    nStatus = WaitHandle.WaitAny(pweLoop, 10000);
                    if (nStatus == 0)
                    {
                        ;
                    }
                    if (nStatus == 1)
                    {
                        threadData.areAcquireDAQGo.Set();
                        threadData.strAcquireDAQThreadStatus = "G";
                        if (nMode > 0)
                        {
                            if (threadData.nSystemActual == 0)
                            {
                                threadData.strAcquireDAQThreadStatus = "Wa";
                                ; // real acquisition
                                Thread.Sleep(1);
                            }
                            else
                            {
                                threadData.strAcquireDAQThreadStatus = "Wd";
                                ; // read from file
                                Thread.Sleep(1);
                            }
                        }
                        threadData.areAcquireDAQComplete.Set();
                        threadData.strAcquireDAQThreadStatus = "D";
                    }
                    if (nStatus == WaitHandle.WaitTimeout)
                    {
                        threadData.strAcquireDAQThreadStatus = "!";
                        bTroublemaker = true;
                        threadData.mreAcquireDAQKill.Set();
                        threadData.mreMainKill.Set();
                    }
                }  // while (threadData.mreAcquireDAQKill.WaitOne
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreAcquireDAQDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strAcquireDAQThreadStatus = "c";
                // clean up code
                ;
                // signal other threads
                threadData.mreAcquireDAQDead.Set();
                threadData.strAcquireDAQThreadStatus = "d";
                #endregion
            }  // if (bTroublemaker
            #endregion
        }

        void AcquireIMAQThread()
        {
            #region initializing
            threadData.strAcquireIMAQThreadStatus = "i";

            // initialization
            bool bTroublemaker = false;
            int nMode = -1;
            

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreAcquireIMAQKill;
            pweStart[1] = threadData.mreAcquireIMAQRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreAcquireIMAQKill;
            pweLoop[1] = threadData.mreAcquireNodeReady;
            int nStatus;

            // initialization complete
            switch (UIData.nLLSystemType)
            {
                case 0: // SD-OCT
                    nMode = 1;
                    break;
                case 1: // PS SD-OCT
                    nMode = 2;
                    break;
                case 2: // line field
                    nMode = 1;
                    break;
                case 3: // OFDI
                    nMode = 0;
                    break;
                case 4: // PS OFDI
                    nMode = 0;
                    break;
            }

            // InitializeIMAQ from dll
            
            string strinterfaceName0 = "img0";
            char[] pchinterfaceName0 = new char[64];
            pchinterfaceName0 = strinterfaceName0.ToCharArray();
            string strinterfaceName1 = "img1";
            char[] pchinterfaceName1 = new char[64];
            pchinterfaceName1 = strinterfaceName1.ToCharArray();
            int errInfo = 0;

            errInfo = nOCTimaqWrapper.InitializeImaq(pchinterfaceName0, pchinterfaceName1, UIData.nLLIMAQLineLength, UIData.nLLLinesPerChunk, errInfo);

            // since the initialization of imaq sometimes fails, there is while loop to make the initialiazation successed 
            while (errInfo < 0)
            {
                nOCTimaqWrapper.StopAcquisition();
                errInfo = 0;
                errInfo = nOCTimaqWrapper.InitializeImaq(pchinterfaceName0, pchinterfaceName1, UIData.nLLIMAQLineLength, UIData.nLLLinesPerChunk, errInfo);
            }
            
            threadData.mreAcquireIMAQReady.Set();
            //threadData.strAcquireIMAQThreadStatus = "r";
            
            if (errInfo < 0)
            {
                threadData.strAcquireIMAQThreadStatus = "F"; // status F meams Imaq Inialization failed
            }
            else
            {
                threadData.strAcquireIMAQThreadStatus = "r";
            }
            
            #endregion

            #region main loop
            //threadData.strAcquireIMAQThreadStatus = "s";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strAcquireIMAQThreadStatus = "g";

                // start acquisition call to dll

                nOCTimaqWrapper.StartAcquisition();

                while (threadData.mreAcquireIMAQKill.WaitOne(0) == false)
                {
                    nStatus = WaitHandle.WaitAny(pweLoop, 10000);
                    if (nStatus == 0)
                    {
                        ;
                    }
                    if (nStatus == 1)
                    {
                        threadData.areAcquireIMAQGo.Set();
                        threadData.strAcquireIMAQThreadStatus = "G";
                        if (nMode > 0)
                        {
                            if (threadData.nSystemActual == 0)
                            {
                                threadData.strAcquireIMAQThreadStatus = "Wa";
                                ; // real acquisition
                                //Thread.Sleep(100);

                                // call dll to copy data from ring buffer into data node
                                
                                
                                int bufferIndex0 = 0;
                                int bufferIndex1 = 0;

                                for(int nChunk=0 ; nChunk<UIData.nLLChunksPerImage; nChunk++)
                                {
                                    nOCTimaqWrapper.RealAcquisition0(bufferIndex0, threadData.nodeAcquire.Value.pnIMAQParallel[nChunk]);
                                    nOCTimaqWrapper.RealAcquisition1(bufferIndex1, threadData.nodeAcquire.Value.pnIMAQPerpendicular[nChunk]);
                                }
                                
                            }   // if (threadData.nSystemActual
                            else
                            {
                                threadData.strAcquireIMAQThreadStatus = "Wd";
                                // read from file (pgreg002 this section reads in binary data from two different files and copies them in chunks into the IMAQ arrays
                                var byteBuffer = new byte[threadData.nRawNumberAlines * threadData.nRawAlineLength * sizeof(Int16)];
                                byteBuffer = File.ReadAllBytes("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCT\\pdH.bin");
                                if (UIData.nLLChunksPerImage > 0)
                                {
                                    for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        Buffer.BlockCopy(byteBuffer, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength * sizeof(Int16), threadData.nodeAcquire.Value.pnIMAQParallel[nChunk], 0, threadData.nodeAcquire.Value.pnIMAQParallel[nChunk].Length * sizeof(Int16));
                                    threadData.strAcquireIMAQThreadStatus = "Wd";
                                }   // if (nChunk

                                if (nMode > 1)
                                {
                                    byteBuffer = File.ReadAllBytes("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCT\\pdV.bin");
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                            Buffer.BlockCopy(byteBuffer, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength * sizeof(Int16), threadData.nodeAcquire.Value.pnIMAQPerpendicular[nChunk], 0, threadData.nodeAcquire.Value.pnIMAQPerpendicular[nChunk].Length * sizeof(Int16));
                                    }   // if (nChunk
                                }   // if (nMode

                            }   // if (threadData.nSystemActual
                        }
                        threadData.areAcquireIMAQComplete.Set();
                        threadData.strAcquireIMAQThreadStatus = "D";
                    }
                    if (nStatus == WaitHandle.WaitTimeout)
                    {
                        threadData.strAcquireIMAQThreadStatus = "!";
                        bTroublemaker = true;
                        threadData.mreAcquireIMAQKill.Set();
                        threadData.mreMainKill.Set();
                    }
                }  // while (threadData.mreAcquireIMAQKill.WaitOne
            }  // if (WaitHandle.WaitAny
            #endregion


            // stopacquisition
            nOCTimaqWrapper.StopAcquisition();

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreAcquireIMAQDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strAcquireIMAQThreadStatus = "c";
                // clean up code
                ;
                // signal other threads
                threadData.mreAcquireIMAQDead.Set();
                threadData.strAcquireIMAQThreadStatus = "d";
                #endregion
            }  // if (bTroublemaker
            #endregion

            // call functon to stop cameras and clean ring buffers
        }

        void SaveThread()   // 20201208 editing 
        {
            #region initializing
            threadData.strSaveThreadStatus = "Initializing...";

            // initialization
            bool bTroublemaker = false;
            LinkedListNode<CDataNode> nodeSave;
            nodeSave = nodeList.First;

            /* Begin: 20201208 editing by JL */
            Thread.Sleep(1);
            string strTest;
            int nOffset1 = 4096;
            int nOffset2; 

            // parameters for saving 
            UInt16[][] pnAlazar;
            double[] pnDAQ;
            Int16[] pnIMAQ; 
            Int16[] pnIMAQParallel;
            Int16[] pnIMAQPerpendicular;            

            int nNumberChunks, nLinesPerChunk, nLineLength, nChannels;
            nNumberChunks = UIData.nLLChunksPerImage;
            nLinesPerChunk = UIData.nLLLinesPerChunk;
            nLineLength = UIData.nLLIMAQLineLength;
            int nNumberLines = nLinesPerChunk * nNumberChunks;

            pnDAQ = new double[4 * nNumberChunks * nLinesPerChunk];
            Array.Clear(pnDAQ, 0, pnDAQ.Length);

            // SD-OCT
            pnIMAQ = new Int16[nNumberLines * nLineLength];

            // PS-SD-OCT
            pnIMAQParallel = new Int16[nNumberLines * nLineLength];
            pnIMAQPerpendicular = new Int16[nNumberLines * nLineLength];

            switch (UIData.nLLSystemType)
            {
                case 0: // SD-OCT
                    Array.Clear(pnIMAQ, 0, pnIMAQ.Length);
                    break;
                case 1: // PS-SD-OCT
                    Array.Clear(pnIMAQParallel, 0, pnIMAQParallel.Length);
                    Array.Clear(pnIMAQPerpendicular, 0, pnIMAQParallel.Length);                    
                    break;
                case 2: // line field

                    break;
                case 3: // OFDI (pgreg002 here is a section to see how the arrays are defined in each node
                    nNumberChunks = UIData.nLLChunksPerImage;
                    nLinesPerChunk = UIData.nLLLinesPerChunk;
                    nLineLength = UIData.nLLAlazarLineLength;
                    nChannels = 0;
                    if (UIData.bLLAlazarCh1 == true)
                        nChannels++;
                    if (UIData.bLLAlazarCh2 == true)
                        nChannels++;
                    pnAlazar = new UInt16[nNumberChunks][];
                    for (int nChunk = 0; nChunk < nNumberChunks; nChunk++)
                    {
                        pnAlazar[nChunk] = new UInt16[nChannels * nLinesPerChunk * nLineLength];  // 2 - MZI + OCT
                        // nSize += Convert.ToUInt64(nChannels * nLinesPerChunk * nLineLength) * sizeof(UInt16);
                        Array.Clear(pnAlazar[nChunk], 0, pnAlazar[nChunk].Length);
                    }   // for (int nChunk
                    pnDAQ = new double[4 * nNumberChunks * nLinesPerChunk];
                    // nSize += Convert.ToUInt64(4 * nNumberChunks * nLinesPerChunk * sizeof(double));
                    Array.Clear(pnDAQ, 0, pnDAQ.Length);
                    break;
                case 4: // PS-OFDI

                    break;
            }

            /* End: 20201208 editing by JL */

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreSaveKill;
            pweStart[1] = threadData.mreSaveRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.ssSaveAction.AvailableWaitHandle;
            pweLoop[1] = threadData.mreSaveKill;

            // initialization complete
            threadData.mreSaveReady.Set();
            threadData.strSaveThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strSaveThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strSaveThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) == 0)
                {
                    threadData.strSaveThreadStatus = "waiting (" + threadData.ssSaveAction.CurrentCount + ")!";
                    threadData.ssSaveAction.Wait();
                    threadData.strSaveThreadStatus = "saving (" + threadData.ssSaveAction.CurrentCount + ")!";
                    if (nodeSave.Value.rwls.TryEnterReadLock(1000) == true)
                    {
                        if (nodeSave.Value.nAcquired > 0)
                        {
                            /* Begin: 20201208 editing by JL */
                            threadData.nSaveNodeID = nodeSave.Value.nNodeID;                            
                            if (nodeSave.Value.bRecord)
                            {
                                // actual save
                                // Thread.Sleep(600);
                                FileStream fs = File.Open(nodeSave.Value.strFilename, FileMode.Create);
                                BinaryWriter binWriter = new BinaryWriter(fs);
                                strTest = nodeSave.Value.strFilename;   binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                switch (UIData.nLLSystemType)
                                {
                                    case 0: // SD-OCT
                                        strTest = "nFrameNumber=" + nodeSave.Value.nFramePosition + ";"; binWriter.Write(strTest.Length); binWriter.Write(strTest);
                                        strTest = "nNumberDataArrays=" + 2 + ";";       binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        // header array 1: IMAQ data (parallel and perpendicular)
                                        strTest = "strVar='pdIMAQ';";                   binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nOffset=" + nOffset1 + ";";          binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nNumberLines=" + nNumberLines + ";"; binWriter.Write(strTest.Length);    binWriter.Write
                                            (strTest);
                                        strTest = "nLineLength=" + nLineLength + ";";   binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "strDataType='int16';";               binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        // header array 2: DAQ data
                                        nOffset2 = nOffset1 + nNumberLines * nLineLength * sizeof(Int16);   // two cameras
                                        strTest = "strVar='pdDAQ';";                    binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nOffset=" + nOffset2 + ";";          binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nNumberLines=" + 4 + ";";            binWriter.Write(strTest.Length);    binWriter.Write(strTest);     // Need double check
                                        strTest = "nLineLength=" + nLineLength + ";";   binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "strDataType='double';";              binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        strTest = "END"; binWriter.Write(strTest.Length); binWriter.Write(strTest);

                                        // save array 1: IMAQ data (parallel and perpendicular)
                                        fs.Seek(nOffset1, SeekOrigin.Begin);

                                        for (int nChunk = 0; nChunk < nNumberChunks; nChunk++)
                                            Array.Copy(nodeSave.Value.pnIMAQ[nChunk], 0, pnIMAQ, nChunk * nLinesPerChunk * nLineLength, nLinesPerChunk * nLineLength);                                          

                                        for (int nLine = 0; nLine < nNumberLines; nLine++)
                                            for (int nPoint = 0; nPoint < nLineLength; nPoint++)
                                                binWriter.Write(pnIMAQ[nLine * nLineLength + nPoint]);

                                        break;
                                    case 1: // PS-SD-OCT
                                        strTest = "nFrameNumber=" + nodeSave.Value.nFramePosition + ";";    binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nNumberDataArrays=" + 2 + ";";       binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        // header array 1: IMAQ data (parallel and perpendicular)
                                        strTest = "strVar='pdIMAQx2';";                 binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nOffset=" + nOffset1 + ";";          binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nNumberLines=" + nNumberLines + ";"; binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nLineLength=" + nLineLength + ";";   binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "strDataType='int16';";               binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        // header array 2: DAQ data
                                        nOffset2 = nOffset1 + 2 * nNumberLines * nLineLength * sizeof(Int16);   // two cameras
                                        strTest = "strVar='pdDAQ';";                    binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nOffset=" + nOffset2 + ";";          binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "nNumberLines=" + 4 + ";";            binWriter.Write(strTest.Length);    binWriter.Write(strTest);     // Need double check
                                        strTest = "nLineLength=" + nLineLength + ";";   binWriter.Write(strTest.Length);    binWriter.Write(strTest);
                                        strTest = "strDataType='double';";              binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        strTest = "END"; binWriter.Write(strTest.Length);    binWriter.Write(strTest);

                                        // save array 1: IMAQ data (parallel and perpendicular)
                                        fs.Seek(nOffset1, SeekOrigin.Begin);

                                        for (int nChunk = 0; nChunk < nNumberChunks; nChunk++)
                                        {
                                            Array.Copy(nodeSave.Value.pnIMAQParallel[nChunk], 0, pnIMAQParallel, nChunk * nLinesPerChunk * nLineLength, nLinesPerChunk * nLineLength);
                                            Array.Copy(nodeSave.Value.pnIMAQPerpendicular[nChunk], 0, pnIMAQPerpendicular, nChunk * nLinesPerChunk * nLineLength, nLinesPerChunk * nLineLength);

                                        }   
                                        for(int nLine = 0; nLine < nNumberLines; nLine++)
                                        {
                                            for (int nPoint = 0; nPoint < nLineLength; nPoint++)
                                            {
                                                binWriter.Write(pnIMAQParallel[nLine * nLineLength + nPoint]);
                                                binWriter.Write(pnIMAQPerpendicular[nLine * nLineLength + nPoint]);
                                            }
                                        } 

                                        break;
                                    case 2: // line field

                                        break;
                                    case 3: // OFDI (pgreg002 here is a section to see how the arrays are defined in each node
                                        
                                        break;
                                    case 4: // PS-OFDI

                                        break;
                                } // switch (UIData.nLLSystemType)

                                fs.Close();                              

                                nodeSave.Value.nSaved = 1;

                                /* End: 20201208 editing by JL */
                            }
                            else
                            {   // if (nodeSave.Value.bRecord
                                nodeSave.Value.nSaved = 2;
                            }   // if (nodeSave.Value.bRecord
                            // save complete
                            nodeSave.Value.rwls.ExitReadLock();
                            // find next node
                            nodeSave = nodeSave.Next;
                            if (nodeSave == null)
                                nodeSave = nodeList.First;
                            // signal other threads
                            threadData.strSaveThreadStatus = "done (" + threadData.ssSaveAction.CurrentCount + ")!";
                        }
                        else
                        {  // if (nodeSave.Value.nAcquired
                            // something wrong
                            threadData.strSaveThreadStatus = "nothing to save! (" + threadData.ssSaveAction.CurrentCount + ")!";
                            nodeSave.Value.rwls.ExitReadLock();
                            bTroublemaker = true;
                            threadData.mreSaveKill.Set();
                            threadData.mreMainKill.Set();
                        }  // if (nodeSave.Value.nAcquired
                    }
                    else
                    {  // if (nodeSave.Value.rwls.TryEnterReadLock
                        // something wrong!
                        threadData.strSaveThreadStatus = "timeout! (" + threadData.ssSaveAction.CurrentCount + ")!";
                        bTroublemaker = true;
                        threadData.mreSaveKill.Set();
                        threadData.mreMainKill.Set();
                    }  // if (nodeSave.Value.rwls.TryEnterReadLock
                }  // while (WaitHandle.WaitAny
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreSaveDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strSaveThreadStatus = "Cleaning up...";
                // clean up code
                ;
                // signal other threads
                threadData.mreSaveDead.Set();
                threadData.strSaveThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion

        }   // void SaveThread

        void ProcessThread()
        {
            // start (from Process1, semaphoreslim from Acquire), kill
            #region initializing
            threadData.strProcessThreadStatus = "Initializing...";

            // initialization
            bool bTroublemaker = false;
            LinkedListNode<CDataNode> nodeProcess;
            nodeProcess = nodeList.First;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreProcessKill;
            pweStart[1] = threadData.mreProcessRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreProcessKill;
            pweLoop[1] = threadData.ssProcessAction.AvailableWaitHandle;

            // initialization complete
            threadData.mreProcessReady.Set();
            threadData.strProcessThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strProcessThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strProcessThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) != 0)
                {
                    threadData.strProcessThreadStatus = "finding node (" + threadData.ssProcessAction.CurrentCount + ")!";
                    while (threadData.ssProcessAction.CurrentCount > 1)
                    {
                        threadData.ssProcessAction.Wait();
                        if (nodeProcess.Value.rwls.TryEnterReadLock(0) == true)
                        {
                            nodeProcess.Value.nProcessed = 2;
                            nodeProcess.Value.rwls.ExitReadLock();
                            nodeProcess = nodeProcess.Next;
                            if (nodeProcess == null)
                                nodeProcess = nodeList.First;
                        }
                        else
                        {  // if (nodeProcess.Value.rwls.TryEnterReadLock
                            // something wrong
                            threadData.strProcessThreadStatus = "problem finding node! (" + threadData.ssProcessAction.CurrentCount + ")!";
                            bTroublemaker = true;
                            threadData.mreProcessKill.Set();
                            threadData.mreMainKill.Set();
                        }  // if (nodeProcess.Value.rwls.TryEnterReadLock
                    }  // while (threadData.ssProcessAction.CurrentCount

                    threadData.strProcessThreadStatus = "at correct node (" + threadData.ssProcessAction.CurrentCount + ")!";
                    threadData.ssProcessAction.Wait();
                    if (nodeProcess.Value.rwls.TryEnterReadLock(0) == true)
                    {
                        threadData.strProcessThreadStatus = "processing (" + threadData.ssProcessAction.CurrentCount + ")!";
                        if (threadData.rwlsProcessTo1.TryEnterWriteLock(0) == true)
                        {
                            threadData.nProcessNode = nodeProcess.Value.nNodeID;
                            switch (UIData.nLLSystemType)
                            {
                                case 0: // SD-OCT
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        Buffer.BlockCopy(nodeProcess.Value.pnDAQ, 0, threadData.pnProcess1DAQ, 0, nodeProcess.Value.pnDAQ.Length);
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        {
//                                            Buffer.BlockCopy(nodeProcess.Value.pnAlazar[nChunk], 0, threadData.pnProcess1Alazar, nChunk * 2 * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnAlazar[nChunk].Length);
                                            Array.Copy(nodeProcess.Value.pnIMAQParallel[nChunk], 0, threadData.pnProcess1IMAQParallel, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, UIData.nLLLinesPerChunk * threadData.nRawAlineLength);

//                                          Buffer.BlockCopy(nodeProcess.Value.pnIMAQPerpendicular[nChunk], 0, threadData.pnProcess1IMAQPerpendicular, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQPerpendicular[nChunk].Length);
                                        }   // for (int nChunk
                                    }   // if (UIData.nLLChunksPerImage
                                    break;
                                case 1: // PS SD-OCT
                                    threadData.strProcessThreadStatus = "processingXXX (" + threadData.ssProcessAction.CurrentCount + ")!";
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        Buffer.BlockCopy(nodeProcess.Value.pnDAQ, 0, threadData.pnProcess1DAQ, 0, nodeProcess.Value.pnDAQ.Length);
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        {
//                                            Buffer.BlockCopy(nodeProcess.Value.pnAlazar[nChunk], 0, threadData.pnProcess1Alazar, nChunk * 2 * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnAlazar[nChunk].Length);
                                            Array.Copy(nodeProcess.Value.pnIMAQParallel[nChunk], 0, threadData.pnProcess1IMAQParallel, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength,  UIData.nLLLinesPerChunk * threadData.nRawAlineLength);
                                            Array.Copy(nodeProcess.Value.pnIMAQPerpendicular[nChunk], 0, threadData.pnProcess1IMAQPerpendicular, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, UIData.nLLLinesPerChunk * threadData.nRawAlineLength);

                                        }   // for (int nChunk
                                    }   // if (UIData.nLLChunksPerImage
                                    break;
                                case 2: // line field
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        Buffer.BlockCopy(nodeProcess.Value.pnDAQ, 0, threadData.pnProcess1DAQ, 0, nodeProcess.Value.pnDAQ.Length);
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        {
//                                            Buffer.BlockCopy(nodeProcess.Value.pnAlazar[nChunk], 0, threadData.pnProcess1Alazar, nChunk * 2 * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnAlazar[nChunk].Length);
                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQParallel[nChunk], 0, threadData.pnProcess1IMAQParallel, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQParallel[nChunk].Length);
//                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQPerpendicular[nChunk], 0, threadData.pnProcess1IMAQPerpendicular, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQPerpendicular[nChunk].Length);
                                        }   // for (int nChunk
                                    }   // if (UIData.nLLChunksPerImage
                                    break;
                                case 3: // OFDI
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        Buffer.BlockCopy(nodeProcess.Value.pnDAQ, 0, threadData.pnProcess1DAQ, 0, nodeProcess.Value.pnDAQ.Length);
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        {
                                            Buffer.BlockCopy(nodeProcess.Value.pnAlazar[nChunk], 0, threadData.pnProcess1Alazar, nChunk * 2 * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnAlazar[nChunk].Length);
//                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQParallel[nChunk], 0, threadData.pnProcess1IMAQParallel, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQParallel[nChunk].Length);
//                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQPerpendicular[nChunk], 0, threadData.pnProcess1IMAQPerpendicular, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQPerpendicular[nChunk].Length);
                                        }   // for (int nChunk
                                    }   // if (UIData.nLLChunksPerImage
                                    break;
                                case 4: // PS OFDI
                                    if (UIData.nLLChunksPerImage > 0)
                                    {
                                        Buffer.BlockCopy(nodeProcess.Value.pnDAQ, 0, threadData.pnProcess1DAQ, 0, nodeProcess.Value.pnDAQ.Length);
                                        for (int nChunk = 0; nChunk < UIData.nLLChunksPerImage; nChunk++)
                                        {
                                            Buffer.BlockCopy(nodeProcess.Value.pnAlazar[nChunk], 0, threadData.pnProcess1Alazar, nChunk * 2 * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnAlazar[nChunk].Length);
//                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQParallel[nChunk], 0, threadData.pnProcess1IMAQParallel, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQParallel[nChunk].Length);
//                                            Buffer.BlockCopy(nodeProcess.Value.pnIMAQPerpendicular[nChunk], 0, threadData.pnProcess1IMAQPerpendicular, nChunk * UIData.nLLLinesPerChunk * threadData.nRawAlineLength, nodeProcess.Value.pnIMAQPerpendicular[nChunk].Length);
                                        }   // for (int nChunk
                                    }   // if (UIData.nLLChunksPerImage
                                    break;
                            }   // switch (UIData.nLLSystemType

                            nodeProcess.Value.nProcessed = 1;
                            threadData.rwlsProcessTo1.ExitWriteLock();
                            threadData.mreProcess1Action.Set();
                        }
                        else
                        {
                            nodeProcess.Value.nProcessed = 2;
                        }
                        nodeProcess.Value.rwls.ExitReadLock();
                        nodeProcess = nodeProcess.Next;
                        if (nodeProcess == null)
                            nodeProcess = nodeList.First;
                    }
                    else
                    {  // if (nodeProcess.Value.rwls.TryEnterReadLock
                        // something wrong
                        threadData.strProcessThreadStatus = "problem processing node! (" + threadData.ssProcessAction.CurrentCount + ")!";
                        bTroublemaker = true;
                        threadData.mreProcessKill.Set();
                        threadData.mreMainKill.Set();
                    }  // if (nodeProcess.Value.rwls.TryEnterReadLock
                }  // while (WaitHandle.WaitAny
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreProcessDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strProcessThreadStatus = "Cleaning up...";
                // clean up code
                ;
                // signal other threads
                threadData.mreProcessDead.Set();
                threadData.strProcessThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion

        }

        void Process1Thread()
        {
            #region initializing
            threadData.strProcess1ThreadStatus = "Initializing...";

            // initialization
            bool bTroublemaker = false;
            int nAline, nPoint;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreProcess1Kill;
            pweStart[1] = threadData.mreProcess1Run;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreProcess1Kill;
            pweLoop[1] = threadData.mreProcess1Action;

            /* Begin: 20201210 editing by JL */
            int nNumberDevices = 0;
            int nDeviceNumber = 0;
            StringBuilder strDeviceName = new StringBuilder(256); 
            nOCTcudaWrapper.getDeviceCount(ref nNumberDevices);
            nOCTcudaWrapper.getDeviceName(nDeviceNumber, strDeviceName);

            //int i = 0; 


            /* End: 20201210 editing by JL */

            // initialization complete
            threadData.mreProcess1Ready.Set();
            threadData.strProcess1ThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strProcess1ThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strProcess1ThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) != 0)
                {
                    threadData.mreProcess1Action.Reset();
                    threadData.strProcess1ThreadStatus = "try read lock!";
                    if (threadData.rwlsProcessTo1.TryEnterReadLock(1000) == true)
                    {
                        threadData.strProcess1ThreadStatus = "working...";
                        threadData.nProcess1Node = threadData.nProcessNode;

                        threadData.nProcess2Type = UIData.nURDisplayIndex;
                        if (threadData.nProcess2Type == 8)  // spectral binning
                        {
                            Array.Copy(threadData.pnProcess1IMAQParallel, threadData.pnProcess2AIMAQParallel, threadData.pnProcess1IMAQParallel.Length);
                            Array.Copy(threadData.pnProcess1IMAQPerpendicular, threadData.pnProcess2AIMAQPerpendicular, threadData.pnProcess1IMAQPerpendicular.Length);
                        }
                        switch (UIData.nLLSystemType)
                        {
                            // copy data to GPU dll
                            case 0: // SD-OCT
                                // nOCTcudaWrapper.getDataAlazar();
                                break;
                            case 1: // PS SD-OCT
                                break;
                            case 2: // line field
                                break;
                            case 3: // OFDI
                                break;
                            case 4: // PS OFDI
                                break;
                        }

                        // copy to raw data graphs
                        switch (UIData.nULDisplayIndex)
                        {
                            case 0: // Alazar
                                Array.Clear(UIData.pnULImage, 0, UIData.pnULImage.Length);
                                Array.Clear(UIData.pdULTop, 0, UIData.pdULTop.Length);
                                Array.Clear(UIData.pdULLeft, 0, UIData.pdULLeft.Length);
                                break;
                            case 1: // DAQ
                                Array.Clear(UIData.pnULImage, 0, UIData.pnULImage.Length);
                                Array.Clear(UIData.pdULTop, 0, UIData.pdULTop.Length);
                                Array.Clear(UIData.pdULLeft, 0, UIData.pdULLeft.Length);
                                break;
                            case 2: // IMAQ
                                switch (UIData.nLLSystemType)
                                {
                                    case 0: // SD-OCT
                                        Buffer.BlockCopy(threadData.pnProcess1IMAQParallel, 0, UIData.pnULImage, 0, threadData.pnProcess1IMAQParallel.Length * sizeof(Int16));
                                        break;
                                    case 1: // PS SD-OCT
                                        // ULMain
                                        if (UIData.nULIMAQCameraIndex == 1)
                                            Buffer.BlockCopy(threadData.pnProcess1IMAQPerpendicular, 0, UIData.pnULImage, 0, threadData.pnProcess1IMAQPerpendicular.Length * sizeof(Int16));
                                        else
                                            Buffer.BlockCopy(threadData.pnProcess1IMAQParallel, 0, UIData.pnULImage, 0, threadData.pnProcess1IMAQParallel.Length * sizeof(Int16));
                                        // ULLeft
                                        nAline = UIData.nULIMAQLeft;
                                        if (nAline < 0) nAline = 0;
                                        if (nAline >= threadData.nRawNumberAlines) nAline = threadData.nRawNumberAlines - 1;
                                        for (nPoint = 0; nPoint < threadData.nRawAlineLength; nPoint++)
                                        {
                                            UIData.pdULLeft[0, nPoint] = UIData.pnULImage[nAline, nPoint];
                                        }
                                        // ULTop
                                        nPoint = UIData.nULIMAQTop;
                                        if (nPoint < 0) nPoint = 0;
                                        if (nPoint >= threadData.nRawAlineLength) nPoint = threadData.nRawAlineLength - 1;
                                        for (nAline = 0; nAline < threadData.nRawNumberAlines; nAline++)
                                        {
                                            UIData.pdULTop[0, nAline] = UIData.pnULImage[nAline, nPoint];
                                        }
                                        break;
                                    case 2: // line field
                                        break;
                                    case 3: // OFDI
                                        Array.Clear(UIData.pnULImage, 0, UIData.pnULImage.Length);
                                        Array.Clear(UIData.pdULTop, 0, UIData.pdULTop.Length);
                                        Array.Clear(UIData.pdULLeft, 0, UIData.pdULLeft.Length);
                                        break;
                                    case 4: // PS OFDI
                                        Array.Clear(UIData.pnULImage, 0, UIData.pnULImage.Length);
                                        Array.Clear(UIData.pdULTop, 0, UIData.pdULTop.Length);
                                        Array.Clear(UIData.pdULLeft, 0, UIData.pdULLeft.Length);
                                        break;
                                }   // switch (UIData.nLLSystemType
                                break;
                            case 3: // intensity
                                Array.Clear(UIData.pnULImage, 0, UIData.pnULImage.Length);
                                Array.Clear(UIData.pdULTop, 0, UIData.pdULTop.Length);
                                Array.Clear(UIData.pdULLeft, 0, UIData.pdULLeft.Length);
                                break;
                        }

                        threadData.rwlsProcessTo1.ExitReadLock();

                        Thread.Sleep(1000); // actual processing time

                        if (threadData.rwlsProcess1To2.TryEnterWriteLock(1000))
                        {

                            switch (threadData.nProcess2Type)
                            {
                                case 0:  // none
                                    break;
                                case 1:  // intensity
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 2:  // attenuation
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 3:  // phase
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 4:  // polarization
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 5:  // angiography
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 6:  // elastography
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 7:  // spectroscopy
                                    // copy results from GPU
                                    // into pnProcess2 data structures
                                    break;
                                case 8:  // spectral binning
                                    break;
                            }

                            threadData.rwlsProcess1To2.ExitWriteLock();
                            threadData.mreProcess2Action.Set();
                        }
                        else
                        {
                            bTroublemaker = true;
                            threadData.strProcess1ThreadStatus = "1 to 2 timeout!";
                            threadData.mreProcess1Kill.Set();
                            threadData.mreMainKill.Set();
                        }


                        threadData.strProcess1ThreadStatus = "done!";
                    }
                    else
                    {
                        bTroublemaker = true;
                        threadData.strProcess1ThreadStatus = "problem!";
                        threadData.mreProcess1Kill.Set();
                        threadData.mreMainKill.Set();
                    }

                }

            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreProcess1Dead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strProcess1ThreadStatus = "Cleaning up...";
                // clean up code
                threadData.nProcess1Node = -1;
                // signal other threads
                threadData.mreProcess1Dead.Set();
                threadData.strProcess1ThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion
        }

        void Process2Thread()
        {
            #region initializing
            threadData.strProcess2ThreadStatus = "Initializing...";

            // initialization
            bool bTroublemaker = false;

            int nProcess2Type;

            int nAline, nPoint;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreProcess2Kill;
            pweStart[1] = threadData.mreProcess2Run;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreProcess2Kill;
            pweLoop[1] = threadData.mreProcess2Action;

            // initialization complete
            threadData.mreProcess2Ready.Set();
            threadData.strProcess2ThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strProcess2ThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strProcess2ThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) != 0)
                {
                    threadData.mreProcess2Action.Reset();
                    threadData.strProcess2ThreadStatus = "try read lock!";
                    if (threadData.rwlsProcess1To2.TryEnterReadLock(1000))
                    {
                        threadData.strProcess2ThreadStatus = "working...";
                        threadData.nProcess2Node = threadData.nProcess1Node;
                        nProcess2Type = threadData.nProcess2Type;

                        switch (nProcess2Type)
                        {
                            case 0:
                                threadData.strProcess2ThreadStatus = "...none...";
                                break;
                            case 1:
                                threadData.strProcess2ThreadStatus = "...intensity...";
                                break;
                            case 2:
                                threadData.strProcess2ThreadStatus = "...attenuation...";
                                break;
                            case 3:
                                threadData.strProcess2ThreadStatus = "...phase...";
                                break;
                            case 4:
                                threadData.strProcess2ThreadStatus = "...polarization...";
                                break;
                            case 5:
                                threadData.strProcess2ThreadStatus = "...angiography...";
                                break;
                            case 6:
                                threadData.strProcess2ThreadStatus = "...elastography...";
                                break;
                            case 7:
                                threadData.strProcess2ThreadStatus = "...spectroscopy...";
                                break;
                            case 8:
                                threadData.strProcess2ThreadStatus = "...spectral binning...";

                                // call to ipp thread for spectral binning
                                Thread.Sleep(500);

                                break;
                        }   // switch (nProcess2Type

                        threadData.rwlsProcess1To2.ExitReadLock();

                        switch (nProcess2Type)
                        {
                            case 0:
                                threadData.strProcess2ThreadStatus = "...none...";
                                break;
                            case 1:
                                threadData.strProcess2ThreadStatus = "...intensity...";
                                break;
                            case 2:
                                threadData.strProcess2ThreadStatus = "...attenuation...";
                                break;
                            case 3:
                                threadData.strProcess2ThreadStatus = "...phase...";
                                break;
                            case 4:
                                threadData.strProcess2ThreadStatus = "...polarization...";
                                break;
                            case 5:
                                threadData.strProcess2ThreadStatus = "...angiography...";
                                break;
                            case 6:
                                threadData.strProcess2ThreadStatus = "...elastography...";
                                break;
                            case 7:
                                threadData.strProcess2ThreadStatus = "...spectroscopy...";
                                break;
                            case 8:
                                threadData.strProcess2ThreadStatus = "...spectral binning...";

                                // actual processing
                                Thread.Sleep(3000);

                                // copy data to upper right data structures
                                // URImage
                                for (nAline = 0; nAline < threadData.nProcessedNumberAlines; nAline++)
                                    for (nPoint = 0; nPoint < threadData.nProcessedAlineLength; nPoint++)
                                        UIData.pdURImage[nAline, nPoint] = Math.Sqrt(threadData.pnProcess2AIMAQParallel[nAline * threadData.nRawAlineLength + nPoint] * threadData.pnProcess2AIMAQParallel[nAline * threadData.nRawAlineLength + nPoint] + threadData.pnProcess2AIMAQPerpendicular[nAline * threadData.nRawAlineLength + nPoint] * threadData.pnProcess2AIMAQPerpendicular[nAline * threadData.nRawAlineLength + nPoint]);
                                // URLeft
                                nAline = UIData.nURSpectralBinningLeft;
                                if (nAline < 0) nAline = 0;
                                if (nAline >= threadData.nProcessedNumberAlines) nAline = threadData.nProcessedNumberAlines - 1;
                                for (nPoint = 0; nPoint < threadData.nProcessedAlineLength; nPoint++)
                                    UIData.pdURLeft[0, nPoint] = UIData.pdURImage[nAline, nPoint];
                                // URTop
                                nPoint = UIData.nURSpectralBinningTop;
                                if (nPoint < 0) nPoint = 0;
                                if (nPoint >= threadData.nProcessedAlineLength) nPoint = threadData.nProcessedAlineLength - 1;
                                for (nAline = 0; nAline < threadData.nProcessedNumberAlines; nAline++)
                                    UIData.pdURTop[0, nAline] = UIData.pdURImage[nAline, nPoint];

                                break;
                        }   // switch (nProcess2Type

                        threadData.strProcess2ThreadStatus = "done!";
                    }
                    else
                    {
                        bTroublemaker = true;
                        threadData.strProcess2ThreadStatus = "problem!";
                        threadData.mreProcess2Kill.Set();
                        threadData.mreMainKill.Set();
                    }

                }

            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreProcess2Dead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strProcess2ThreadStatus = "Cleaning up...";
                // clean up code
                threadData.nProcess2Node = -1;
                // signal other threads
                threadData.mreProcess2Dead.Set();
                threadData.strProcess2ThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion
        }

        void CleanupThread()
        {
            #region initializing
            threadData.strCleanupThreadStatus = "Initializing...";

            // initialization
            bool bTroublemaker = false;
            LinkedListNode<CDataNode> nodeCleanup;
            nodeCleanup = nodeList.First;

            // set up wait handles to start
            WaitHandle[] pweStart = new WaitHandle[2];
            pweStart[0] = threadData.mreCleanupKill;
            pweStart[1] = threadData.mreCleanupRun;

            WaitHandle[] pweLoop = new WaitHandle[2];
            pweLoop[0] = threadData.mreCleanupAction;
            pweLoop[1] = threadData.mreCleanupKill;

            // initialization complete
            threadData.mreCleanupReady.Set();
            threadData.strCleanupThreadStatus = "Ready!";
            #endregion

            #region main loop
            threadData.strCleanupThreadStatus = "Set...";
            if (WaitHandle.WaitAny(pweStart) == 1)
            {
                threadData.strCleanupThreadStatus = "GO!";

                while (WaitHandle.WaitAny(pweLoop) == 0)
                {
                    threadData.strCleanupThreadStatus = "action...";
                    if (nodeCleanup.Value.rwls.TryEnterReadLock(1000) == true)
                    {
                        if (nodeCleanup.Value.nAcquired == 0)
                        {
                            threadData.strCleanupThreadStatus = "blank!";
                            nodeCleanup.Value.rwls.ExitReadLock();
                            threadData.mreCleanupAction.Reset();
                        }
                        else
                        {  // if (nodeCleanup.Value.nAcquired
                            if ((nodeCleanup.Value.nSaved > 0) && (nodeCleanup.Value.nProcessed > 0))
                            {
                                threadData.strCleanupThreadStatus = "cleaning!";
                                nodeCleanup.Value.nAcquired = 0;
                                nodeCleanup.Value.nSaved = 0;
                                nodeCleanup.Value.nProcessed = 0;
                                nodeCleanup.Value.rwls.ExitReadLock();
                                nodeCleanup = nodeCleanup.Next;
                                if (nodeCleanup == null)
                                    nodeCleanup = nodeList.First;
                            }
                            else
                            {  // if ((nodeCleanup.Value.nSaved
                                threadData.strCleanupThreadStatus = "waiting!";
                                nodeCleanup.Value.rwls.ExitReadLock();
                                threadData.mreCleanupAction.Reset();
                            }  // if ((nodeCleanup.Value.nSaved
                        }  // if (nodeCleanup.Value.nAcquired
                    }
                    else
                    {  // if (nodeCleanup.Value.rwls.TryEnterReadLock
                        threadData.strCleanupThreadStatus = "timeout!";
                        bTroublemaker = true;
                        threadData.mreCleanupKill.Set();
                        threadData.mreMainKill.Set();
                    }  // if (nodeCleanup.Value.rwls.TryEnterReadLock

                }  // while (WaitHandle.WaitAny
            }  // if (WaitHandle.WaitAny
            #endregion

            #region cleanup
            if (bTroublemaker)
            {
                threadData.mreCleanupDead.Set();
            }
            else
            {  // if (bTroublemaker
                #region cleanup
                threadData.strCleanupThreadStatus = "Cleaning up...";
                // clean up code
                ;
                // signal other threads
                threadData.mreCleanupDead.Set();
                threadData.strCleanupThreadStatus = "Done.";
                #endregion
            }  // if (bTroublemaker
            #endregion

        }

        private void graphULMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            UIData.nULIMAQLeft = (int)(((float)(p.X) - 303.0) / 536.0 * (threadData.nRawNumberAlines - 1));
            UIData.nULIMAQTop = (int)(((float)(p.Y) - 300.0) / 471.0 * (threadData.nRawAlineLength - 1));
        }


        private void graphURMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);
            UIData.nURSpectralBinningLeft = (int)(((float)(p.X) - 1239.0) / 539.0 * (threadData.nProcessedNumberAlines - 1));
            UIData.nURSpectralBinningTop = (int)(((float)(p.Y) - 298.0) / 474.0 * (threadData.nProcessedAlineLength - 1));
            UIData.nURIntensityLeft = (int)(p.X);
            UIData.nURIntensityTop = (int)(p.Y);
        }

    }



    public class CUIData : INotifyPropertyChanged
    {

        public Random rnd = new Random();

        #region templates

        public string name_strXXX = "strXXX";
        private string _strXXX;
        public string strXXX
        {
            get { return _strXXX; }
            set { _strXXX = value; OnPropertyChanged(name_strXXX); }
        }   // public string strXXX

        public string name_nXXX = "nXXX";
        private int _nXXX;
        public int nXXX
        {
            get { return _nXXX; }
            set { _nXXX = value; OnPropertyChanged(name_nXXX); }
        }   // public int nXXX

        public string name_bXXX = "bXXX";
        private bool _bXXX;
        public bool bXXX
        {
            get { return _bXXX; }
            set { _bXXX = value; OnPropertyChanged(name_bXXX); }
        }   // public bool bXXX

        public string name_dXXX = "dXXX";
        private double _dXXX;
        public double dXXX
        {
            get { return _dXXX; }
            set { _dXXX = value; OnPropertyChanged(name_dXXX); }
        }   // public double dXXX

        #endregion

        #region UL

        public string name_nULDisplayIndex = "nULDisplayIndex";
        private int _nULDisplayIndex;
        public int nULDisplayIndex
        {
            get { return _nULDisplayIndex; }
            set { _nULDisplayIndex = value; OnPropertyChanged(name_nULDisplayIndex); }
        }   // public int nULDisplayIndex

        public string name_nULAlazarReferenceIndex = "nULAlazarReferenceIndex";
        private int _nULAlazarReferenceIndex;
        public int nULAlazarReferenceIndex
        {
            get { return _nULAlazarReferenceIndex; }
            set { _nULAlazarReferenceIndex = value; OnPropertyChanged(name_nULAlazarReferenceIndex); }
        }   // public int nULAlazarReferenceIndex

        public string name_nULAlazarTop = "nULAlazarTop";
        private int _nULAlazarTop;
        public int nULAlazarTop
        {
            get { return _nULAlazarTop; }
            set { _nULAlazarTop = value; OnPropertyChanged(name_nULAlazarTop); }
        }   // public int nULAlazarTop

        public string name_nULAlazarLeft = "nULAlazarLeft";
        private int _nULAlazarLeft;
        public int nULAlazarLeft
        {
            get { return _nULAlazarLeft; }
            set { _nULAlazarLeft = value; OnPropertyChanged(name_nULAlazarLeft); }
        }   // public int nULAlazarLeft

        public string name_dULAlazarMax = "dULAlazarMax";
        private double _dULAlazarMax;
        public double dULAlazarMax
        {
            get { return _dULAlazarMax; }
            set { _dULAlazarMax = value; OnPropertyChanged(name_dULAlazarMax); }
        }   // public double dULAlazarMax

        public string name_dULAlazarMin = "dULAlazarMin";
        private double _dULAlazarMin;
        public double dULAlazarMin
        {
            get { return _dULAlazarMin; }
            set { _dULAlazarMin = value; OnPropertyChanged(name_dULAlazarMin); }
        }   // public double dULAlazarMin

        public string name_nULDAQReferenceIndex = "nULDAQReferenceIndex";
        private int _nULDAQReferenceIndex;
        public int nULDAQReferenceIndex
        {
            get { return _nULDAQReferenceIndex; }
            set { _nULDAQReferenceIndex = value; OnPropertyChanged(name_nULDAQReferenceIndex); }
        }   // public int nULDAQReferenceIndex

        public string name_nULDAQTop = "nULDAQTop";
        private int _nULDAQTop;
        public int nULDAQTop
        {
            get { return _nULDAQTop; }
            set { _nULDAQTop = value; OnPropertyChanged(name_nULDAQTop); }
        }   // public int nULDAQTop

        public string name_nULDAQLeft = "nULDAQLeft";
        private int _nULDAQLeft;
        public int nULDAQLeft
        {
            get { return _nULDAQLeft; }
            set { _nULDAQLeft = value; OnPropertyChanged(name_nULDAQLeft); }
        }   // public int nULDAQLeft

        public string name_nULIMAQCameraIndex = "nULIMAQCameraIndex";
        private int _nULIMAQCameraIndex;
        public int nULIMAQCameraIndex
        {
            get { return _nULIMAQCameraIndex; }
            set { _nULIMAQCameraIndex = value; OnPropertyChanged(name_nULIMAQCameraIndex); }
        }   // public int nULIMAQCameraIndex

        public string name_dULDAQMin = "dULDAQMin";
        private double _dULDAQMin;
        public double dULDAQMin
        {
            get { return _dULDAQMin; }
            set { _dULDAQMin = value; OnPropertyChanged(name_dULDAQMin); }
        }   // public double dULDAQMin

        public string name_dULDAQMax = "dULDAQMax";
        private double _dULDAQMax;
        public double dULDAQMax
        {
            get { return _dULDAQMax; }
            set { _dULDAQMax = value; OnPropertyChanged(name_dULDAQMax); }
        }   // public double dULDAQMax

        public string name_nULIMAQReferenceIndex = "nULIMAQReferenceIndex";
        private int _nULIMAQReferenceIndex;
        public int nULIMAQReferenceIndex
        {
            get { return _nULIMAQReferenceIndex; }
            set { _nULIMAQReferenceIndex = value; OnPropertyChanged(name_nULIMAQReferenceIndex); }
        }   // public int nULIMAQReferenceIndex

        public string name_nULIMAQTop = "nULIMAQTop";
        private int _nULIMAQTop;
        public int nULIMAQTop
        {
            get { return _nULIMAQTop; }
            set { _nULIMAQTop = value; OnPropertyChanged(name_nULIMAQTop); }
        }   // public int nULIMAQTop

        public string name_nULIMAQLeft = "nULIMAQLeft";
        private int _nULIMAQLeft;
        public int nULIMAQLeft
        {
            get { return _nULIMAQLeft; }
            set { _nULIMAQLeft = value; OnPropertyChanged(name_nULIMAQLeft); }
        }   // public int nULIMAQLeft

        public string name_dULIMAQMax = "dULIMAQMax";
        private double _dULIMAQMax;
        public double dULIMAQMax
        {
            get { return _dULIMAQMax; }
            set { _dULIMAQMax = value; OnPropertyChanged(name_dULIMAQMax); }
        }   // public double dULIMAQMax

        public string name_dULIMAQMin = "dULIMAQMin";
        private double _dULIMAQMin;
        public double dULIMAQMin
        {
            get { return _dULIMAQMin; }
            set { _dULIMAQMin = value; OnPropertyChanged(name_dULIMAQMin); }
        }   // public double dULIMAQMin

        public string name_nULIntensityTop = "nULIntensityTop";
        private int _nULIntensityTop;
        public int nULIntensityTop
        {
            get { return _nULIntensityTop; }
            set { _nULIntensityTop = value; OnPropertyChanged(name_nULIntensityTop); }
        }   // public int nULIntensityTop

        public string name_nULIntensityLeft = "nULIntensityLeft";
        private int _nULIntensityLeft;
        public int nULIntensityLeft
        {
            get { return _nULIntensityLeft; }
            set { _nULIntensityLeft = value; OnPropertyChanged(name_nULIntensityLeft); }
        }   // public int nULIntensityLeft

        public string name_dULIntensityMax = "dULIntensityMax";
        private double _dULIntensityMax;
        public double dULIntensityMax
        {
            get { return _dULIntensityMax; }
            set { _dULIntensityMax = value; OnPropertyChanged(name_dULIntensityMax); }
        }   // public double dULIntensityMax

        public string name_dULIntensityMin = "dULIntensityMin";
        private double _dULIntensityMin;
        public double dULIntensityMin
        {
            get { return _dULIntensityMin; }
            set { _dULIntensityMin = value; OnPropertyChanged(name_dULIntensityMin); }
        }   // public double dULIntensityMin

        #endregion

        #region UR

        public string name_nURDisplayIndex = "nURDisplayIndex";
        private int _nURDisplayIndex;
        public int nURDisplayIndex
        {
            get { return _nURDisplayIndex; }
            set { _nURDisplayIndex = value; OnPropertyChanged(name_nURDisplayIndex); }
        }   // public int nURDisplayIndex

        public string name_nURIntensityTop = "nURIntensityTop";
        private int _nURIntensityTop;
        public int nURIntensityTop
        {
            get { return _nURIntensityTop; }
            set { _nURIntensityTop = value; OnPropertyChanged(name_nURIntensityTop); }
        }   // public int nURIntensityTop

        public string name_nURIntensityLeft = "nURIntensityLeft";
        private int _nURIntensityLeft;
        public int nURIntensityLeft
        {
            get { return _nURIntensityLeft; }
            set { _nURIntensityLeft = value; OnPropertyChanged(name_nURIntensityLeft); }
        }   // public int nURIntensityLeft

        /* Begin: 20201210 editing by JL */
        public string name_nURIntensityCUDA = "nURIntensityCUDA";
        private int _nURIntensityCUDA;
        public int nURIntensityCUDA
        {
            get { return _nURIntensityCUDA; }
            set { _nURIntensityCUDA = value; OnPropertyChanged(name_nURIntensityCUDA); }
        }   // public int nURIntensityCUDA
        /* End: 20201210 editing by JL */

        public string name_nURSpectralBinningTop = "nURSpectralBinningTop";
        private int _nURSpectralBinningTop;
        public int nURSpectralBinningTop
        {
            get { return _nURSpectralBinningTop; }
            set { _nURSpectralBinningTop = value; OnPropertyChanged(name_nURSpectralBinningTop); }
        }   // public int nURSpectralBinningTop

        public string name_nURSpectralBinningLeft = "nURSpectralBinningLeft";
        private int _nURSpectralBinningLeft;
        public int nURSpectralBinningLeft
        {
            get { return _nURSpectralBinningLeft; }
            set { _nURSpectralBinningLeft = value; OnPropertyChanged(name_nURSpectralBinningLeft); }
        }   // public int nURSpectralBinningLeft

        #endregion

        #region LL

        public string name_strLLConfigurationFilename = "strLLConfigurationFilename";
        private string _strLLConfigurationFilename;
        public string strLLConfigurationFilename
        {
            get { return _strLLConfigurationFilename; }
            set { _strLLConfigurationFilename = value; OnPropertyChanged(name_strLLConfigurationFilename); }
        }   // public string strLLConfigurationFilename

        public string name_strLLAlazarBoard = "strLLAlazarBoard";
        private string _strLLAlazarBoard;
        public string strLLAlazarBoard
        {
            get { return _strLLAlazarBoard; }
            set { _strLLAlazarBoard = value; OnPropertyChanged(name_strLLAlazarBoard); }
        }   // public string strLLAlazarBoard

        public string name_bLLAlazarCh1 = "bLLAlazarCh1";
        private bool _bLLAlazarCh1;
        public bool bLLAlazarCh1
        {
            get { return _bLLAlazarCh1; }
            set { _bLLAlazarCh1 = value; OnPropertyChanged(name_bLLAlazarCh1); }
        }   // public bool bLLAlazarCh1

        public string name_bLLAlazarCh2 = "bLLAlazarCh2";
        private bool _bLLAlazarCh2;
        public bool bLLAlazarCh2
        {
            get { return _bLLAlazarCh2; }
            set { _bLLAlazarCh2 = value; OnPropertyChanged(name_bLLAlazarCh2); }
        }   // public bool bLLAlazarCh2

        public string name_nLLAlazarLineLength = "nLLAlazarLineLength";
        private int _nLLAlazarLineLength;
        public int nLLAlazarLineLength
        {
            get { return _nLLAlazarLineLength; }
            set { _nLLAlazarLineLength = value; OnPropertyChanged(name_nLLAlazarLineLength); }
        }   // public int nLLAlazarLineLength

        public string name_nLLAlazarSamplingRate = "nLLAlazarSamplingRate";
        private int _nLLAlazarSamplingRate;
        public int nLLAlazarSamplingRate
        {
            get { return _nLLAlazarSamplingRate; }
            set { _nLLAlazarSamplingRate = value; OnPropertyChanged(name_nLLAlazarSamplingRate); }
        }   // public int nLLAlazarSamplingRate

        public string name_strLLDAQDevice = "strLLDAQDevice";
        private string _strLLDAQDevice;
        public string strLLDAQDevice
        {
            get { return _strLLDAQDevice; }
            set { _strLLDAQDevice = value; OnPropertyChanged(name_strLLDAQDevice); }
        }   // public string strLLDAQDevice

        public string name_strLLIMAQParallel = "strLLIMAQParallel";
        private string _strLLIMAQParallel;
        public string strLLIMAQParallel
        {
            get { return _strLLIMAQParallel; }
            set { _strLLIMAQParallel = value; OnPropertyChanged(name_strLLIMAQParallel); }
        }   // public string strLLIMAQParallel

        public string name_strLLIMAQPerpendicular = "strLLIMAQPerpendicular";
        private string _strLLIMAQPerpendicular;
        public string strLLIMAQPerpendicular
        {
            get { return _strLLIMAQPerpendicular; }
            set { _strLLIMAQPerpendicular = value; OnPropertyChanged(name_strLLIMAQPerpendicular); }
        }   // public string strLLIMAQPerpendicular

        public string name_nLLIMAQLineLength = "nLLIMAQLineLength";
        private int _nLLIMAQLineLength;
        public int nLLIMAQLineLength
        {
            get { return _nLLIMAQLineLength; }
            set { _nLLIMAQLineLength = value; OnPropertyChanged(name_nLLIMAQLineLength); }
        }   // public int nLLIMAQLineLength

        public string name_nLLIMAQRingBuffers = "nLLIMAQRingBuffers";
        private int _nLLIMAQRingBuffers;
        public int nLLIMAQRingBuffers
        {
            get { return _nLLIMAQRingBuffers; }
            set { _nLLIMAQRingBuffers = value; OnPropertyChanged(name_nLLIMAQRingBuffers); }
        }   // public int nLLIMAQRingBuffers

        public string name_nLLSystemType = "nLLSystemType";
        private int _nLLSystemType;
        public int nLLSystemType
        {
            get { return _nLLSystemType; }
            set { _nLLSystemType = value; OnPropertyChanged(name_nLLSystemType); }
        }   // public int nLLSystemType

        public string name_nLLSystemActual = "nLLSystemActual";
        private int _nLLSystemActual;
        public int nLLSystemActual
        {
            get { return _nLLSystemActual; }
            set { _nLLSystemActual = value; OnPropertyChanged(name_nLLSystemActual); }
        }   // public int nLLSystemActual

        public string name_nLLLineRate = "nLLLineRate";
        private int _nLLLineRate;
        public int nLLLineRate
        {
            get { return _nLLLineRate; }
            set { _nLLLineRate = value; OnPropertyChanged(name_nLLLineRate); }
        }   // public int nLLLineRate

        public string name_nLLLinesPerChunk = "nLLLinesPerChunk";
        private int _nLLLinesPerChunk;
        public int nLLLinesPerChunk
        {
            get { return _nLLLinesPerChunk; }
            set { _nLLLinesPerChunk = value; OnPropertyChanged(name_nLLLinesPerChunk); }
        }   // public int nLLLinesPerChunk

        public string name_nLLChunksPerImage = "nLLChunksPerImage";
        private int _nLLChunksPerImage;
        public int nLLChunksPerImage
        {
            get { return _nLLChunksPerImage; }
            set { _nLLChunksPerImage = value; OnPropertyChanged(name_nLLChunksPerImage); }
        }   // public int nLLChunksPerImage

        public string name_nLLImagesPerVolume = "nLLImagesPerVolume";
        private int _nLLImagesPerVolume;
        public int nLLImagesPerVolume
        {
            get { return _nLLImagesPerVolume; }
            set { _nLLImagesPerVolume = value; OnPropertyChanged(name_nLLImagesPerVolume); }
        }   // public int nLLImagesPerVolume

        public string name_nLLLinkedListLength = "nLLLinkedListLength";
        private int _nLLLinkedListLength;
        public int nLLLinkedListLength
        {
            get { return _nLLLinkedListLength; }
            set { _nLLLinkedListLength = value; OnPropertyChanged(name_nLLLinkedListLength); }
        }   // public int nLLLinkedListLength

        public string name_nLLCUDADevice = "nLLCUDADevice";
        private int _nLLCUDADevice;
        public int nLLCUDADevice
        {
            get { return _nLLCUDADevice; }
            set { _nLLCUDADevice = value; OnPropertyChanged(name_nLLCUDADevice); }
        }   // public int nLLCUDADevice

        public string name_strLLFileDirectory = "strLLFileDirectory";
        private string _strLLFileDirectory;
        public string strLLFileDirectory
        {
            get { return _strLLFileDirectory; }
            set { _strLLFileDirectory = value; OnPropertyChanged(name_strLLFileDirectory); }
        }   // public string strLLFileDirectory

        public string name_strLLFilePrefix = "strLLFilePrefix";
        private string _strLLFilePrefix;
        public string strLLFilePrefix
        {
            get { return _strLLFilePrefix; }
            set { _strLLFilePrefix = value; OnPropertyChanged(name_strLLFilePrefix); }
        }   // public string strLLFilePrefix

        public string name_nLLFileNumber = "nLLFileNumber";
        private int _nLLFileNumber;
        public int nLLFileNumber
        {
            get { return _nLLFileNumber; }
            set { _nLLFileNumber = value; OnPropertyChanged(name_nLLFileNumber); }
        }   // public int nLLFileNumber

        public string name_bLLFileRecord = "bLLFileRecord";
        private bool _bLLFileRecord;
        public bool bLLFileRecord
        {
            get { return _bLLFileRecord; }
            set { _bLLFileRecord = value; OnPropertyChanged(name_bLLFileRecord); }
        }   // public bool bLLFileRecord

        public string name_nLLFileCycle = "nLLFileCycle";
        private int _nLLFileCycle;
        public int nLLFileCycle
        {
            get { return _nLLFileCycle; }
            set { _nLLFileCycle = value; OnPropertyChanged(name_nLLFileCycle); }
        }   // public int nLLFileCycle

        public string name_dLLCenterX = "dLLCenterX";
        private double _dLLCenterX;
        public double dLLCenterX
        {
            get { return _dLLCenterX; }
            set { _dLLCenterX = value; OnPropertyChanged(name_dLLCenterX); }
        }   // public double dLLCenterX

        public string name_dLLCenterY = "dLLCenterY";
        private double _dLLCenterY;
        public double dLLCenterY
        {
            get { return _dLLCenterY; }
            set { _dLLCenterY = value; OnPropertyChanged(name_dLLCenterY); }
        }   // public double dLLCenterY

        public string name_dLLFastAngle = "dLLFastAngle";
        private double _dLLFastAngle;
        public double dLLFastAngle
        {
            get { return _dLLFastAngle; }
            set { _dLLFastAngle = value; OnPropertyChanged(name_dLLFastAngle); }
        }   // public double dLLFastAngle

        public string name_dLLRangeFast = "dLLRangeFast";
        private double _dLLRangeFast;
        public double dLLRangeFast
        {
            get { return _dLLRangeFast; }
            set { _dLLRangeFast = value; OnPropertyChanged(name_dLLRangeFast); }
        }   // public double dLLRangeFast

        public string name_dLLRangeSlow = "dLLRangeSlow";
        private double _dLLRangeSlow;
        public double dLLRangeSlow
        {
            get { return _dLLRangeSlow; }
            set { _dLLRangeSlow = value; OnPropertyChanged(name_dLLRangeSlow); }
        }   // public double dLLRangeSlow

        public string name_nLLDwellFast = "nLLDwellFast";
        private int _nLLDwellFast;
        public int nLLDwellFast
        {
            get { return _nLLDwellFast; }
            set { _nLLDwellFast = value; OnPropertyChanged(name_nLLDwellFast); }
        }   // public int nLLDwellFast

        public string name_nLLDwellSlow = "nLLDwellSlow";
        private int _nLLDwellSlow;
        public int nLLDwellSlow
        {
            get { return _nLLDwellSlow; }
            set { _nLLDwellSlow = value; OnPropertyChanged(name_nLLDwellSlow); }
        }   // public int nLLDwellSlow

        public string name_nLLRoundingFast = "nLLRoundingFast";
        private int _nLLRoundingFast;
        public int nLLRoundingFast
        {
            get { return _nLLRoundingFast; }
            set { _nLLRoundingFast = value; OnPropertyChanged(name_nLLRoundingFast); }
        }   // public int nLLRoundingFast

        public string name_nLLRoundingSlow = "nLLRoundingSlow";
        private int _nLLRoundingSlow;
        public int nLLRoundingSlow
        {
            get { return _nLLRoundingSlow; }
            set { _nLLRoundingSlow = value; OnPropertyChanged(name_nLLRoundingSlow); }
        }   // public int nLLRoundingSlow

        #endregion

        #region LR

        public string name_dLRAvailableMemory = "dLRAvailableMemory";
        private double _dLRAvailableMemory;
        public double dLRAvailableMemory
        {
            get { return _dLRAvailableMemory; }
            set { _dLRAvailableMemory = value; OnPropertyChanged(name_dLRAvailableMemory); }
        }   // public double dLRAvailableMemory

        public string name_nLRLinkedListLength = "nLRLinkedListLength";
        private int _nLRLinkedListLength;
        public int nLRLinkedListLength
        {
            get { return _nLRLinkedListLength; }
            set { _nLRLinkedListLength = value; OnPropertyChanged(name_nLRLinkedListLength); }
        }   // public int nLRLinkedListLength

        public string name_strLRMainThreadStatus = "strLRMainThreadStatus";
        private string _strLRMainThreadStatus;
        public string strLRMainThreadStatus
        {
            get { return _strLRMainThreadStatus; }
            set { _strLRMainThreadStatus = value; OnPropertyChanged(name_strLRMainThreadStatus); }
        }   // public string strLRMainThreadStatus

        public string name_strLROutputThreadStatus = "strLROutputThreadStatus";
        private string _strLROutputThreadStatus;
        public string strLROutputThreadStatus
        {
            get { return _strLROutputThreadStatus; }
            set { _strLROutputThreadStatus = value; OnPropertyChanged(name_strLROutputThreadStatus); }
        }   // public string strLROutputThreadStatus

        public string name_strLRCleanupThreadStatus = "strLRCleanupThreadStatus";
        private string _strLRCleanupThreadStatus;
        public string strLRCleanupThreadStatus
        {
            get { return _strLRCleanupThreadStatus; }
            set { _strLRCleanupThreadStatus = value; OnPropertyChanged(name_strLRCleanupThreadStatus); }
        }   // public string strLRCleanupThreadStatus

        public string name_strLRSaveThreadStatus = "strLRSaveThreadStatus";
        private string _strLRSaveThreadStatus;
        public string strLRSaveThreadStatus
        {
            get { return _strLRSaveThreadStatus; }
            set { _strLRSaveThreadStatus = value; OnPropertyChanged(name_strLRSaveThreadStatus); }
        }   // public string strLRSaveThreadStatus

        public string name_strLRAcquireThreadStatus = "strLRAcquireThreadStatus";
        private string _strLRAcquireThreadStatus;
        public string strLRAcquireThreadStatus
        {
            get { return _strLRAcquireThreadStatus; }
            set { _strLRAcquireThreadStatus = value; OnPropertyChanged(name_strLRAcquireThreadStatus); }
        }   // public string strLRAcquireThreadStatus

        public string name_strLRProcessThreadStatus = "strLRProcessThreadStatus";
        private string _strLRProcessThreadStatus;
        public string strLRProcessThreadStatus
        {
            get { return _strLRProcessThreadStatus; }
            set { _strLRProcessThreadStatus = value; OnPropertyChanged(name_strLRProcessThreadStatus); }
        }   // public string strLRProcessThreadStatus

        public string name_strLRProcess1ThreadStatus = "strLRProcess1ThreadStatus";
        private string _strLRProcess1ThreadStatus;
        public string strLRProcess1ThreadStatus
        {
            get { return _strLRProcess1ThreadStatus; }
            set { _strLRProcess1ThreadStatus = value; OnPropertyChanged(name_strLRProcess1ThreadStatus); }
        }   // public string strLRProcess1ThreadStatus

        public string name_strLRProcess2ThreadStatus = "strLRProcess2ThreadStatus";
        private string _strLRProcess2ThreadStatus;
        public string strLRProcess2ThreadStatus
        {
            get { return _strLRProcess2ThreadStatus; }
            set { _strLRProcess2ThreadStatus = value; OnPropertyChanged(name_strLRProcess2ThreadStatus); }
        }   // public string strLRProcess2ThreadStatus

        #endregion

        #region graphs
        public int[,] pnLinkedList = null;

        public double[,] pdULLeft = null;
        public double[,] pdULTop = null;
        public Int16[,] pnULImage = null;

        public double[,] pdURLeft = null;
        public double[,] pdURTop = null;
        public double[,] pdURImage = null;

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }   // if (handler
        }   // protected void OnPropertyChanged
    }   // public class CUIData

    public class CDataNode
    {

        public int nNodeID;
        /* Begin: 20201208 editing by JL */
        public string strFilename;
        public int nFileNumber; 
        public int nFramePosition;
        /* End: 20201208 editing by JL */

        public ulong nSize;
        public ReaderWriterLockSlim rwls;
        public int nAcquired;
        public bool bRecord;
        public int nSaved;
        public int nProcessed;

        public UInt16[][] pnAlazar;
        public double[] pnDAQ;
        /* Begin: 20201208 editing by JL */
        // SD-OCT
        public Int16[][] pnIMAQ; 
        // PS-SD-OCT
        public Int16[][] pnIMAQParallel;
        public Int16[][] pnIMAQPerpendicular;
        /* End: 20201208 editing by JL */

        public CDataNode(CUIData uiData, int nodeID)
        {
            nSize = sizeof(ulong);
            nNodeID = nodeID; nSize += sizeof(int);

            rwls = new ReaderWriterLockSlim(); nSize += 0;
            nAcquired = 0; nSize += sizeof(int);
            nSaved = 0; nSize += sizeof(int);
            nProcessed = 0; nSize += sizeof(int);

            int nNumberChunks, nLinesPerChunk, nLineLength, nChannels;

            switch (uiData.nLLSystemType)
            {
                case 0: // SD-OCT

                    break;
                case 1: // PS-SD-OCT
                    nNumberChunks = uiData.nLLChunksPerImage;
                    nLinesPerChunk = uiData.nLLLinesPerChunk;
                    nLineLength = uiData.nLLIMAQLineLength;
                    pnIMAQParallel = new Int16[nNumberChunks][];
                    pnIMAQPerpendicular = new Int16[nNumberChunks][];
                    for (int nChunk = 0; nChunk < nNumberChunks; nChunk++)
                    {
                        pnIMAQParallel[nChunk] = new Int16[nLinesPerChunk * nLineLength];
                        pnIMAQPerpendicular[nChunk] = new Int16[nLinesPerChunk * nLineLength];
                        nSize += Convert.ToUInt64(2 * nLinesPerChunk * nLineLength) * sizeof(Int16);
                        Array.Clear(pnIMAQParallel[nChunk], 0, pnIMAQParallel[nChunk].Length);
                        Array.Clear(pnIMAQPerpendicular[nChunk], 0, pnIMAQParallel[nChunk].Length);
                    }   // for (int nChunk
                    pnDAQ = new double[4 * nNumberChunks * nLinesPerChunk];
                    nSize += Convert.ToUInt64(4 * nNumberChunks * nLinesPerChunk * sizeof(double));
                    Array.Clear(pnDAQ, 0, pnDAQ.Length);
                    break;
                case 2: // line field

                    break;
                case 3: // OFDI (pgreg002 here is a section to see how the arrays are defined in each node
                    nNumberChunks = uiData.nLLChunksPerImage;
                    nLinesPerChunk = uiData.nLLLinesPerChunk;
                    nLineLength = uiData.nLLAlazarLineLength;
                    nChannels = 0;
                    if (uiData.bLLAlazarCh1 == true)
                        nChannels++;
                    if (uiData.bLLAlazarCh2 == true)
                        nChannels++;
                    pnAlazar = new UInt16[nNumberChunks][];
                    for (int nChunk = 0; nChunk < nNumberChunks; nChunk++)
                    {
                        pnAlazar[nChunk] = new UInt16[nChannels * nLinesPerChunk * nLineLength];  // 2 - MZI + OCT
                        nSize += Convert.ToUInt64(nChannels * nLinesPerChunk * nLineLength) * sizeof(UInt16);
                        Array.Clear(pnAlazar[nChunk], 0, pnAlazar[nChunk].Length);
                    }   // for (int nChunk
                    pnDAQ = new double[4 * nNumberChunks * nLinesPerChunk];
                    nSize += Convert.ToUInt64(4 * nNumberChunks * nLinesPerChunk * sizeof(double));
                    Array.Clear(pnDAQ, 0, pnDAQ.Length);
                    break;
                case 4: // PS-OFDI

                    break;
            }
        }   // public CDataNode

        ~CDataNode()
        {
            rwls.Dispose();
        }   // ~CDataNode

    }   // public class CDataNode

    public class CThreadData
    {
        public int nRawNumberAlines;
        public int nRawAlineLength;
        public int nProcessedNumberAlines;
        public int nProcessedAlineLength;

        public bool bRecord = false;

        #region MainThread
        public Thread threadMain;
        public ManualResetEvent mreMainReady;
        public ManualResetEvent mreMainRun;
        public ManualResetEvent mreMainKill;
        public ManualResetEvent mreMainDead;
        public string strMainThreadStatus = "XXX";
        #endregion

        #region OutputThread
        public Thread threadOutput;
        public ManualResetEvent mreOutputReady;
        public ManualResetEvent mreOutputRun;
        public ManualResetEvent mreOutputKill;
        public ManualResetEvent mreOutputDead;
        public ManualResetEvent mreOutputUpdate;
        public string strOutputThreadStatus = "XXX";
        #endregion

        #region AcquireThread
        public Thread threadAcquire;
        public ManualResetEvent mreAcquireReady;
        public ManualResetEvent mreAcquireRun;
        public ManualResetEvent mreAcquireKill;
        public ManualResetEvent mreAcquireDead;
        public SemaphoreSlim ssAcquireComplete;
        public string strAcquireThreadStatus = "XXX";
        /* Begin: 20201208 editing JL */
        public int nAcquisitionNodeID;
        public int nFileNumber = 100001; 
        public int nFramePosition = 1;
        /* End: 20201208 editing JL */
        #endregion

        public int nSystemActual;
        public LinkedListNode<CDataNode> nodeAcquire;
        public ManualResetEvent mreAcquireNodeReady;

        #region AcquireAlazarThread
        public Thread threadAcquireAlazar;
        public ManualResetEvent mreAcquireAlazarReady;
        public ManualResetEvent mreAcquireAlazarRun;
        public ManualResetEvent mreAcquireAlazarKill;
        public ManualResetEvent mreAcquireAlazarDead;
        public AutoResetEvent areAcquireAlazarGo;
        public AutoResetEvent areAcquireAlazarComplete;
        public string strAcquireAlazarThreadStatus = "XAla";
        #endregion

        #region AcquireDAQThread
        public Thread threadAcquireDAQ;
        public ManualResetEvent mreAcquireDAQReady;
        public ManualResetEvent mreAcquireDAQRun;
        public ManualResetEvent mreAcquireDAQKill;
        public ManualResetEvent mreAcquireDAQDead;
        public AutoResetEvent areAcquireDAQGo;
        public AutoResetEvent areAcquireDAQComplete;
        public string strAcquireDAQThreadStatus = "XDAQ";
        #endregion

        #region AcquireIMAQThread
        public Thread threadAcquireIMAQ;
        public ManualResetEvent mreAcquireIMAQReady;
        public ManualResetEvent mreAcquireIMAQRun;
        public ManualResetEvent mreAcquireIMAQKill;
        public ManualResetEvent mreAcquireIMAQDead;
        public AutoResetEvent areAcquireIMAQGo;
        public AutoResetEvent areAcquireIMAQComplete;
        public string strAcquireIMAQThreadStatus = "XIMQ";
        #endregion

        #region SaveThread
        public Thread threadSave;
        public ManualResetEvent mreSaveReady;
        public ManualResetEvent mreSaveRun;
        public ManualResetEvent mreSaveKill;
        public ManualResetEvent mreSaveDead;
        public SemaphoreSlim ssSaveAction;
        public string strSaveThreadStatus = "XXX";
        public int nSaveNodeID; 
        #endregion

        #region ProcessThread
        public Thread threadProcess;
        public ManualResetEvent mreProcessReady;
        public ManualResetEvent mreProcessRun;
        public ManualResetEvent mreProcessKill;
        public ManualResetEvent mreProcessDead;
        public SemaphoreSlim ssProcessAction;
        public string strProcessThreadStatus = "XXX";
        #endregion

        public ReaderWriterLockSlim rwlsProcessTo1;
        public UInt16[] pnProcess1Alazar;
        public double[] pnProcess1DAQ;
        public Int16[] pnProcess1IMAQParallel;
        public Int16[] pnProcess1IMAQPerpendicular;
        // public Int16[] pnRawIMAQ; 
        public int nProcessNode = -1;
        public int nProcess1Node = -1;
        public int nProcessTo1Data = 0;

        #region Process1Thread
        public Thread threadProcess1;
        public ManualResetEvent mreProcess1Ready;
        public ManualResetEvent mreProcess1Run;
        public ManualResetEvent mreProcess1Kill;
        public ManualResetEvent mreProcess1Dead;
        public ManualResetEvent mreProcess1Action;
        public string strProcess1ThreadStatus = "XXX";
        #endregion

        public int nProcess2Type;

        public ReaderWriterLockSlim rwlsProcess1To2;
        public int nProcess2Node = -1;
        public int nProcess1To2Data = 0;

        public UInt16[] pnProcess2AAlazar;
        public double[] pnProcess2ADAQ;
        public Int16[] pnProcess2AIMAQParallel;
        public Int16[] pnProcess2AIMAQPerpendicular;

        public double[] pnProcess2ComplexRealParallel;
        public double[] pnProcess2ComplexImagParallel;
        public double[] pnProcess2ComplexRealPerpendicular;
        public double[] pnProcess2ComplexImagPerpendicular;

        public int nProcess2ANode = -1;
        public int nProcess1To2AData = 0;

        #region Process2Thread
        public Thread threadProcess2;
        public ManualResetEvent mreProcess2Ready;
        public ManualResetEvent mreProcess2Run;
        public ManualResetEvent mreProcess2Kill;
        public ManualResetEvent mreProcess2Dead;
        public ManualResetEvent mreProcess2Action;
        public string strProcess2ThreadStatus = "XXX";
        #endregion

        #region CleanupThread
        public Thread threadCleanup;
        public ManualResetEvent mreCleanupReady;
        public ManualResetEvent mreCleanupRun;
        public ManualResetEvent mreCleanupKill;
        public ManualResetEvent mreCleanupDead;
        public ManualResetEvent mreCleanupAction;
        public string strCleanupThreadStatus = "XXX";
        #endregion


        public void Initialize()
        {
            #region MainThread
            mreMainReady = new ManualResetEvent(false);
            mreMainRun = new ManualResetEvent(false);
            mreMainKill = new ManualResetEvent(false);
            mreMainDead = new ManualResetEvent(false);
            #endregion

            #region OutputThread
            mreOutputReady = new ManualResetEvent(false);
            mreOutputRun = new ManualResetEvent(false);
            mreOutputKill = new ManualResetEvent(false);
            mreOutputDead = new ManualResetEvent(false);
            mreOutputUpdate = new ManualResetEvent(false);
            #endregion

            #region AcquireThread
            mreAcquireReady = new ManualResetEvent(false);
            mreAcquireRun = new ManualResetEvent(false);
            mreAcquireKill = new ManualResetEvent(false);
            mreAcquireDead = new ManualResetEvent(false);
            ssAcquireComplete = new SemaphoreSlim(0);
            #endregion

            mreAcquireNodeReady = new ManualResetEvent(false);

            #region AcquireAlazarThread
            mreAcquireAlazarReady = new ManualResetEvent(false);
            mreAcquireAlazarRun = new ManualResetEvent(false);
            mreAcquireAlazarKill = new ManualResetEvent(false);
            mreAcquireAlazarDead = new ManualResetEvent(false);
            areAcquireAlazarGo = new AutoResetEvent(false);
            areAcquireAlazarComplete = new AutoResetEvent(false);
            #endregion

            #region AcquireDAQThread
            mreAcquireDAQReady = new ManualResetEvent(false);
            mreAcquireDAQRun = new ManualResetEvent(false);
            mreAcquireDAQKill = new ManualResetEvent(false);
            mreAcquireDAQDead = new ManualResetEvent(false);
            areAcquireDAQGo = new AutoResetEvent(false);
            areAcquireDAQComplete = new AutoResetEvent(false);
            #endregion

            #region AcquireIMAQThread
            mreAcquireIMAQReady = new ManualResetEvent(false);
            mreAcquireIMAQRun = new ManualResetEvent(false);
            mreAcquireIMAQKill = new ManualResetEvent(false);
            mreAcquireIMAQDead = new ManualResetEvent(false);
            areAcquireIMAQGo = new AutoResetEvent(false);
            areAcquireIMAQComplete = new AutoResetEvent(false);
            #endregion

            #region SaveThead
            mreSaveReady = new ManualResetEvent(false);
            mreSaveRun = new ManualResetEvent(false);
            mreSaveKill = new ManualResetEvent(false);
            mreSaveDead = new ManualResetEvent(false);
            ssSaveAction = new SemaphoreSlim(0);
            #endregion

            #region ProcessThread
            mreProcessReady = new ManualResetEvent(false);
            mreProcessRun = new ManualResetEvent(false);
            mreProcessKill = new ManualResetEvent(false);
            mreProcessDead = new ManualResetEvent(false);
            ssProcessAction = new SemaphoreSlim(0);
            #endregion

            rwlsProcessTo1 = new ReaderWriterLockSlim();

            #region Process1Thread
            mreProcess1Ready = new ManualResetEvent(false);
            mreProcess1Run = new ManualResetEvent(false);
            mreProcess1Kill = new ManualResetEvent(false);
            mreProcess1Dead = new ManualResetEvent(false);
            mreProcess1Action = new ManualResetEvent(true);
            #endregion

            rwlsProcess1To2 = new ReaderWriterLockSlim();

            #region Process2Thread
            mreProcess2Ready = new ManualResetEvent(false);
            mreProcess2Run = new ManualResetEvent(false);
            mreProcess2Kill = new ManualResetEvent(false);
            mreProcess2Dead = new ManualResetEvent(false);
            mreProcess2Action = new ManualResetEvent(true);
            #endregion

            #region CleanupThead
            mreCleanupReady = new ManualResetEvent(false);
            mreCleanupRun = new ManualResetEvent(false);
            mreCleanupKill = new ManualResetEvent(false);
            mreCleanupDead = new ManualResetEvent(false);
            mreCleanupAction = new ManualResetEvent(false);
            #endregion

        }   // public void Initialize

        public void Destroy()
        {
            ;
        }   // public void Destroy

    }   // public class CThreadData

    public class nOCTcudaWrapper : IDisposable
    {
        bool disposed = false;

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Jason Programming\\x64\\Debug\\nOCTcudaDLL.dll")]
        public static extern int getDeviceCount(ref int numberDevices);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Jason Programming\\x64\\Debug\\nOCTcudaDLL.dll")]
        public static extern int getDeviceName(int deviceNumber, StringBuilder strDeviceName);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTcuda\\x64\\Release\\nOCTcuda.dll")]
        public static extern int initialize(int nMode, int nRawNumberAlines, int nRawAlineLength, int nProcessNumberAlines, int nProcessedNumberAlines, int nProcessedAlineLength);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTcuda\\x64\\Release\\nOCTcuda.dll")]
        public static extern int cleanup();

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTcuda\\x64\\Release\\nOCTcuda.dll")]
        public static extern int getDataAlazar();

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTcuda\\x64\\Release\\nOCTcuda.dll")]
        public static extern int calibrateMZI();

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTcuda\\x64\\Release\\nOCTcuda.dll")]
        public static extern int shutdown();

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                disposed = true;
            }   // if (!disposed
        }   // public void Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //free managed ressources
            }   // if (disposing
            // free other ressources
        }   // protected virtual void Dispose

        ~nOCTcudaWrapper()
        {
            Dispose(false);
        }   // ~nOCTcudaWrapper

    }   // public class nOCTcudaWrapper

    public class nOCTippWrapper : IDisposable
    {
        bool disposed = false;

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Acquisition\\nOCTipp\\x64\\Release\\nOCTipp.dll")]
        public static extern int ippTest();

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                disposed = true;
            }   // if (!disposed
        }   // public void Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //free managed ressources
            }   // if (disposing
            // free other ressources
        }   // protected virtual void Dispose

        ~nOCTippWrapper()
        {
            Dispose(false);
        }   // ~nOCTippWrapper

    }   // public class nOCTippWrapper

    public class nOCTimaqWrapper : IDisposable
    {
        bool disposed = false;



        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Hang\\Lab Razer\\nOCTImaq\\x64\\Debug\\nOCTImaq.dll")]
        public static extern int InitializeImaq(char[] interfaceName0, char[] interfaceName1, int nImaqLineLength, int nLinesPerChunk, int errInfo);

        
        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Hang\\Lab Razer\\nOCTImaq\\x64\\Debug\\nOCTImaq.dll")]
        public static extern void StartAcquisition();

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Hang\\Lab Razer\\nOCTImaq\\x64\\Debug\\nOCTImaq.dll")]
        public static extern void RealAcquisition0(int bufferIndex0, Int16[] pnTemp0);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Hang\\Lab Razer\\nOCTImaq\\x64\\Debug\\nOCTImaq.dll")]
        public static extern void RealAcquisition1(int bufferIndex1, Int16[] pnTemp1);

        [SuppressUnmanagedCodeSecurityAttribute()]
        [DllImport("C:\\Users\\ONI-WORKSTATION-01\\Desktop\\Hang\\Lab Razer\\nOCTImaq\\x64\\Debug\\nOCTImaq.dll")]
        public static extern void StopAcquisition();

        
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                disposed = true;
            }   // if (!disposed
        }   // public void Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //free managed ressources
            }   // if (disposing
            // free other ressources
        }   // protected virtual void Dispose

        ~nOCTimaqWrapper()
        {
            Dispose(false);
        }   // ~nOCTimaqWrapper

    }   // public class nOCTimaqWrapper

}
