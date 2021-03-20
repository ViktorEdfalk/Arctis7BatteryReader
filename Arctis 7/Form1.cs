﻿using Arctis_7.Properties;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Resources;
using System.Windows.Forms;

namespace Arctis_7
{
    public partial class Form1 : Form
    {
        private Arctis7Reader reader;

        private string batteryPercentage;
        private bool muted = false;
        private byte cachedBatPerc = 200;
        public Form1()
        {
            InitializeComponent();
            reader = new Arctis7Reader();
            timer1.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1_Tick(null, null);
        }

        private void SetIcon(byte percentage)
        {
            if (percentage >= 100)
                percentage = 99;


            if (percentage != cachedBatPerc)
            {
                notifyIconArtics7.Icon = GenerateIcon(percentage);
                cachedBatPerc = percentage;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                byte batCharge = 0;
                if (reader.ReadBattery(out batCharge) && batCharge != 0)
                {
                    batteryPercentage = batCharge.ToString();
                    reader.ReadMute(out byte muteState);
                    muted = (muteState == 1 ? true : false);
                    SetIcon(batCharge);
                    SetBalloonText(batCharge);
                }
                else
                {
                    if (batCharge != cachedBatPerc)
                    {
                        SetNotFoundIcon(batCharge);
                        SetNotFoundBalloonText();
                        cachedBatPerc = batCharge;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetNotFoundBalloonText()
        {
            notifyIconArtics7.Text = "No Arctis 7 Device Found!";
        }

        private void SetNotFoundIcon(byte percentage)
        {
            notifyIconArtics7.Icon = GenerateNotFoundIcon();
            SetNotFoundBalloonText();
        }

        private Icon GenerateNotFoundIcon()
        {
            Bitmap flag = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(flag))
            {
                RectangleF rectf = new RectangleF(5, 0, 60, 60);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.FillRectangle(new SolidBrush(Color.Red), 0, 0, 32, 32);
                g.DrawString("x", new Font("Tahoma", 18, FontStyle.Bold), new SolidBrush(Color.Black), rectf);

            }

            return Icon.FromHandle(flag.GetHicon());
        }

        private Icon GenerateIcon(byte percentage)
        {
            Bitmap flag = new Bitmap(32, 32);
            Color color = Color.White;

            using (var g = Graphics.FromImage(flag))
            {
                if (percentage <= 24)
                    color = Properties.Settings.Default.lowColor;
                else if (percentage >= 25 && percentage <= 49)
                    color = Properties.Settings.Default.mediumLowColor;
                else if (percentage >= 50 && percentage <= 74)
                    color = Properties.Settings.Default.mediumHighColor;
                else if (percentage >= 75)
                    color = Properties.Settings.Default.highColor;

                if (Properties.Settings.Default.UseBackground)
                {
                    g.FillRectangle(new SolidBrush(color), 0, 0, 32, 32);
                }

                RectangleF rectf = new RectangleF(-2, 0, 60, 60);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                if (Properties.Settings.Default.ShowPercentage)
                {
                    g.DrawString(percentage.ToString(), new Font("Tahoma", 18, FontStyle.Bold), new SolidBrush(Properties.Settings.Default.PercentageColor), rectf);
                }
            }

            return Icon.FromHandle(flag.GetHicon());
        }

        private void SetBalloonText(byte bt)
        {
            string mutedStr = (muted ? "Yes \n" : "No \n");
            string battPerc = (bt >= 100 ? "99" : bt.ToString());

            notifyIconArtics7.Text = "Arctis 7 Info \n" +
                "Muted: " + mutedStr +
                "Battery: " + battPerc + "%";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sett = new Settings();
            sett.ShowDialog();
            cachedBatPerc = 200;
            ReloadReader();
            timer1_Tick(null, null);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Made by Viheiser! date: 4/20 time: 13:37 8) \n" +
                "Couldn't have made this if it wasn't for crazyklatsch & MightyDevices \n" +
                "SHOUT OUT TO THEIR FAMILIES! \n" +
                "Also Thanks Sam for testing this and reporting bugs! \nCurrentVersion: 0.02", "About A7 BatteryReader", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReloadReader();
        }

        private void ReloadReader()
        {
            timer1.Stop();
            reader = null;


            reader = new Arctis7Reader();
            timer1.Start();
        }
    }
}
