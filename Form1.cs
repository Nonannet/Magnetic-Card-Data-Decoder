using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using WaveReader;

namespace Magnetic_Card_Reader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int yVal;
            int lastyVal = 0;
            int currSide = 1;
            bool currStateHi = false;

            MagneticCardReader mcReader = new MagneticCardReader();
            Stream inpFile = new StreamReader(textBox1.Text).BaseStream;
            WaveData wavData = new WaveData(inpFile);

            for (int i = 0; i < wavData.NumberOfFrames; i++)
            {
                yVal = wavData.Samples[0][i];
                if (yVal > 0x7FFF) yVal -= 0xFFFF;

                //Call addNewSignalState on zero crossing:
                if (((lastyVal < yVal && currSide < 0) || (lastyVal > yVal && currSide > 0)) && (System.Math.Abs(yVal) > 0xFF)) 
                {
                    currSide = 0;
                    currStateHi = (yVal > 0xFF);

                    mcReader.addNewSignalState(i, Convert.ToInt32(currStateHi));
                }

                if (lastyVal * yVal < 1)
                {
                    currSide = Convert.ToInt32(!currStateHi) * 2 - 1;
                }

                lastyVal = yVal;
            }

            textBox2.Text = mcReader.getDataString();
            inpFile.Close();
        }

    }
}
