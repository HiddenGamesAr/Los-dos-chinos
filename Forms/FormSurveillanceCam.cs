using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Los_dos_chinos.OtherForms
{
    public partial class FormSurveillanceCam : Form
    {
        List<ComboBox> comboBoxes;
        public FormSurveillanceCam() { InitializeComponent(); comboBoxes = new() { cmbBCam1, cmbBCam2, cmbBCam3, cmbBCam4 }; }
        FilterInfoCollection FilterInfoCollection;
        VideoCaptureDevice captureDevice1, captureDevice2, captureDevice3, captureDevice4;
        List<PictureBox> pictureBoxes;
        List<VideoCaptureDevice> captureDevices;

        #region FormLoad and FormClosing
        void FormSurveillanceCam_Load (object sender, EventArgs e)
        {
            pictureBoxes = new() {pictureBox1, pictureBox2, pictureBox3, pictureBox4 };
            captureDevices = new() { captureDevice1, captureDevice2, captureDevice3, captureDevice4 };
            FilterInfoCollection = new(FilterCategory.VideoInputDevice);
            if (FilterInfoCollection.Count > 0)
            {
                foreach (var cmbBCam in comboBoxes) // A cada cmbBox se le añade las camaras disponibles
                {
                    foreach (FilterInfo cam in FilterInfoCollection)
                    {
                        cmbBCam.Items.Add(cam.Name);
                    }
                }
            }
            //checkBox1.Checked = true;
        }
        void FormSurveillanceCam_FormClosing (object sender, FormClosingEventArgs e)
        {
            foreach (var capDev in captureDevices) { if (capDev != null && capDev.IsRunning) capDev.Stop(); }
        }
        #endregion

        #region CaptureDevices_NewFrame
        void CaptureDevice1_NewFrame (object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        void CaptureDevice2_NewFrame (object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox2.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        void CaptureDevice3_NewFrame (object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox3.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        void CaptureDevice4_NewFrame (object sender, NewFrameEventArgs eventArgs)
        {
            pictureBox4.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        #endregion

        #region ComboBox SeletedValueChanged and CamerasCheckBox
        void cmbBCam1_SelectedValueChanged (object sender, EventArgs e) { checkBC1.Checked = false; checkBC1.Checked = true; }
        void cmbBCam2_SelectedValueChanged (object sender, EventArgs e) { checkBC2.Checked = false; checkBC2.Checked = true; }
        void cmbBCam3_SelectedValueChanged (object sender, EventArgs e) { checkBC3.Checked = false; checkBC4.Checked = true; }
        void cmbBCam4_SelectedValueChanged (object sender, EventArgs e) { checkBC4.Checked = false; checkBC4.Checked = true; }

        void checkBC1_CheckedChanged (object sender, EventArgs e) { CaptureCamra(1, cmbBCam1, checkBC1, CaptureDevice1_NewFrame); }
        void checkBC2_CheckedChanged (object sender, EventArgs e) { CaptureCamra(2, cmbBCam2, checkBC2, CaptureDevice2_NewFrame); }
        void checkBC3_CheckedChanged(object sender, EventArgs e) { { CaptureCamra(3, cmbBCam3, checkBC3, CaptureDevice3_NewFrame); } }
        void checkBC4_CheckedChanged (object sender, EventArgs e) { { CaptureCamra(4, cmbBCam4, checkBC4, CaptureDevice4_NewFrame); } }
        #endregion

        void CaptureCamra (int captureDev, ComboBox cmbB, CheckBox checkBC, NewFrameEventHandler eventHandler)
        {
            if (checkBC.Checked && cmbB.SelectedIndex != -1 && FilterInfoCollection.Count > 0)
            {
                //if (captureDevices[captureDev - 1] != null && captureDevices[captureDev - 1].IsRunning) captureDevices[captureDev - 1].WaitForStop();
                if(captureDevices[captureDev - 1] == null)
                {
                    captureDevices[captureDev - 1] = new VideoCaptureDevice(FilterInfoCollection[cmbB.SelectedIndex].MonikerString);
                    captureDevices[captureDev - 1].NewFrame += eventHandler;
                    captureDevices[captureDev - 1].Start();
                }
                //return;
            }
            else
            {
                if(captureDevices[captureDev - 1] != null)
                {
                    captureDevices[captureDev - 1].NewFrame -= eventHandler;
                    captureDevices[captureDev - 1].SignalToStop();
                    pictureBoxes[(captureDev - 1)].Image = null;
                    captureDevices[captureDev - 1] = null;
                }
            }
        }
    }
}
