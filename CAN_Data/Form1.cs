using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.DAQmx;
using NationalInstruments.NI4882;
using NationalInstruments.VisaNS;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Peak.Can.Basic;
using TPCANHandle = System.UInt16;    
using TPCANBitrateFD = System.String;
using TPCANTimestampFD = System.UInt64;
using System.Threading;

namespace CAN_Data
{
    public partial class Form1 : Form
    {
        private TPCANHandle canHandle = PCANBasic.PCAN_USBBUS1;
        private Thread canReadThread;
        private bool isRunning = false;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeCAN()
        {
            TPCANStatus status = PCANBasic.Initialize(canHandle, TPCANBaudrate.PCAN_BAUD_500K);
            if (status != TPCANStatus.PCAN_ERROR_OK)
            {
                MessageBox.Show("CAN 초기화 실패: " + status.ToString());
            }
        }

        private double DecodeCurrentValue(TPCANMsg message)
        {
            if (message.ID == 0x3C2) // CAB 500 센서 CAN ID
            {
                try
                {
                    // 큐를 비워 기존 메시지를 삭제
                    PCANBasic.Reset(canHandle);

                    // CAN 데이터 상위 4바이트 추출
                    byte[] currentBytes = message.DATA.Take(4).ToArray();

                    // Big-Endian에서 Little-Endian으로 변환
                    byte[] reversedData = currentBytes.Reverse().ToArray();

                    // 부호 있는 32비트 정수로 변환
                    int rawValue = BitConverter.ToInt32(reversedData, 0);

                    // 오프셋 적용 (예: 기본값이 0x80000000 = -2147483648인 경우)
                    int offset = unchecked((int)0x80000000); // 기본 오프셋
                    rawValue = unchecked(rawValue - offset);

                    // 스케일링: 0.1mA 단위 -> A로 변환
                    double currentValue = rawValue * 0.001; // 0.1mA -> A로 변환
                    Console.WriteLine("Raw Value: {0}, Offset Applied: {1}, Scaled Value: {2} A",
                                      rawValue + offset, rawValue, currentValue);

                    return currentValue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error decoding current value: " + ex.Message);
                    return 0.0;
                }
            }
            return 0.0;
        }



        private void UpdateCurrentValue(double currentValue)
        {
            // UI 스레드에서 작업 수행
            if (CurrentValue.InvokeRequired) // UI 스레드가 아닌 경우
            {
                CurrentValue.Invoke(new System.Action(() =>
                {
                    CurrentValue.Items.Add(currentValue.ToString("F2") + " A");

                    if (CurrentValue.Items.Count > 100) // 항목 개수 제한
                    {
                        CurrentValue.Items.RemoveAt(0);
                    }

                    CurrentGraph1.PlotYAppend(currentValue, 0.1);

                    // 스크롤을 마지막 항목으로 이동
                    CurrentValue.TopIndex = CurrentValue.Items.Count - 1;
                }));
            }
            else // UI 스레드인 경우 바로 작업
            {
                CurrentValue.Items.Add(currentValue.ToString("F2") + " A");

                if (CurrentValue.Items.Count > 100) // 항목 개수 제한
                {
                    CurrentValue.Items.RemoveAt(0);
                }

                // 스크롤을 마지막 항목으로 이동
                CurrentValue.TopIndex = CurrentValue.Items.Count - 1;
            }
        }

        private void UpdateCANData(TPCANMsg message, int period)
        {
            string dataString = BitConverter.ToString(message.DATA, 0, message.LEN);
            string displayText = "ID: " + message.ID.ToString("X") + ", Len: " + message.LEN +
                                 ", Data: " + dataString + ", Period: " + period + " ms";

            if (CanData.InvokeRequired) // UI 스레드가 아닌 경우
            {
                CanData.Invoke(new System.Action(() =>
                {
                    CanData.Items.Add(displayText);
                    if (CanData.Items.Count > 100) // 항목 개수 제한
                    {
                        CanData.Items.RemoveAt(0);
                    }

                    // 스크롤을 마지막 항목으로 이동
                    CanData.TopIndex = CanData.Items.Count - 1;
                }));
            }
            else // UI 스레드인 경우 바로 작업
            {
                CanData.Items.Add(displayText);
                if (CanData.Items.Count > 100) // 항목 개수 제한
                {
                    CanData.Items.RemoveAt(0);
                }

                // 스크롤을 마지막 항목으로 이동
                CanData.TopIndex = CanData.Items.Count - 1;
            }
        }

        private void ReadCANData()
        {
            TPCANMsg message;
            TPCANTimestamp timestamp;

            // 큐를 비워 기존 메시지를 삭제
            //PCANBasic.Reset(canHandle);

            DateTime lastReceivedTime = DateTime.Now;

            while (isRunning)
            {
                TPCANStatus status = PCANBasic.Read(canHandle, out message, out timestamp);
                if (status == TPCANStatus.PCAN_ERROR_OK)
                {
                    int period = (int)(DateTime.Now - lastReceivedTime).TotalMilliseconds;
                    lastReceivedTime = DateTime.Now;

                    double currentValue = DecodeCurrentValue(message);
                    
                    UpdateCurrentValue(currentValue);
                    UpdateCANData(message, period);
                }
                Thread.Sleep(100);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeCAN();
            isRunning = true;

            canReadThread = new Thread(ReadCANData)
            {
                IsBackground = true
            };
            canReadThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isRunning = false;
            if (canReadThread != null && canReadThread.IsAlive)
            {
                canReadThread.Join(); // 스레드 종료 대기
            }
            PCANBasic.Uninitialize(canHandle); // CAN 종료
        }
    }
}
