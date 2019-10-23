using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

using FTD2XX_NET;

namespace Prog24_2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            //normal();
            lassanJo();
            //showOne();

            Console.ReadKey();

        }

        static void showOne()
        {
            FTDI ftdi_handle = new FTDI();
            FTDI.FT_STATUS ft_status;
            uint devcount = 0;
            ft_status = ftdi_handle.GetNumberOfDevices(ref devcount);
            if (ft_status != FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Error getting number of devices.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("FTDI devices connected: {0}", devcount);
            if (devcount == 0) return;
            FTDI.FT_DEVICE_INFO_NODE[] ft_dev_list = new FTDI.FT_DEVICE_INFO_NODE[devcount];
            ft_status = ftdi_handle.GetDeviceList(ft_dev_list);
            var serial_numbers = ft_dev_list.Select(h => h.SerialNumber);
            foreach (string st in serial_numbers) if (st != null && st.CompareTo("") != 0)
                    Console.WriteLine("Serial: {0}", st);

            string def_serial = serial_numbers.FirstOrDefault();
            if (def_serial == null || def_serial.CompareTo("") == 0)
            {
                Console.WriteLine("Error getting device serial");
                Console.ReadKey();
                return;
            }
            ft_status = ftdi_handle.OpenBySerialNumber(def_serial);

            ft_status = ftdi_handle.SetBaudRate(1);
            ft_status = ftdi_handle.SetBitMode(0x07, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);

            uint buf = 0;

            int numberOfBytes;
            byte[] outb = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7};

            int idx = 0;

            do
            {
                idx = (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
                ftdi_handle.Write(outb, idx+1, ref buf);
            } while (idx < 8);

            ftdi_handle.Close();
        }

        static void lassanJo() {
            FTDI ftdi_handle = new FTDI();
            FTDI.FT_STATUS ft_status;
            uint devcount = 0;
            ft_status = ftdi_handle.GetNumberOfDevices(ref devcount);
            if (ft_status != FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Error getting number of devices.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("FTDI devices connected: {0}", devcount);
            if (devcount == 0) return;
            FTDI.FT_DEVICE_INFO_NODE[] ft_dev_list = new FTDI.FT_DEVICE_INFO_NODE[devcount];
            ft_status = ftdi_handle.GetDeviceList(ft_dev_list);
            var serial_numbers = ft_dev_list.Select(h => h.SerialNumber);
            foreach (string st in serial_numbers) if (st != null && st.CompareTo("") != 0)
                    Console.WriteLine("Serial: {0}", st);

            string def_serial = serial_numbers.FirstOrDefault();
            if (def_serial == null || def_serial.CompareTo("") == 0)
            {
                Console.WriteLine("Error getting device serial");
                Console.ReadKey();
                return;
            }
            ft_status = ftdi_handle.OpenBySerialNumber(def_serial);

            ft_status = ftdi_handle.SetBaudRate(1);
            ft_status = ftdi_handle.SetBitMode(0x07, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);

            uint buf = 0;

            int numberOfBytes;
            byte[] outb;

            if (false) // True: random, False: Fájl
            {

                Random rnd = new Random();
                numberOfBytes = 400;
                outb = new byte[numberOfBytes];
                for (int i = 0; i < numberOfBytes; i++)
                {
                    outb[i] = (byte)rnd.Next(0, 8);
                }
            }
            else
            {
                //outb = File.ReadAllBytes(".\\PROBA_FoldszintiMikro");
                outb = File.ReadAllBytes(".\\1550_fm_1");
                numberOfBytes = outb.Length;
            }

            ftdi_handle.Write(new byte[] { 7 }, 1, ref buf);
            Console.WriteLine("Press a button to Start");
            Console.ReadKey();
            ftdi_handle.Write(new byte[] { 0 }, 1, ref buf);
            Thread.Sleep(170);
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Stopwatch refresher = new Stopwatch();
            refresher.Start();
            long t = 0;
            int idx = 1;
            for (int i = 1; i <= numberOfBytes-2; i+=3)
            {
                int big = outb[i - 1]*256*256 + outb[i]*256 + outb[i + 1];

                string s = Convert.ToString(big, 2); //Convert to binary in a string

                int[] mybits = s.PadLeft(24, '0') // Add 0's from left
                             .Select(c => int.Parse(c.ToString())) // convert each char to int
                             .ToArray(); // Convert IEnumerable from select to Array

                //Console.WriteLine("{0}, {1}, {2} - " + bits, outb[i - 1], outb[i], outb[i + 1]);
                for(int j = 0; j < 24; j+=3)
                {
                    byte x = (byte)(mybits[j] * 4 + mybits[j + 1] * 2 + mybits[j + 2]);
                    ftdi_handle.Write(new byte[] { x }, 1, ref buf);
                    Console.WriteLine("{0}{1}{2}", mybits[j], mybits[j + 1], mybits[j + 2]);
                    t = refresher.ElapsedMilliseconds;
                    int timeToSleep = (idx++ * 1000 / 35) - (int)t;
                    Thread.Sleep(timeToSleep);
                }
                //Console.WriteLine("{0}", outb[i]);
            }
            refresher.Stop();
            stopwatch.Stop();
            long elapsed = stopwatch.ElapsedMilliseconds;
            Console.WriteLine("Elapsed milliseconds: {0}", elapsed);
            ftdi_handle.Close();

            foreach(byte b in outb)
            {
                //Console.WriteLine("{0}", b);
            }
        }
    }
}
